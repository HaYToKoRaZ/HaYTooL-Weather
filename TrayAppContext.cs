using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using HaYTooLWeather.Forms;
using HaYTooLWeather.Models;
using HaYTooLWeather.Services;

namespace HaYTooLWeather;

/// <summary>
/// Sistem tepsisi (System Tray) yaşam döngüsünü, Çift Alan / Tek Alan gösterimini ve sadeleştirilmiş menüyü yöneten ana bağlam sınıfı.
/// </summary>
public class TrayAppContext : ApplicationContext
{
    private readonly WeatherService _weatherService;
    private AppSettings _settings;
    private readonly System.Windows.Forms.Timer _updateTimer;

    // Sistem Tepsisi Kontrolleri (Çift Alan / Dual Slots)
    private readonly NotifyIcon _notifyIconWeather;
    private readonly NotifyIcon _notifyIconTemp;
    private readonly ContextMenuStrip _contextMenu;

    // GDI+ İkon Handle Takibi
    private IntPtr _currentWeatherHicon = IntPtr.Zero;
    private IntPtr _currentTempHicon = IntPtr.Zero;
    private Icon? _currentWeatherIcon;
    private Icon? _currentTempIcon;

    private WeatherCardForm? _activeWeatherCard;
    private string? _pendingUpdateUrl;

    public TrayAppContext()
    {
        // 1. Ayarları Yükle
        _settings = ConfigManager.LoadSettings();
        LocalizationService.SetLanguage(_settings.Language);

        // 2. Servisleri Başlat
        _weatherService = new WeatherService();

        // 3. Sağ Tık Menüsünü Hazırla
        _contextMenu = new ContextMenuStrip
        {
            Renderer = new DarkMenuRenderer(),
            ShowImageMargin = false
        };

        Icon defaultAppIcon = SystemIcons.Application;
        try
        {
            defaultAppIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        }
        catch { }

        // 4. İki Tepsi Simgesini (Dual NotifyIcons) Başlat
        _notifyIconWeather = new NotifyIcon
        {
            ContextMenuStrip = _contextMenu,
            Icon = defaultAppIcon,
            Visible = true,
            Text = "HaYTooL Weather"
        };
        _notifyIconWeather.MouseClick += OnTrayIconMouseClick;
        _notifyIconWeather.BalloonTipClicked += (s, e) => OpenPendingUpdateUrl();

        _notifyIconTemp = new NotifyIcon
        {
            ContextMenuStrip = _contextMenu,
            Icon = defaultAppIcon,
            Visible = true,
            Text = "HaYTooL Weather"
        };
        _notifyIconTemp.MouseClick += OnTrayIconMouseClick;
        _notifyIconTemp.BalloonTipClicked += (s, e) => OpenPendingUpdateUrl();

        RebuildContextMenu();

        // 5. Güncelleme Zamanlayıcısını Ayarla (Varsayılan 6 saat)
        _updateTimer = new System.Windows.Forms.Timer();
        _updateTimer.Tick += async (s, e) => await RefreshWeatherAsync(silent: true);
        SetTimerInterval(_settings.UpdateIntervalHours);
        _updateTimer.Start();

        // 6. İlk Hava Durumu Sorgulamasını ve Güncelleme Kontrolünü Başlat
        _ = RefreshWeatherAsync(silent: false);
        if (_settings.AutoCheckUpdates) _ = CheckUpdatesOnStartupAsync();
    }

