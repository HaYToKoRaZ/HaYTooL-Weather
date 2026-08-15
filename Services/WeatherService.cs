using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using HaYTooLWeather.Models;

namespace HaYTooLWeather.Services;

/// <summary>
/// Open-Meteo API üzerinden hava durumu ve konum arama işlemlerini yöneten servis.
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
    /// Belirtilen koordinatlara göre hava durumu verilerini çeker ve işler.
    /// </summary>
    public async Task<ProcessedWeatherData?> GetWeatherAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var latStr = settings.Latitude.ToString("F4", CultureInfo.InvariantCulture);
            var lngStr = settings.Longitude.ToString("F4", CultureInfo.InvariantCulture);

            var tempUnitParam = settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase) ? "&temperature_unit=fahrenheit" : "";
            var windUnitParam = settings.WindSpeedUnit.Equals("mph", StringComparison.OrdinalIgnoreCase) ? "&wind_speed_unit=mph" : (settings.WindSpeedUnit.Equals("ms", StringComparison.OrdinalIgnoreCase) ? "&wind_speed_unit=ms" : "");

            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lngStr}&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,wind_speed_10m&hourly=temperature_2m,weather_code&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto{tempUnitParam}{windUnitParam}";

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
}
