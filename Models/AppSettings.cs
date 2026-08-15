namespace HaYTooLWeather.Models;

/// <summary>
/// Uygulama yapılandırma ayarlarını temsil eden model sınıfı.
/// </summary>
public class AppSettings
{
    // Genel Ayarlar
    public string Language { get; set; } = "tr"; // tr, en, es, de, pt, ar, ru
    public string Theme { get; set; } = "auto"; // auto, dark, light
    public bool StartWithWindows { get; set; } = false;

    // Hava Durumu ve Konum Ayarları
    public string City { get; set; } = "Istanbul";
    public string District { get; set; } = "";
    public string Country { get; set; } = "Turkey";
    public double Latitude { get; set; } = 41.0082;
    public double Longitude { get; set; } = 28.9784;
    public int UpdateIntervalHours { get; set; } = 6; // Varsayılan 6 saat
    public string TemperatureUnit { get; set; } = "celsius"; // celsius, fahrenheit
    public string WindSpeedUnit { get; set; } = "kmh"; // kmh, mph, ms

    // Sistem Tepsisi Gösterim Modu
    public string TrayDisplayMode { get; set; } = "dual"; // dual, temp_only, weather_only, single_compact

    // Hava Durumu Simgesi Özelleştirmeleri
    public int WeatherIconScale { get; set; } = 100; // %55 - %175
    public string WeatherBgColor { get; set; } = "#000000";
    public int WeatherBgOpacity { get; set; } = 0; // %0 (Şeffaf) - %100

    // Sıcaklık Metni Özelleştirmeleri
    public int TempTextScale { get; set; } = 100; // %55 - %175
    public string TempBgColor { get; set; } = "#000000";
    public int TempBgOpacity { get; set; } = 0; // %0 (Şeffaf) - %100
    public bool HighContrastTrayIcon { get; set; } = true;

    // Bildirim Tercihi
    public bool ShowNotificationOnUpdate { get; set; } = false;
}
