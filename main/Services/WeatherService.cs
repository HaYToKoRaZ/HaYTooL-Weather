using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using HaYTooLWeather.Models;

namespace HaYTooLWeather.Services;

/// <summary>
/// Open-Meteo ve MGM (Resmi Türkiye Meteorolojisi) API üzerinden hava durumu ve konum arama işlemlerini yöneten servis.
/// </summary>
public class WeatherService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public ProcessedWeatherData? LastWeatherData { get; private set; }

    static WeatherService()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HaYTooL-Weather/1.0 (Windows NT; by HaYTo)");
        HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.mgm.gov.tr");
        HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.mgm.gov.tr/");
    }

    /// <summary>
    /// Şehir ve ilçe isimlerini tekrarı (örn: Derince İlçesi, Derince İlçesi) engelleyerek temiz ve profesyonel başlık yapar.
    /// </summary>
    public static string FormatLocationTitle(string city, string district)
    {
        city = city?.Trim() ?? "";
        district = district?.Trim() ?? "";

        if (string.IsNullOrEmpty(district)) return city;
        if (string.IsNullOrEmpty(city)) return district;

        // Birebir aynı ise tekrarlama
        if (city.Equals(district, StringComparison.OrdinalIgnoreCase))
            return city;

        // 'İlçesi' veya 'Ilcesi' eklerini temizleyip kontrol et
        string cleanCity = city.Replace("İlçesi", "", StringComparison.OrdinalIgnoreCase)
                               .Replace("Ilcesi", "", StringComparison.OrdinalIgnoreCase).Trim();
        string cleanDistrict = district.Replace("İlçesi", "", StringComparison.OrdinalIgnoreCase)
                                       .Replace("Ilcesi", "", StringComparison.OrdinalIgnoreCase).Trim();

        if (cleanCity.Equals(cleanDistrict, StringComparison.OrdinalIgnoreCase))
            return district;

        return $"{district}, {city}";
    }

    /// <summary>
    /// Verilen arama terimine göre Open-Meteo Geocoding API üzerinden şehir/ilçe arar.
    /// </summary>
    public async Task<List<GeoLocation>> SearchLocationAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<GeoLocation>();

        try
        {
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query.Trim())}&count=15&language={LocalizationService.CurrentLanguage}&format=json";
            var response = await HttpClient.GetFromJsonAsync<GeocodingResponse>(url, cancellationToken);
            return response?.Results ?? new List<GeoLocation>();
        }
        catch
        {
            return new List<GeoLocation>();
        }
    }

    /// <summary>
    /// Ayarlardaki seçili sağlayıcıya (MGM veya Open-Meteo / ECMWF / GFS / DWD) göre hava durumu verilerini çeker ve işler.
    /// </summary>
    public async Task<ProcessedWeatherData?> GetWeatherAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        // 1. Sağlayıcı MGM ise ve konum Türkiye ise MGM'den dene
        if (settings.WeatherProvider.Equals("mgm", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(settings.Country, "Turkey", StringComparison.OrdinalIgnoreCase) || string.Equals(settings.Country, "Türkiye", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(settings.Country)))
        {
            var mgmData = await GetMgmWeatherAsync(settings, cancellationToken);
            if (mgmData != null)
            {
                LastWeatherData = mgmData;
                return mgmData;
            }
        }

        // 2. Open-Meteo ve Seçili Model (ECMWF, GFS, DWD, Auto)
        return await GetOpenMeteoWeatherAsync(settings, cancellationToken);
    }

    /// <summary>
    /// MGM (Meteoroloji Genel Müdürlüğü) web servislerinden resmi Türkiye istasyon verilerini çeker.
    /// </summary>
    private async Task<ProcessedWeatherData?> GetMgmWeatherAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            string searchCity = settings.City.Trim();
            // Türkçe karakter düzeltmesi ve ilçe eşleştirmesi için merkezler sorgusu
            string merkezlerUrl = $"https://servis.mgm.gov.tr/web/merkezler?il={Uri.EscapeDataString(searchCity)}";
            var merkezlerJson = await HttpClient.GetStringAsync(merkezlerUrl, cancellationToken);
            using var docMerkezler = JsonDocument.Parse(merkezlerJson);

            if (docMerkezler.RootElement.ValueKind != JsonValueKind.Array || docMerkezler.RootElement.GetArrayLength() == 0)
                return null;

            // İlçe veya ana merkezi bul
            JsonElement selectedMerkez = docMerkezler.RootElement[0];
            string searchDistrict = settings.District.Trim();

            if (!string.IsNullOrEmpty(searchDistrict))
            {
                foreach (var m in docMerkezler.RootElement.EnumerateArray())
                {
                    if (m.TryGetProperty("ilce", out var ilceProp) &&
                        ilceProp.GetString()?.Contains(searchDistrict, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        selectedMerkez = m;
                        break;
                    }
                }
            }

            int sondurumIstNo = selectedMerkez.TryGetProperty("sondurumIstNo", out var sProp) ? sProp.GetInt32() : 0;
            int gunlukTahminIstNo = selectedMerkez.TryGetProperty("gunlukTahminIstNo", out var gProp) ? gProp.GetInt32() : (selectedMerkez.TryGetProperty("merkezId", out var mProp) ? mProp.GetInt32() : 0);

            if (sondurumIstNo == 0 && gunlukTahminIstNo == 0)
                return null;

            // Anlık Durum Sorgusu
            double temp = 0;
            double feelsLike = 0;
            int humidity = 0;
            double windSpeed = 0;
            int weatherCode = 0;

            if (sondurumIstNo > 0)
            {
                string sondurumUrl = $"https://servis.mgm.gov.tr/web/sondurumlar?istno={sondurumIstNo}";
                var sondurumJson = await HttpClient.GetStringAsync(sondurumUrl, cancellationToken);
                using var docSondurum = JsonDocument.Parse(sondurumJson);

                if (docSondurum.RootElement.ValueKind == JsonValueKind.Array && docSondurum.RootElement.GetArrayLength() > 0)
                {
                    var son = docSondurum.RootElement[0];
                    if (son.TryGetProperty("sicaklik", out var sicProp)) temp = sicProp.GetDouble();
                    if (son.TryGetProperty("hissedilenSicaklik", out var hisProp)) feelsLike = hisProp.GetDouble(); else feelsLike = temp;
                    if (son.TryGetProperty("nem", out var nemProp)) humidity = (int)Math.Round(nemProp.GetDouble());
                    if (son.TryGetProperty("ruzgarHiz", out var ruzProp)) windSpeed = Math.Round(ruzProp.GetDouble(), 1);
                    if (son.TryGetProperty("hadiseKodu", out var hadProp))
                    {
                        weatherCode = MapMgmHadiseCode(hadProp.GetString() ?? "");
                    }
                }
            }

            // Birim Dönüşümleri (Fahrenheit / mph isteniyorsa)
            if (settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase))
            {
                temp = Math.Round((temp * 9 / 5) + 32, 1);
                feelsLike = Math.Round((feelsLike * 9 / 5) + 32, 1);
            }
            if (settings.WindSpeedUnit.Equals("mph", StringComparison.OrdinalIgnoreCase))
            {
                windSpeed = Math.Round(windSpeed * 0.621371, 1);
            }

            var locationTitle = FormatLocationTitle(settings.City, settings.District);

            var processed = new ProcessedWeatherData
            {
                LocationName = locationTitle,
                Temperature = Math.Round(temp, 1),
                ApparentTemperature = Math.Round(feelsLike, 1),
                Humidity = humidity,
                WindSpeed = windSpeed,
                Precipitation = 0,
                WeatherCode = weatherCode,
                IsDay = DateTime.Now.Hour is >= 6 and < 20,
                LastUpdated = DateTime.Now
            };

            // Günlük Tahmin Sorgusu (5 Günlük MGM Tahmini)
            if (gunlukTahminIstNo > 0)
            {
                string gunlukUrl = $"https://servis.mgm.gov.tr/web/tahminler/gunluk?istno={gunlukTahminIstNo}";
                var gunlukJson = await HttpClient.GetStringAsync(gunlukUrl, cancellationToken);
                using var docGunluk = JsonDocument.Parse(gunlukJson);

                if (docGunluk.RootElement.ValueKind == JsonValueKind.Array && docGunluk.RootElement.GetArrayLength() > 0)
                {
                    var gunlukData = docGunluk.RootElement[0];
                    var today = DateTime.Today;

                    for (int i = 1; i <= 5; i++)
                    {
                        double minT = gunlukData.TryGetProperty($"enDusukGun{i}", out var minP) ? minP.GetDouble() : 0;
                        double maxT = gunlukData.TryGetProperty($"enYuksekGun{i}", out var maxP) ? maxP.GetDouble() : 0;
                        string hadise = gunlukData.TryGetProperty($"hadiseGun{i}", out var hadP) ? (hadP.GetString() ?? "") : "";

                        if (settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase))
                        {
                            minT = Math.Round((minT * 9 / 5) + 32, 1);
                            maxT = Math.Round((maxT * 9 / 5) + 32, 1);
                        }

                        int dailyWCode = MapMgmHadiseCode(hadise);

                        processed.DailyForecasts.Add(new ProcessedDailyForecast
                        {
                            Date = today.AddDays(i - 1),
                            WeatherCode = dailyWCode,
                            MinTemp = Math.Round(minT, 1),
                            MaxTemp = Math.Round(maxT, 1),
                            RainChance = (hadise.Contains("Y") || hadise.Contains("K") || hadise.Contains("G")) ? 65 : 0
                        });
                    }
                }
            }

            return processed;
        }
        catch
        {
            return null; // MGM başarısız olursa Open-Meteo'ya fallback yapacak
        }
    }

    /// <summary>
    /// Open-Meteo ve ilgili tahmin modeli (ECMWF, GFS, DWD, Auto) üzerinden hava verilerini çeker.
    /// </summary>
    private async Task<ProcessedWeatherData?> GetOpenMeteoWeatherAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var latStr = settings.Latitude.ToString("F4", CultureInfo.InvariantCulture);
            var lngStr = settings.Longitude.ToString("F4", CultureInfo.InvariantCulture);

            var tempUnitParam = settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase) ? "&temperature_unit=fahrenheit" : "";
            var windUnitParam = settings.WindSpeedUnit.Equals("mph", StringComparison.OrdinalIgnoreCase) ? "&wind_speed_unit=mph" : (settings.WindSpeedUnit.Equals("ms", StringComparison.OrdinalIgnoreCase) ? "&wind_speed_unit=ms" : "");

            // Model Parametresi (ECMWF, GFS, DWD, Auto)
            string modelParam = settings.WeatherProvider.ToLowerInvariant() switch
            {
                "ecmwf" => "&models=ecmwf_ifs025",
                "gfs" => "&models=gfs_seamless",
                "dwd" => "&models=icon_seamless",
                _ => "" // Auto / Best Match
            };

            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lngStr}&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,wind_speed_10m&hourly=temperature_2m,weather_code&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto{tempUnitParam}{windUnitParam}{modelParam}";

            var response = await HttpClient.GetFromJsonAsync<WeatherApiResponse>(url, cancellationToken);
            if (response?.Current == null)
                return LastWeatherData;

            var locationTitle = FormatLocationTitle(settings.City, settings.District);

            var processed = new ProcessedWeatherData
            {
                LocationName = locationTitle,
                Temperature = Math.Round(response.Current.Temperature, 1),
                ApparentTemperature = Math.Round(response.Current.ApparentTemperature, 1),
                Humidity = response.Current.RelativeHumidity,
                WindSpeed = Math.Round(response.Current.WindSpeed, 1),
                Precipitation = response.Current.Precipitation,
                WeatherCode = response.Current.WeatherCode,
                IsDay = response.Current.IsDay == 1,
                LastUpdated = DateTime.Now
            };

            // Günlük tahminleri işle (7 gün)
            if (response.Daily?.Time != null && response.Daily.WeatherCode != null)
            {
                for (int i = 0; i < response.Daily.Time.Count && i < 7; i++)
                {
                    if (DateTime.TryParse(response.Daily.Time[i], out var date))
                    {
                        processed.DailyForecasts.Add(new ProcessedDailyForecast
                        {
                            Date = date,
                            WeatherCode = response.Daily.WeatherCode.ElementAtOrDefault(i),
                            MinTemp = Math.Round(response.Daily.TemperatureMin?.ElementAtOrDefault(i) ?? 0, 1),
                            MaxTemp = Math.Round(response.Daily.TemperatureMax?.ElementAtOrDefault(i) ?? 0, 1),
                            RainChance = response.Daily.PrecipitationProbabilityMax?.ElementAtOrDefault(i) ?? 0
                        });
                    }
                }
            }

            // Saatlik tahminleri işle (Önümüzdeki 24 saat)
            if (response.Hourly?.Time != null && response.Hourly.Temperature != null)
            {
                var now = DateTime.Now;
                for (int i = 0; i < response.Hourly.Time.Count; i++)
                {
                    if (DateTime.TryParse(response.Hourly.Time[i], out var hourTime))
                    {
                        if (hourTime >= now.AddHours(-1) && processed.HourlyForecasts.Count < 24)
                        {
                            processed.HourlyForecasts.Add(new ProcessedHourlyForecast
                            {
                                Time = hourTime,
                                Temp = Math.Round(response.Hourly.Temperature.ElementAtOrDefault(i), 1),
                                WeatherCode = response.Hourly.WeatherCode?.ElementAtOrDefault(i) ?? 0
                            });
                        }
                    }
                }
            }

            LastWeatherData = processed;
            return processed;
        }
        catch
        {
            return LastWeatherData;
        }
    }

    /// <summary>
    /// MGM Hadise Kodlarını (A, AB, PB, CB, Y, K, GSY vb.) standart WMO Hava Durumu Kodlarına dönüştürür.
    /// </summary>
    public static int MapMgmHadiseCode(string mgmCode)
    {
        return (mgmCode?.Trim().ToUpperInvariant()) switch
        {
            "A" => 0,       // Açık
            "AB" => 1,      // Az Bulutlu
            "PB" => 2,      // Parçalı Bulutlu
            "CB" => 3,      // Çok Bulutlu
            "SIS" => 45,    // Sisli
            "PUS" => 45,    // Puslu
            "DY" => 51,     // Çisenti
            "HY" => 61,     // Hafif Yağmurlu
            "Y" => 63,      // Yağmurlu
            "SY" => 80,     // Sağanak Yağışlı
            "KSY" => 81,    // Kuvvetli Sağanak
            "GSY" => 95,    // Gök Gürültülü Sağanak
            "KGY" => 96,    // Kuvvetli Gök Gürültülü
            "HKY" => 71,    // Hafif Kar Yağışlı
            "KY" => 73,     // Kar Yağışlı
            "YKY" => 75,    // Yoğun Kar
            "K" => 71,      // Karlı
            "R" => 1,       // Rüzgarlı
            "KF" => 95,     // Kum Fırtınası
            _ => 2          // Varsayılan Parçalı Bulutlu
        };
    }
}
