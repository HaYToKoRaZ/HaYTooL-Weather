using System.Globalization;
using System.Text;
using HaYTooLWeather.Models;

namespace HaYTooLWeather.Services;

/// <summary>
/// Exe ile aynı dizindeki HaYTooLWeather.ini dosyasını okuyan ve yöneten taşınabilir ayar yöneticisi.
/// </summary>
public static class ConfigManager
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HaYTooLWeather.ini");
    private static AppSettings? _cachedSettings;

    /// <summary>
    /// Ayarları HaYTooLWeather.ini dosyasından yükler veya yoksa varsayılanları oluşturur.
    /// </summary>
    public static AppSettings LoadSettings()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        var settings = new AppSettings();

        try
        {
            if (!File.Exists(ConfigFilePath))
            {
                SaveSettings(settings);
                _cachedSettings = settings;
                return settings;
            }

            var lines = File.ReadAllLines(ConfigFilePath, Encoding.UTF8);
            string currentSection = string.Empty;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(';') || line.StartsWith('#'))
                    continue;

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1].Trim().ToUpperInvariant();
                    continue;
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0) continue;

                var key = line[..separatorIndex].Trim().ToUpperInvariant();
                var val = line[(separatorIndex + 1)..].Trim();

                switch (currentSection)
                {
                    case "GENERAL":
                        if (key == "LANGUAGE") settings.Language = val;
                        else if (key == "THEME") settings.Theme = val;
                        else if (key == "STARTWITHWINDOWS") settings.StartWithWindows = bool.TryParse(val, out var sw) && sw;
                        else if (key == "AUTOCHECKUPDATES") settings.AutoCheckUpdates = !bool.TryParse(val, out var acu) || acu;
                        break;

                    case "WEATHER":
                        if (key == "CITY") settings.City = string.IsNullOrWhiteSpace(val) ? "Istanbul" : val;
                        else if (key == "DISTRICT") settings.District = val;
                        else if (key == "COUNTRY") settings.Country = val;
                        else if (key == "LATITUDE" && double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat)) settings.Latitude = lat;
                        else if (key == "LONGITUDE" && double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var lng)) settings.Longitude = lng;
                        else if (key == "UPDATEINTERVALHOURS" && int.TryParse(val, out var interval)) settings.UpdateIntervalHours = interval <= 0 ? 6 : interval;
                        else if (key == "TEMPERATUREUNIT") settings.TemperatureUnit = val;
                        else if (key == "WINDSPEEDUNIT") settings.WindSpeedUnit = val;
                        else if (key == "WEATHERPROVIDER") settings.WeatherProvider = string.IsNullOrWhiteSpace(val) ? "auto" : val.ToLowerInvariant();
                        break;

                    case "UI":
                        if (key == "TRAYDISPLAYMODE") settings.TrayDisplayMode = string.IsNullOrWhiteSpace(val) ? "dual" : val;
                        
                        // İkon ve Derece Boyutları
                        else if (key == "WEATHERICONSCALE" && int.TryParse(val, out var wScale)) settings.WeatherIconScale = wScale is >= 40 and <= 250 ? wScale : 100;
                        else if (key == "TEMPTEXTSCALE" && int.TryParse(val, out var tScale)) settings.TempTextScale = tScale is >= 40 and <= 250 ? tScale : 100;
                        else if (key == "TRAYICONSCALE" && int.TryParse(val, out var oldScale)) // Eski ayardan aktarma
                        {
                            settings.WeatherIconScale = oldScale;
                            settings.TempTextScale = oldScale;
                        }

                        // Arka Plan Renk ve Opaklıkları
                        else if (key == "WEATHERBGCOLOR") settings.WeatherBgColor = string.IsNullOrWhiteSpace(val) ? "#000000" : val;
                        else if (key == "WEATHERBGOPACITY" && int.TryParse(val, out var wOp)) settings.WeatherBgOpacity = Math.Clamp(wOp, 0, 100);

                        else if (key == "TEMPBGCOLOR") settings.TempBgColor = string.IsNullOrWhiteSpace(val) ? "#000000" : val;
                        else if (key == "TEMPBGOPACITY" && int.TryParse(val, out var tOp)) settings.TempBgOpacity = Math.Clamp(tOp, 0, 100);

                        else if (key == "HIGHCONTRASTTRAYICON") settings.HighContrastTrayIcon = bool.TryParse(val, out var hc) && hc;
                        else if (key == "SHOWNOTIFICATIONONUPDATE") settings.ShowNotificationOnUpdate = bool.TryParse(val, out var sn) && sn;
                        break;
                }
            }
        }
        catch
        {
            settings = new AppSettings();
        }

        _cachedSettings = settings;
        return settings;
    }

    /// <summary>
    /// Verilen ayarları HaYTooLWeather.ini dosyasına UTF-8 olarak kaydeder.
    /// </summary>
    public static void SaveSettings(AppSettings settings)
    {
        _cachedSettings = settings;

        var sb = new StringBuilder();
        sb.AppendLine("; ==========================================");
        sb.AppendLine("; HaYTooL Weather - Yapılandırma Dosyası");
        sb.AppendLine("; Geliştirici: HaYTo");
        sb.AppendLine("; ==========================================");
        sb.AppendLine();

        sb.AppendLine("[General]");
        sb.AppendLine($"; Dil Seçimi: tr, en, es, de, pt, ar, ru");
        sb.AppendLine($"Language={settings.Language}");
        sb.AppendLine($"; Tema: auto, dark, light");
        sb.AppendLine($"Theme={settings.Theme}");
        sb.AppendLine($"StartWithWindows={settings.StartWithWindows.ToString().ToLowerInvariant()}");
        sb.AppendLine($"AutoCheckUpdates={settings.AutoCheckUpdates.ToString().ToLowerInvariant()}");
        sb.AppendLine();

        sb.AppendLine("[Weather]");
        sb.AppendLine($"; Varsayılan Şehir (Kullanıcı belirtmediğinde Istanbul)");
        sb.AppendLine($"City={settings.City}");
        sb.AppendLine($"District={settings.District}");
        sb.AppendLine($"Country={settings.Country}");
        sb.AppendLine($"Latitude={settings.Latitude.ToString("F4", CultureInfo.InvariantCulture)}");
        sb.AppendLine($"Longitude={settings.Longitude.ToString("F4", CultureInfo.InvariantCulture)}");
        sb.AppendLine($"; Güncelleme Aralığı (Saat cinsinden, varsayılan 6 saat)");
        sb.AppendLine($"UpdateIntervalHours={settings.UpdateIntervalHours}");
        sb.AppendLine($"; Sıcaklık Birimi: celsius, fahrenheit");
        sb.AppendLine($"TemperatureUnit={settings.TemperatureUnit}");
        sb.AppendLine($"; Rüzgar Hızı Birimi: kmh, mph, ms");
        sb.AppendLine($"WindSpeedUnit={settings.WindSpeedUnit}");
        sb.AppendLine($"; Hava Durumu Veri Kaynağı: auto, mgm, ecmwf, gfs, dwd");
        sb.AppendLine($"WeatherProvider={settings.WeatherProvider}");
        sb.AppendLine();

        sb.AppendLine("[UI]");
        sb.AppendLine($"; Tepsi Gösterim Modu: dual (Çift Alan), temp_only (Sadece Derece), weather_only (Sadece İkon), single_compact");
        sb.AppendLine($"TrayDisplayMode={settings.TrayDisplayMode}");
        sb.AppendLine();
        sb.AppendLine($"; Hava Durumu Simgesi Ölçeği (%55 - %175)");
        sb.AppendLine($"WeatherIconScale={settings.WeatherIconScale}");
        sb.AppendLine($"; Hava Durumu Simgesi Arka Plan Rengi (Hex: #000000, #1E1E2E vb.)");
        sb.AppendLine($"WeatherBgColor={settings.WeatherBgColor}");
        sb.AppendLine($"; Hava Durumu Simgesi Arka Plan Opaklığı (0 = Şeffaf, 100 = Tam Opak)");
        sb.AppendLine($"WeatherBgOpacity={settings.WeatherBgOpacity}");
        sb.AppendLine();
        sb.AppendLine($"; Sıcaklık Metni Ölçeği (%55 - %175)");
        sb.AppendLine($"TempTextScale={settings.TempTextScale}");
        sb.AppendLine($"; Sıcaklık Metni Arka Plan Rengi (Hex: #000000, #1E1E2E vb.)");
        sb.AppendLine($"TempBgColor={settings.TempBgColor}");
        sb.AppendLine($"; Sıcaklık Metni Arka Plan Opaklığı (0 = Şeffaf, 100 = Tam Opak)");
        sb.AppendLine($"TempBgOpacity={settings.TempBgOpacity}");
        sb.AppendLine($"; Yüksek Kontrastlı Beyaz Metin: true/false");
        sb.AppendLine($"HighContrastTrayIcon={settings.HighContrastTrayIcon.ToString().ToLowerInvariant()}");
        sb.AppendLine();
        sb.AppendLine($"ShowNotificationOnUpdate={settings.ShowNotificationOnUpdate.ToString().ToLowerInvariant()}");

        try
        {
            File.WriteAllText(ConfigFilePath, sb.ToString(), Encoding.UTF8);
        }
        catch
        {
            // Dosya yazma hatası
        }
    }

    /// <summary>
    /// Windows başlangıcında otomatik başlatma kaydını Registry üzerinde günceller.
    /// </summary>
    public static void SetAutoStartWithWindows(bool enable)
    {
        try
        {
            const string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(runKey, true);
            if (key == null) return;

            string exePath = Application.ExecutablePath;
            if (enable)
            {
                key.SetValue("HaYTooLWeather", $"\"{exePath}\"");
            }
            else
            {
                if (key.GetValue("HaYTooLWeather") != null)
                {
                    key.DeleteValue("HaYTooLWeather");
                }
            }
        }
        catch { }
    }
}