    private void OpenPendingUpdateUrl()
    {
        if (!string.IsNullOrEmpty(_pendingUpdateUrl))
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = _pendingUpdateUrl, UseShellExecute = true });
            }
            catch { }
        }
    }

    private async Task CheckUpdatesOnStartupAsync()
    {
        try
        {
            await Task.Delay(3000); // Başlangıçta 3 saniye sonra arka planda sorgula
            var result = await UpdateService.CheckForUpdatesAsync();
            if (result.IsUpdateAvailable)
            {
                _pendingUpdateUrl = result.ReleaseUrl;
                string title = string.Format(LocalizationService.Get("notify_update_title"), result.LatestVersion);
                string body = string.Format(LocalizationService.Get("notify_update_body"), result.LatestVersion);
                _notifyIconWeather.ShowBalloonTip(7000, title, body, ToolTipIcon.Info);
            }
        }
        catch { }
    }

    private void SetTimerInterval(int hours)
    {
        long ms = hours <= 0 ? 30L * 60 * 1000 : (long)hours * 60 * 60 * 1000;
        if (ms > int.MaxValue) ms = int.MaxValue;
        _updateTimer.Interval = (int)ms;
    }

    private void RebuildContextMenu()
    {
        _contextMenu.Items.Clear();

        // 1. Üst Başlık (Mevcut Durum - Tıklanınca Kartı Açar)
        if (_weatherService.LastWeatherData != null)
        {
            var weather = _weatherService.LastWeatherData;
            var desc = LocalizationService.GetWeatherDescription(weather.WeatherCode);
            var unit = _settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase) ? "°F" : "°C";
            var itemHeader = new ToolStripMenuItem($"🌤️ {weather.LocationName}: {weather.Temperature:0}{unit} - {desc}")
            {
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 255)
            };
            itemHeader.Click += (s, e) => ToggleWeatherCard();
            _contextMenu.Items.Add(itemHeader);
            _contextMenu.Items.Add(new ToolStripSeparator());
        }

        // 2. Şimdi Yenile
        var itemRefresh = new ToolStripMenuItem("🔄 " + LocalizationService.Get("menu_refresh_now"));
        itemRefresh.Click += async (s, e) => await RefreshWeatherAsync(silent: false);
        _contextMenu.Items.Add(itemRefresh);

        // 3. Ayarlar & Kontrol Merkezi
        var itemSettings = new ToolStripMenuItem("⚙️ " + LocalizationService.Get("menu_settings"));
        itemSettings.Click += (s, e) => OpenControlCenter("location");
        _contextMenu.Items.Add(itemSettings);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // 4. Windows Başlangıcında Çalıştır (Net Görsel Durumlu & Anında Yenilenen)
        var autoStartIcon = _settings.StartWithWindows ? "✅" : "⬜";
        var autoStartBadge = _settings.StartWithWindows ? LocalizationService.Get("status_on") : LocalizationService.Get("status_off");
        var itemAutoStart = new ToolStripMenuItem($"{autoStartIcon} 🚀 {LocalizationService.Get("gen_autostart")}  {autoStartBadge}")
        {
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            ForeColor = _settings.StartWithWindows ? Color.FromArgb(0, 230, 160) : Color.FromArgb(200, 215, 230)
        };
        itemAutoStart.Click += (s, e) =>
        {
            _settings.StartWithWindows = !_settings.StartWithWindows;
            ConfigManager.SetAutoStartWithWindows(_settings.StartWithWindows);
            ConfigManager.SaveSettings(_settings);
            RebuildContextMenu();
        };
        _contextMenu.Items.Add(itemAutoStart);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // 5. Çıkış
        var itemExit = new ToolStripMenuItem("❌ " + LocalizationService.Get("menu_exit"));
        itemExit.Click += (s, e) => ExitApplication();
        _contextMenu.Items.Add(itemExit);
    }

    private async Task RefreshWeatherAsync(bool silent = false)
    {
        try
        {
            if (!silent)
            {
                var text = $"{_settings.City} - {LocalizationService.Get("updating")}";
                _notifyIconWeather.Text = text.Length > 63 ? text[..63] : text;
                _notifyIconTemp.Text = text.Length > 63 ? text[..63] : text;
            }

            var weather = await _weatherService.GetWeatherAsync(_settings);
            if (weather != null)
            {
                UpdateTrayDisplay(weather);
                RebuildContextMenu();
            }
        }
        catch
        {
            // Ağ hatası durumunda mevcut simgeyi koru
        }
    }

    private void UpdateTrayDisplay(ProcessedWeatherData weather)
    {
        var desc = LocalizationService.GetWeatherDescription(weather.WeatherCode);
        var unit = _settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase) ? "°F" : "°C";
        var tooltipText = $"{weather.LocationName}: {weather.Temperature:0}{unit} - {desc}";
        if (tooltipText.Length > 63) tooltipText = tooltipText[..63];

        _notifyIconWeather.Text = tooltipText;
        _notifyIconTemp.Text = tooltipText;

        var mode = _settings.TrayDisplayMode.ToLowerInvariant();

        if (mode == "dual" || string.IsNullOrEmpty(mode))
        {
            // ÇİFT ALAN MODU (Solda Tam Boy İkon, Sağda Dev Sıcaklık)
            _notifyIconWeather.Visible = true;
            _notifyIconTemp.Visible = true;

            // 1. Hava Durumu İkonunu Güncelle
            var (wIcon, wHicon) = IconGenerator.GenerateWeatherOnlyIcon(
                weather.WeatherCode, 
                weather.IsDay, 
                _settings.WeatherIconScale, 
                _settings.WeatherBgColor, 
                _settings.WeatherBgOpacity
            );
            var oldWHicon = _currentWeatherHicon;
            var oldWIcon = _currentWeatherIcon;
            _currentWeatherHicon = wHicon;
            _currentWeatherIcon = wIcon;
            _notifyIconWeather.Icon = wIcon;
            oldWIcon?.Dispose();
            if (oldWHicon != IntPtr.Zero) IconGenerator.DestroyIcon(oldWHicon);

            // 2. Dev Sıcaklık İkonunu Güncelle
            var (tIcon, tHicon) = IconGenerator.GenerateTemperatureOnlyIcon(
                weather.Temperature, 
                _settings.HighContrastTrayIcon, 
                _settings.TempTextScale, 
                _settings.TempBgColor, 
                _settings.TempBgOpacity
            );
            var oldTHicon = _currentTempHicon;
            var oldTIcon = _currentTempIcon;
            _currentTempHicon = tHicon;
            _currentTempIcon = tIcon;
            _notifyIconTemp.Icon = tIcon;
            oldTIcon?.Dispose();
            if (oldTHicon != IntPtr.Zero) IconGenerator.DestroyIcon(oldTHicon);
        }
        else if (mode == "temp_only")
        {
            // SADECE DEV SICAKLIK MODU
            _notifyIconWeather.Visible = false;
            _notifyIconTemp.Visible = true;

            var (tIcon, tHicon) = IconGenerator.GenerateTemperatureOnlyIcon(
                weather.Temperature, 
                _settings.HighContrastTrayIcon, 
                _settings.TempTextScale, 
                _settings.TempBgColor, 
                _settings.TempBgOpacity
            );
            var oldTHicon = _currentTempHicon;
            var oldTIcon = _currentTempIcon;
            _currentTempHicon = tHicon;
            _currentTempIcon = tIcon;
            _notifyIconTemp.Icon = tIcon;
            oldTIcon?.Dispose();
            if (oldTHicon != IntPtr.Zero) IconGenerator.DestroyIcon(oldTHicon);
        }
        else if (mode == "weather_only")
        {
            // SADECE HAVA DURUMU İKONU MODU
            _notifyIconTemp.Visible = false;
            _notifyIconWeather.Visible = true;

            var (wIcon, wHicon) = IconGenerator.GenerateWeatherOnlyIcon(
                weather.WeatherCode, 
                weather.IsDay, 
                _settings.WeatherIconScale, 
                _settings.WeatherBgColor, 
                _settings.WeatherBgOpacity
            );
            var oldWHicon = _currentWeatherHicon;
            var oldWIcon = _currentWeatherIcon;
            _currentWeatherHicon = wHicon;
            _currentWeatherIcon = wIcon;
            _notifyIconWeather.Icon = wIcon;
            oldWIcon?.Dispose();
            if (oldWHicon != IntPtr.Zero) IconGenerator.DestroyIcon(oldWHicon);
        }
        else
        {
            // TEK ALAN KOMPAKT MODU (Solda Küçük İkon + Sağda Derece)
            _notifyIconTemp.Visible = false;
            _notifyIconWeather.Visible = true;

            var (cIcon, cHicon) = IconGenerator.GenerateCompactIcon(
                weather.Temperature, 
                weather.WeatherCode, 
                weather.IsDay, 
                _settings.HighContrastTrayIcon, 
                _settings.TempTextScale, 
                _settings.TempBgColor, 
                _settings.TempBgOpacity
            );
            var oldWHicon = _currentWeatherHicon;
            var oldWIcon = _currentWeatherIcon;
            _currentWeatherHicon = cHicon;
            _currentWeatherIcon = cIcon;
            _notifyIconWeather.Icon = cIcon;
            oldWIcon?.Dispose();
            if (oldWHicon != IntPtr.Zero) IconGenerator.DestroyIcon(oldWHicon);
        }
    }

    private void OnTrayIconMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ToggleWeatherCard();
        }
    }

    private void ToggleWeatherCard()
    {
        if (_activeWeatherCard != null && !_activeWeatherCard.IsDisposed)
        {
            _activeWeatherCard.Close();
            _activeWeatherCard = null;
            return;
        }

        if (_weatherService.LastWeatherData != null)
        {
            _activeWeatherCard = new WeatherCardForm(_weatherService.LastWeatherData, _settings, async () =>
            {
                await RefreshWeatherAsync(silent: false);
            });
            _activeWeatherCard.Show();
            _activeWeatherCard.Activate();
        }
        else
        {
            _ = RefreshWeatherAsync(silent: false);
        }
    }

    private void OpenControlCenter(string initialTab = "location")
    {
        var currentLat = _settings.Latitude;
        var currentLng = _settings.Longitude;
        var currentCity = _settings.City;
        var currentProvider = _settings.WeatherProvider;

        using var settingsForm = new SettingsForm(_weatherService, _settings, async savedSettings =>
        {
            bool locationChanged = (Math.Abs(currentLat - savedSettings.Latitude) > 0.0001 || 
                                    Math.Abs(currentLng - savedSettings.Longitude) > 0.0001 || 
                                    !currentCity.Equals(savedSettings.City, StringComparison.OrdinalIgnoreCase));
            bool providerChanged = !currentProvider.Equals(savedSettings.WeatherProvider, StringComparison.OrdinalIgnoreCase);

            _settings = savedSettings;
            SetTimerInterval(_settings.UpdateIntervalHours);

            if (locationChanged || providerChanged)
            {
                currentLat = _settings.Latitude;
                currentLng = _settings.Longitude;
                currentCity = _settings.City;
                currentProvider = _settings.WeatherProvider;
                await RefreshWeatherAsync(silent: false);
            }
            else if (_weatherService.LastWeatherData != null)
            {
                UpdateTrayDisplay(_weatherService.LastWeatherData);
                RebuildContextMenu();
            }
            else
            {
                await RefreshWeatherAsync(silent: false);
            }
        }, initialTab);

        settingsForm.ShowDialog();
    }

    private void ExitApplication()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();

        _notifyIconWeather.Visible = false;
        _notifyIconWeather.Dispose();

        _notifyIconTemp.Visible = false;
        _notifyIconTemp.Dispose();

        if (_currentWeatherHicon != IntPtr.Zero) IconGenerator.DestroyIcon(_currentWeatherHicon);
        if (_currentTempHicon != IntPtr.Zero) IconGenerator.DestroyIcon(_currentTempHicon);
        _currentWeatherIcon?.Dispose();
        _currentTempIcon?.Dispose();

        Application.Exit();
    }
}

/// <summary>
/// Sağ tık menüsü için koyu temalı zarif render edici.
/// </summary>
public class DarkMenuRenderer : ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkColorTable()) { }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (!e.Item.Selected)
        {
            using var brush = new SolidBrush(Color.FromArgb(28, 31, 40));
            e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
        }
        else
        {
            using var brush = new SolidBrush(Color.FromArgb(0, 122, 255));
            e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
        }
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Selected ? Color.White : (e.Item.Font?.Bold == true ? Color.FromArgb(0, 200, 255) : Color.FromArgb(225, 235, 245));
        base.OnRenderItemText(e);
    }
}

public class DarkColorTable : ProfessionalColorTable
{
    public override Color ToolStripDropDownBackground => Color.FromArgb(28, 31, 40);
    public override Color ImageMarginGradientBegin => Color.FromArgb(28, 31, 40);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(28, 31, 40);
    public override Color ImageMarginGradientEnd => Color.FromArgb(28, 31, 40);
    public override Color MenuBorder => Color.FromArgb(50, 56, 72);
    public override Color MenuItemBorder => Color.Transparent;
    public override Color SeparatorDark => Color.FromArgb(50, 56, 72);
    public override Color SeparatorLight => Color.Transparent;
}
