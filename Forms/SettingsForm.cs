using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32;
using HaYTooLWeather.Models;
using HaYTooLWeather.Services;

namespace HaYTooLWeather.Forms;

/// <summary>
/// HaYTooL Weather Kontrol Merkezi - UI/UX Pro Max Standartlarında Modern Masaüstü Arayüzü.
/// Glassmorphism kartlar, 240px Sidebar, mikro-etkileşimler, akıcı düzen ve canlı görev çubuğu simülasyonu.
/// </summary>
public class SettingsForm : Form
{
    private readonly WeatherService _weatherService;
    private readonly AppSettings _settings;
    private readonly Action<AppSettings> _onSettingsSaved;
    private bool _isInitializing = true;

    // Design Tokens (UI/UX Pro Max - Dark Glass System)
    private static readonly Color ColorBgMain = Color.FromArgb(11, 14, 22);
    private static readonly Color ColorBgSidebar = Color.FromArgb(17, 21, 32);
    private static readonly Color ColorBgCard = Color.FromArgb(20, 26, 40);
    private static readonly Color ColorBgCardHover = Color.FromArgb(26, 34, 52);
    private static readonly Color ColorBgInput = Color.FromArgb(14, 18, 28);
    private static readonly Color ColorBorder = Color.FromArgb(34, 44, 66);
    private static readonly Color ColorBorderFocus = Color.FromArgb(0, 180, 255);
    private static readonly Color ColorAccent = Color.FromArgb(0, 140, 255);
    private static readonly Color ColorAccentCyan = Color.FromArgb(0, 210, 255);
    private static readonly Color ColorSuccess = Color.FromArgb(16, 185, 129);
    private static readonly Color ColorTextPrimary = Color.FromArgb(245, 248, 255);
    private static readonly Color ColorTextSecondary = Color.FromArgb(150, 168, 195);
    private static readonly Color ColorTextMuted = Color.FromArgb(100, 116, 142);

    // Sidebar Butonları
    private Button _btnTabLocation = null!;
    private Button _btnTabAppearance = null!;
    private Button _btnTabGeneral = null!;
    private Button _btnTabLanguage = null!;
    private Button _btnTabAbout = null!;
    private Button? _currentActiveTabBtn;
    private Button _btnClose = null!;

    // Sekme İçerik Panelleri
    private readonly Panel _pnlContent;
    private Panel? _pnlLocationTab;
    private Panel? _pnlAppearanceTab;
    private Panel? _pnlGeneralTab;
    private Panel? _pnlLanguageTab;
    private Panel? _pnlAboutTab;

    // 1. Konum Sekmesi Kontrolleri
    private TextBox? _txtSearch;
    private ListBox? _lstSearchResults;
    private Label? _lblCurrentLocationName;
    private Label? _lblCurrentLocationCoords;
    private readonly List<GeoLocation> _searchResults = new();

    // 2. Görünüm Sekmesi Kontrolleri
    private ComboBox? _cmbTrayMode;
    private ComboBox? _cmbCityTheme;
    private TrackBar? _tbWeatherScale;
    private Label? _lblWeatherScaleVal;
    private Panel? _pnlWeatherColorPreview;
    private TrackBar? _tbWeatherOpacity;
    private Label? _lblWeatherOpacityVal;

    private TrackBar? _tbTempScale;
    private Label? _lblTempScaleVal;
    private Panel? _pnlTempColorPreview;
    private TrackBar? _tbTempOpacity;
    private Label? _lblTempOpacityVal;
    private CheckBox? _chkHighContrast;
    private PictureBox? _pbLivePreview;

    // 3. Genel Sekmesi Kontrolleri
    private ComboBox? _cmbProvider;
    private ComboBox? _cmbInterval;
    private ComboBox? _cmbTempUnit;
    private ComboBox? _cmbWindUnit;
    private CheckBox? _chkAutoCheckUpdates;

    public SettingsForm(WeatherService weatherService, AppSettings settings, Action<AppSettings> onSettingsSaved, string initialTab = "location")
    {
        _weatherService = weatherService;
        _settings = settings;
        _onSettingsSaved = onSettingsSaved;

        Text = "HaYTooL Weather — " + LocalizationService.Get("settings_title");
        Size = new Size(920, 680);
        MinimumSize = new Size(920, 680);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = ColorBgMain;
        ForeColor = ColorTextPrimary;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        // 1. SOL KENAR ÇUBUĞU (Sidebar: 240px)
        var pnlSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 240,
            BackColor = ColorBgSidebar,
            Padding = new Padding(14, 20, 14, 16)
        };
        pnlSidebar.Paint += (s, e) =>
        {
            // Sağ sınır çizgisi (Subtle divider)
            using var pen = new Pen(ColorBorder, 1);
            e.Graphics.DrawLine(pen, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
        };

        var lblLogo = new Label
        {
            Text = "🌤️ HaYTooL Weather",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
            ForeColor = ColorAccentCyan
        };
        var lblSubtitle = new Label
        {
            Text = "Control Center & Appearance",
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = ColorTextMuted
        };

        var pnlNavButtons = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 20, 0, 0)
        };

        _btnTabLocation = CreateSidebarButton(LocalizationService.Get("tab_location"));
        _btnTabAppearance = CreateSidebarButton(LocalizationService.Get("tab_appearance"));
        _btnTabGeneral = CreateSidebarButton(LocalizationService.Get("tab_general"));
        _btnTabLanguage = CreateSidebarButton(LocalizationService.Get("tab_language"));
        _btnTabAbout = CreateSidebarButton(LocalizationService.Get("tab_about"));

        _btnTabLocation.Click += (s, e) => SwitchTab(_btnTabLocation, _pnlLocationTab!);
        _btnTabAppearance.Click += (s, e) => SwitchTab(_btnTabAppearance, _pnlAppearanceTab!);
        _btnTabGeneral.Click += (s, e) => SwitchTab(_btnTabGeneral, _pnlGeneralTab!);
        _btnTabLanguage.Click += (s, e) => SwitchTab(_btnTabLanguage, _pnlLanguageTab!);
        _btnTabAbout.Click += (s, e) => SwitchTab(_btnTabAbout, _pnlAboutTab!);

        // Doğru yukarıdan aşağıya hiyerarşik sıralama
        pnlNavButtons.Controls.AddRange(new Control[] {
            _btnTabAbout, _btnTabLanguage, _btnTabGeneral, _btnTabAppearance, _btnTabLocation
        });

        pnlSidebar.Controls.Add(pnlNavButtons);
        pnlSidebar.Controls.Add(lblSubtitle);
        pnlSidebar.Controls.Add(lblLogo);

        // 2. ALT BAR (Status & Kapat Butonu)
        var pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            BackColor = Color.FromArgb(14, 18, 28),
            Padding = new Padding(24, 10, 24, 10)
        };
        pnlBottom.Paint += (s, e) =>
        {
            using var pen = new Pen(ColorBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, pnlBottom.Width, 0);
        };

        var lblFooterHint = new Label
        {
            Text = "🛡️ %100 Yerel Ayarlar & Sıfır Telemetri",
            Dock = DockStyle.Left,
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = ColorTextMuted,
            Padding = new Padding(0, 10, 0, 0)
        };

        _btnClose = new Button
        {
            Text = LocalizationService.Get("btn_close"),
            Dock = DockStyle.Right,
            Width = 140,
            BackColor = ColorAccent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        _btnClose.FlatAppearance.BorderSize = 0;
        _btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 160, 255);
        _btnClose.Click += (s, e) => Close();

        pnlBottom.Controls.Add(lblFooterHint);
        pnlBottom.Controls.Add(_btnClose);

        // 3. SAĞ İÇERİK ALANI (Content Area)
        _pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorBgMain,
            Padding = new Padding(24, 18, 24, 16)
        };

        // Sekme Sayfalarını Oluştur
        BuildLocationTab();
        BuildAppearanceTab();
        BuildGeneralTab();
        BuildLanguageTab();
        BuildAboutTab();

        Controls.Add(_pnlContent);
        Controls.Add(pnlBottom);
        Controls.Add(pnlSidebar);

        _isInitializing = false;

        if (initialTab == "appearance")
            SwitchTab(_btnTabAppearance, _pnlAppearanceTab!);
        else
            SwitchTab(_btnTabLocation, _pnlLocationTab!);
    }

    private Button CreateSidebarButton(string text)
    {
        var btn = new Button
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = ColorTextSecondary,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 6)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 36, 54);
        return btn;
    }

    private void SwitchTab(Button activeBtn, Panel targetPanel)
    {
        if (_currentActiveTabBtn != null)
        {
            _currentActiveTabBtn.BackColor = Color.Transparent;
            _currentActiveTabBtn.ForeColor = ColorTextSecondary;
        }

        _currentActiveTabBtn = activeBtn;
        _currentActiveTabBtn.BackColor = ColorAccent;
        _currentActiveTabBtn.ForeColor = Color.White;

        _pnlContent.Controls.Clear();
        _pnlContent.Controls.Add(targetPanel);
        targetPanel.Dock = DockStyle.Fill;
        targetPanel.BringToFront();

        if (targetPanel == _pnlAppearanceTab)
        {
            UpdateLivePreview();
        }
    }

    private void RebuildAllTabsAfterLanguageChange()
    {
        Text = "HaYTooL Weather — " + LocalizationService.Get("settings_title");
        _btnClose.Text = LocalizationService.Get("btn_close");

        _btnTabLocation.Text = LocalizationService.Get("tab_location");
        _btnTabAppearance.Text = LocalizationService.Get("tab_appearance");
        _btnTabGeneral.Text = LocalizationService.Get("tab_general");
        _btnTabLanguage.Text = LocalizationService.Get("tab_language");
        _btnTabAbout.Text = LocalizationService.Get("tab_about");

        BuildLocationTab();
        BuildAppearanceTab();
        BuildGeneralTab();
        BuildLanguageTab();
        BuildAboutTab();

        SwitchTab(_btnTabLanguage, _pnlLanguageTab!);
    }

    // =========================================================================
    // 1. SEKME: 📍 KONUM & ŞEHİR SEÇİMİ
    // =========================================================================
    private void BuildLocationTab()
    {
        _pnlLocationTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 8, 8) };
        int y = 0;
        int contentW = 620;

        var lblHeader = new Label { Text = LocalizationService.Get("loc_title"), Location = new Point(0, y), Size = new Size(contentW, 26), Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = ColorAccentCyan };
        y += 34;

        // Aktif Konum Kartı (Glass Card)
        var pnlCurrentLoc = new Panel { Location = new Point(0, y), Size = new Size(contentW, 68), BackColor = ColorBgCard, Padding = new Padding(16, 10, 16, 10) };
        pnlCurrentLoc.Paint += (s, e) => DrawSubtleBorder(e.Graphics, pnlCurrentLoc.ClientRectangle);

        var lblCurBadge = new Label { Text = LocalizationService.Get("loc_active_badge"), Location = new Point(14, 8), Size = new Size(200, 16), Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = ColorSuccess };
        _lblCurrentLocationName = new Label { Text = $"{WeatherService.FormatLocationTitle(_settings.City, _settings.District)}, {_settings.Country}", Location = new Point(14, 26), Size = new Size(360, 28), Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = ColorTextPrimary };
        _lblCurrentLocationCoords = new Label { Text = $"Koordinat: {_settings.Latitude:F4}°N, {_settings.Longitude:F4}°E", Location = new Point(380, 28), Size = new Size(225, 24), TextAlign = ContentAlignment.TopRight, Font = new Font("Segoe UI", 8.5f), ForeColor = ColorTextSecondary };
        pnlCurrentLoc.Controls.AddRange(new Control[] { lblCurBadge, _lblCurrentLocationName, _lblCurrentLocationCoords });
        y += 78;

        // Popüler Şehirler (7 Dilin Temsilcileri)
        var lblQuick = new Label { Text = LocalizationService.Get("loc_popular"), Location = new Point(0, y), Size = new Size(contentW, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ColorTextSecondary };
        y += 24;

        var flpChips = new FlowLayoutPanel { Location = new Point(0, y), Size = new Size(contentW, 38), BackColor = Color.Transparent, AutoScroll = false };
        (string Query, string Display)[] favoriteCities = {
            ("Istanbul", "🇹🇷 İstanbul"),
            ("London", "🇬🇧 London"),
            ("Berlin", "🇩🇪 Berlin"),
            ("Madrid", "🇪🇸 Madrid"),
            ("Lisbon", "🇵🇹 Lisbon"),
            ("Moscow", "🇷🇺 Moscow"),
            ("Riyadh", "🇸🇦 Riyadh")
        };

        foreach (var (query, display) in favoriteCities)
        {
            var chip = new Button
            {
                Text = display,
                Height = 32,
                AutoSize = true,
                BackColor = query == "Istanbul" ? ColorAccent : ColorBgCard,
                ForeColor = ColorTextPrimary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(10, 0, 10, 0)
            };
            chip.FlatAppearance.BorderSize = 0;
            chip.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 160, 255);
            chip.Click += async (s, e) => await QuickSelectCity(query);
            flpChips.Controls.Add(chip);
        }
        y += 44;

        // Arama Alanı
        var lblSearchTitle = new Label { Text = LocalizationService.Get("loc_search_title"), Location = new Point(0, y), Size = new Size(contentW, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ColorTextSecondary };
        y += 24;

        var pnlSearchBox = new Panel { Location = new Point(0, y), Size = new Size(contentW, 40), BackColor = ColorBgInput };
        pnlSearchBox.Paint += (s, e) => DrawSubtleBorder(e.Graphics, pnlSearchBox.ClientRectangle);

        _txtSearch = new TextBox
        {
            Location = new Point(14, 9),
            Size = new Size(contentW - 130, 24),
            BackColor = ColorBgInput,
            ForeColor = ColorTextPrimary,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10.5f),
            PlaceholderText = LocalizationService.Get("loc_search_placeholder")
        };
        _txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await PerformSearchAsync(); } };

        var btnSearch = new Button
        {
            Text = LocalizationService.Get("loc_search_btn"),
            Location = new Point(contentW - 110, 4),
            Size = new Size(104, 32),
            BackColor = ColorAccent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 160, 255);
        btnSearch.Click += async (s, e) => await PerformSearchAsync();

        pnlSearchBox.Controls.Add(_txtSearch);
        pnlSearchBox.Controls.Add(btnSearch);
        y += 48;

        // Sonuç Listesi
        _lstSearchResults = new ListBox
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 140),
            BackColor = ColorBgCard,
            ForeColor = ColorTextPrimary,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9.5f),
            ItemHeight = 26
        };
        y += 148;

        var lblLocationStatus = new Label
        {
            Text = "",
            Location = new Point(0, y + 6),
            Size = new Size(390, 24),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorSuccess
        };
        _lstSearchResults.DoubleClick += async (s, e) => await ApplySelectedLocationAsync(lblLocationStatus);

        var btnApplyLocation = new Button
        {
            Text = LocalizationService.Get("loc_apply_btn"),
            Location = new Point(contentW - 220, y),
            Size = new Size(220, 36),
            BackColor = ColorSuccess,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnApplyLocation.FlatAppearance.BorderSize = 0;
        btnApplyLocation.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 210, 145);
        btnApplyLocation.Click += async (s, e) => await ApplySelectedLocationAsync(lblLocationStatus);

        _pnlLocationTab.Controls.AddRange(new Control[] {
            lblHeader, pnlCurrentLoc, lblQuick, flpChips, lblSearchTitle, pnlSearchBox, _lstSearchResults, lblLocationStatus, btnApplyLocation
        });
    }

    private async Task QuickSelectCity(string cityName)
    {
        if (_txtSearch != null) _txtSearch.Text = cityName;
        await PerformSearchAsync();
        if (_searchResults.Count > 0)
        {
            _lstSearchResults!.SelectedIndex = 0;
            await ApplySelectedLocationAsync(null);
        }
    }

    private async Task PerformSearchAsync()
    {
        if (_txtSearch == null || string.IsNullOrWhiteSpace(_txtSearch.Text)) return;

        string query = _txtSearch.Text.Trim();
        _lstSearchResults!.Items.Clear();
        _searchResults.Clear();
        _lstSearchResults.Items.Add(LocalizationService.Get("loc_searching"));

        try
        {
            var results = await _weatherService.SearchLocationAsync(query);
            _lstSearchResults.Items.Clear();

            if (results.Count == 0)
            {
                _lstSearchResults.Items.Add(LocalizationService.Get("loc_no_results"));
                return;
            }

            _searchResults.AddRange(results);
            foreach (var loc in results)
            {
                string admin = !string.IsNullOrEmpty(loc.Admin2) ? $"{loc.Admin2}, " : (!string.IsNullOrEmpty(loc.Admin1) ? $"{loc.Admin1}, " : "");
                _lstSearchResults.Items.Add($"📍 {loc.Name}, {admin}{loc.Country} ({loc.Latitude:F2}°, {loc.Longitude:F2}°)");
            }
            _lstSearchResults.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            _lstSearchResults.Items.Clear();
            _lstSearchResults.Items.Add($"Hata: {ex.Message}");
        }
    }

    private async Task ApplySelectedLocationAsync(Label? statusLabel)
    {
        if (_lstSearchResults == null || _lstSearchResults.SelectedIndex < 0 || _lstSearchResults.SelectedIndex >= _searchResults.Count)
            return;

        var selected = _searchResults[_lstSearchResults.SelectedIndex];
        string name = selected.Name?.Trim() ?? "";
        string province = selected.Admin1?.Trim() ?? "";
        string district = selected.Admin2?.Trim() ?? "";

        if (!string.IsNullOrEmpty(province) && !province.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            _settings.City = province;
            _settings.District = name;
        }
        else
        {
            _settings.City = name;
            _settings.District = !string.IsNullOrEmpty(district) && !district.Equals(name, StringComparison.OrdinalIgnoreCase) ? district : "";
        }

        _settings.Country = selected.Country;
        _settings.Latitude = selected.Latitude;
        _settings.Longitude = selected.Longitude;

        if (_lblCurrentLocationName != null)
            _lblCurrentLocationName.Text = $"{WeatherService.FormatLocationTitle(_settings.City, _settings.District)}, {_settings.Country}";
        if (_lblCurrentLocationCoords != null)
            _lblCurrentLocationCoords.Text = $"Koordinat: {_settings.Latitude:F4}°N, {_settings.Longitude:F4}°E";

        if (statusLabel != null)
        {
            string locName = WeatherService.FormatLocationTitle(_settings.City, _settings.District);
            statusLabel.Text = $"⏳ {locName}...";
            statusLabel.ForeColor = ColorAccentCyan;
        }

        ConfigManager.SaveSettings(_settings);
        _onSettingsSaved(_settings);

        try
        {
            var weather = await _weatherService.GetWeatherAsync(_settings);
            if (weather != null && statusLabel != null)
            {
                statusLabel.Text = $"✅ {weather.LocationName}: {weather.Temperature:0}°C - OK!";
                statusLabel.ForeColor = ColorSuccess;
                UpdateLivePreview();
            }
        }
        catch
        {
            if (statusLabel != null)
            {
                string locName = WeatherService.FormatLocationTitle(_settings.City, _settings.District);
                statusLabel.Text = $"✅ {locName}";
            }
        }
    }

    // =========================================================================
    // 2. SEKME: 🎨 GÖRÜNÜM & BOYUT (KAYDIRMASIZ / NO SCROLL)
    // =========================================================================
    private void BuildAppearanceTab()
    {
        _pnlAppearanceTab = new Panel { AutoScroll = false, BackColor = Color.Transparent };
        int y = 0;
        int contentW = 620;

        var lblHeader = new Label { Text = LocalizationService.Get("app_title"), Location = new Point(0, y), Size = new Size(contentW, 26), Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = ColorAccentCyan };
        y += 32;

        // 1. Canlı Görev Çubuğu Önizleme Kutusu (Glass Bar)
        var pnlPreviewBox = new Panel { Location = new Point(0, y), Size = new Size(contentW, 64), BackColor = ColorBgCard, Padding = new Padding(12) };
        pnlPreviewBox.Paint += (s, e) => DrawSubtleBorder(e.Graphics, pnlPreviewBox.ClientRectangle);

        var lblPreviewTitle = new Label { Text = LocalizationService.Get("app_live_preview"), Location = new Point(12, 6), Size = new Size(250, 16), Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = ColorSuccess };
        _pbLivePreview = new PictureBox { Location = new Point(12, 24), Size = new Size(contentW - 24, 34), BackColor = Color.FromArgb(12, 14, 20), BorderStyle = BorderStyle.None };
        _pbLivePreview.Paint += (s, e) => PaintLivePreview(e.Graphics);
        pnlPreviewBox.Controls.Add(lblPreviewTitle);
        pnlPreviewBox.Controls.Add(_pbLivePreview);
        y += 72;

        // 2. Üst Kontrol Satırları (Gösterim Modu & Şehir Teması)
        var pnlDropdowns = new Panel { Location = new Point(0, y), Size = new Size(contentW, 70), BackColor = ColorBgCard, Padding = new Padding(12, 8, 12, 8) };
        pnlDropdowns.Paint += (s, e) => DrawSubtleBorder(e.Graphics, pnlDropdowns.ClientRectangle);

        var lblMode = new Label { Text = LocalizationService.Get("app_mode"), Location = new Point(12, 10), Size = new Size(130, 22), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = ColorTextSecondary };
        _cmbTrayMode = new ComboBox { Location = new Point(145, 8), Size = new Size(contentW - 160, 26), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ColorBgInput, ForeColor = ColorTextPrimary, FlatStyle = FlatStyle.Flat };
        _cmbTrayMode.Items.Add("⭐ Çift Alan (Solda Tam Boy İkon + Sağda Dev Sıcaklık)");
        _cmbTrayMode.Items.Add("🌡️ Sadece Dev Sıcaklık Derecesi (Tek Alan)");
        _cmbTrayMode.Items.Add("🌤️ Sadece Hava Durumu İkonu (Tek Alan)");
        _cmbTrayMode.Items.Add("🔹 Tek Alan Kompakt (Küçük İkon + Derece)");
        _cmbTrayMode.SelectedIndex = _settings.TrayDisplayMode.ToLowerInvariant() switch { "temp_only" => 1, "weather_only" => 2, "single_compact" => 3, _ => 0 };
        _cmbTrayMode.SelectedIndexChanged += (s, e) => ApplyLiveChanges();

        var lblTheme = new Label { Text = "🏙️ Şehir Teması:", Location = new Point(12, 40), Size = new Size(130, 22), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = ColorAccentCyan };
        _cmbCityTheme = new ComboBox { Location = new Point(145, 38), Size = new Size(contentW - 160, 26), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ColorBgInput, ForeColor = ColorTextPrimary, FlatStyle = FlatStyle.Flat };
        foreach (var t in ThemeFactory.CityThemes)
        {
            _cmbCityTheme.Items.Add(t.NameTr);
        }
        int activeThemeIdx = ThemeFactory.CityThemes.FindIndex(t => t.Id.Equals(_settings.Theme, StringComparison.OrdinalIgnoreCase));
        _cmbCityTheme.SelectedIndex = activeThemeIdx >= 0 ? activeThemeIdx : 0;
        _cmbCityTheme.SelectedIndexChanged += (s, e) =>
        {
            if (_cmbCityTheme.SelectedIndex >= 0 && _cmbCityTheme.SelectedIndex < ThemeFactory.CityThemes.Count)
            {
                var selectedTheme = ThemeFactory.CityThemes[_cmbCityTheme.SelectedIndex];
                _settings.Theme = selectedTheme.Id;
                _settings.WeatherBgColor = IconGenerator.ColorToHex(selectedTheme.Surface);
                _settings.TempBgColor = IconGenerator.ColorToHex(selectedTheme.CardBg);
                if (_pnlWeatherColorPreview != null) _pnlWeatherColorPreview.BackColor = selectedTheme.Surface;
                if (_pnlTempColorPreview != null) _pnlTempColorPreview.BackColor = selectedTheme.CardBg;
                ApplyLiveChanges();
            }
        };

        pnlDropdowns.Controls.AddRange(new Control[] { lblMode, _cmbTrayMode, lblTheme, _cmbCityTheme });
        y += 78;

        // 3. YAN YANA 2 SÜTUN KARTLARI (Kusursuz Simetrik Grid)
        int colWidth = (contentW - 14) / 2; // 303px
        int colGap = 14;
        int cardHeight = 310;

        // SOL SÜTUN: HAVA DURUMU SİMGESİ KARTI
        var pnlWeather = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(colWidth, cardHeight),
            BackColor = ColorBgCard,
            Padding = new Padding(16)
        };
        pnlWeather.Paint += (s, e) => DrawSubtleBorder(e.Graphics, pnlWeather.ClientRectangle);

        var lblWTitle = new Label { Text = LocalizationService.Get("app_weather_icon"), Location = new Point(14, 12), Size = new Size(colWidth - 28, 22), ForeColor = Color.FromArgb(255, 215, 0), Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        int gy1 = 40;

        var lblWScale = new Label { Text = LocalizationService.Get("app_icon_size"), Location = new Point(14, gy1), Size = new Size(180, 20), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _lblWeatherScaleVal = new Label { Text = $"%{_settings.WeatherIconScale}", Location = new Point(colWidth - 80, gy1), Size = new Size(66, 20), TextAlign = ContentAlignment.TopRight, ForeColor = Color.FromArgb(255, 220, 100), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy1 += 22;
        _tbWeatherScale = new TrackBar { Location = new Point(10, gy1), Size = new Size(colWidth - 20, 30), Minimum = 40, Maximum = 200, Value = Math.Clamp(_settings.WeatherIconScale, 40, 200), TickStyle = TickStyle.None };
        _tbWeatherScale.Scroll += (s, e) => { _lblWeatherScaleVal.Text = $"%{_tbWeatherScale.Value}"; ApplyLiveChanges(); };
        gy1 += 44;

        var lblWBg = new Label { Text = LocalizationService.Get("app_bg_color"), Location = new Point(14, gy1 + 3), Size = new Size(110, 22), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _pnlWeatherColorPreview = new Panel { Location = new Point(125, gy1), Size = new Size(32, 26), BackColor = IconGenerator.ParseHexColor(_settings.WeatherBgColor), BorderStyle = BorderStyle.FixedSingle };
        var btnWBgColor = new Button { Text = LocalizationService.Get("app_pick_color"), Location = new Point(165, gy1), Size = new Size(colWidth - 180, 26), BackColor = Color.FromArgb(42, 52, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5f) };
        btnWBgColor.FlatAppearance.BorderSize = 0;
        btnWBgColor.Click += (s, e) => PickColor(c => _settings.WeatherBgColor = c, _settings.WeatherBgColor, _pnlWeatherColorPreview);
        gy1 += 48;

        var lblWOp = new Label { Text = LocalizationService.Get("app_bg_opacity"), Location = new Point(14, gy1), Size = new Size(180, 20), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _lblWeatherOpacityVal = new Label { Text = $"%{_settings.WeatherBgOpacity}", Location = new Point(colWidth - 80, gy1), Size = new Size(66, 20), TextAlign = ContentAlignment.TopRight, ForeColor = ColorAccentCyan, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy1 += 22;
        _tbWeatherOpacity = new TrackBar { Location = new Point(10, gy1), Size = new Size(colWidth - 20, 30), Minimum = 0, Maximum = 100, Value = Math.Clamp(_settings.WeatherBgOpacity, 0, 100), TickStyle = TickStyle.None };
        _tbWeatherOpacity.Scroll += (s, e) => { _lblWeatherOpacityVal.Text = $"%{_tbWeatherOpacity.Value}"; ApplyLiveChanges(); };

        pnlWeather.Controls.AddRange(new Control[] { lblWTitle, lblWScale, _lblWeatherScaleVal, _tbWeatherScale, lblWBg, _pnlWeatherColorPreview, btnWBgColor, lblWOp, _lblWeatherOpacityVal, _tbWeatherOpacity });

        // SAĞ SÜTUN: SICAKLIK DERECESİ KARTI
        var pnlTemp = new Panel
        {
            Location = new Point(colWidth + colGap, y),
            Size = new Size(colWidth, cardHeight),
            BackColor = ColorBgCard,
            Padding = new Padding(16)
        };
        pnlTemp.Paint += (s, e) => DrawSubtleBorder(e.Graphics, pnlTemp.ClientRectangle);

        var lblTTitle = new Label { Text = LocalizationService.Get("app_temp_text"), Location = new Point(14, 12), Size = new Size(colWidth - 28, 22), ForeColor = ColorAccentCyan, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        int gy2 = 40;

        var lblTScale = new Label { Text = LocalizationService.Get("app_text_size"), Location = new Point(14, gy2), Size = new Size(180, 20), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _lblTempScaleVal = new Label { Text = $"%{_settings.TempTextScale}", Location = new Point(colWidth - 80, gy2), Size = new Size(66, 20), TextAlign = ContentAlignment.TopRight, ForeColor = ColorAccentCyan, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy2 += 22;
        _tbTempScale = new TrackBar { Location = new Point(10, gy2), Size = new Size(colWidth - 20, 30), Minimum = 40, Maximum = 200, Value = Math.Clamp(_settings.TempTextScale, 40, 200), TickStyle = TickStyle.None };
        _tbTempScale.Scroll += (s, e) => { _lblTempScaleVal.Text = $"%{_tbTempScale.Value}"; ApplyLiveChanges(); };
        gy2 += 44;

        var lblTBg = new Label { Text = LocalizationService.Get("app_bg_color"), Location = new Point(14, gy2 + 3), Size = new Size(110, 22), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _pnlTempColorPreview = new Panel { Location = new Point(125, gy2), Size = new Size(32, 26), BackColor = IconGenerator.ParseHexColor(_settings.TempBgColor), BorderStyle = BorderStyle.FixedSingle };
        var btnTBgColor = new Button { Text = LocalizationService.Get("app_pick_color"), Location = new Point(165, gy2), Size = new Size(colWidth - 180, 26), BackColor = Color.FromArgb(42, 52, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5f) };
        btnTBgColor.FlatAppearance.BorderSize = 0;
        btnTBgColor.Click += (s, e) => PickColor(c => _settings.TempBgColor = c, _settings.TempBgColor, _pnlTempColorPreview);
        gy2 += 48;

        var lblTOp = new Label { Text = LocalizationService.Get("app_bg_opacity"), Location = new Point(14, gy2), Size = new Size(180, 20), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _lblTempOpacityVal = new Label { Text = $"%{_settings.TempBgOpacity}", Location = new Point(colWidth - 80, gy2), Size = new Size(66, 20), TextAlign = ContentAlignment.TopRight, ForeColor = ColorAccentCyan, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy2 += 22;
        _tbTempOpacity = new TrackBar { Location = new Point(10, gy2), Size = new Size(colWidth - 20, 30), Minimum = 0, Maximum = 100, Value = Math.Clamp(_settings.TempBgOpacity, 0, 100), TickStyle = TickStyle.None };
        _tbTempOpacity.Scroll += (s, e) => { _lblTempOpacityVal.Text = $"%{_tbTempOpacity.Value}"; ApplyLiveChanges(); };
        gy2 += 44;

        _chkHighContrast = new CheckBox
        {
            Text = LocalizationService.Get("app_high_contrast"),
            Location = new Point(14, gy2),
            Size = new Size(colWidth - 28, 24),
            Checked = _settings.HighContrastTrayIcon,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 240, 150)
        };
        _chkHighContrast.CheckedChanged += (s, e) => ApplyLiveChanges();

        pnlTemp.Controls.AddRange(new Control[] { lblTTitle, lblTScale, _lblTempScaleVal, _tbTempScale, lblTBg, _pnlTempColorPreview, btnTBgColor, lblTOp, _lblTempOpacityVal, _tbTempOpacity, _chkHighContrast });

        _pnlAppearanceTab.Controls.AddRange(new Control[] {
            lblHeader, pnlPreviewBox, pnlDropdowns, pnlWeather, pnlTemp
        });
    }

    private void PaintLivePreview(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(12, 14, 20));

        // Görev Çubuğu Saati Çizimi
        using var fontClock = new Font("Segoe UI", 9f, FontStyle.Regular);
        using var brushClock = new SolidBrush(ColorTextSecondary);
        string timeStr = DateTime.Now.ToString("HH:mm");
        g.DrawString(timeStr, fontClock, brushClock, 535, 8);

        // Canlı Simge Önizlemesi
        int temp = _weatherService.LastWeatherData != null ? (int)Math.Round(_weatherService.LastWeatherData.Temperature) : 24;
        int weatherCode = _weatherService.LastWeatherData?.WeatherCode ?? 0;
        bool isDay = _weatherService.LastWeatherData?.IsDay ?? true;

        var (wIcon, wHicon) = IconGenerator.GenerateWeatherOnlyIcon(weatherCode, isDay, _settings.WeatherIconScale, _settings.WeatherBgColor, _settings.WeatherBgOpacity);
        var (tIcon, tHicon) = IconGenerator.GenerateTemperatureOnlyIcon(temp, _settings.HighContrastTrayIcon, _settings.TempTextScale, _settings.TempBgColor, _settings.TempBgOpacity);

        g.DrawIcon(wIcon, new Rectangle(450, 2, 30, 30));
        g.DrawIcon(tIcon, new Rectangle(488, 2, 30, 30));

        using var fontLabel = new Font("Segoe UI", 8.5f, FontStyle.Italic);
        using var brushLabel = new SolidBrush(ColorTextMuted);
        g.DrawString("Windows Görev Çubuğu Önizlemesi ➔", fontLabel, brushLabel, 230, 9);

        wIcon.Dispose();
        tIcon.Dispose();
        IconGenerator.DestroyIcon(wHicon);
        IconGenerator.DestroyIcon(tHicon);
    }

    private void UpdateLivePreview()
    {
        _pbLivePreview?.Invalidate();
    }

    // =========================================================================
    // 3. SEKME: ⚙️ GENEL & SİSTEM AYARLARI
    // =========================================================================
    private void BuildGeneralTab()
    {
        _pnlGeneralTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 8, 8) };
        int y = 0;
        int contentW = 620;

        var lblHeader = new Label { Text = LocalizationService.Get("gen_title"), Location = new Point(0, y), Size = new Size(contentW, 26), Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = ColorAccentCyan };
        y += 34;

        // Ayar Kartı
        var pnlCard = new Panel { Location = new Point(0, y), Size = new Size(contentW, 360), BackColor = ColorBgCard, Padding = new Padding(20) };
        pnlCard.Paint += (s, e) => DrawSubtleBorder(e.Graphics, pnlCard.ClientRectangle);
        int cy = 16;

        // Hava Durumu Veri Kaynağı / Model Seçimi
        var lblProvider = new Label { Text = LocalizationService.Get("gen_weather_provider"), Location = new Point(16, cy), Size = new Size(contentW - 32, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ColorTextSecondary };
        cy += 24;
        _cmbProvider = new ComboBox { Location = new Point(16, cy), Size = new Size(contentW - 32, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ColorBgInput, ForeColor = ColorTextPrimary, FlatStyle = FlatStyle.Flat };
        _cmbProvider.Items.Add(new ProviderItem(LocalizationService.Get("provider_auto"), "auto"));
        _cmbProvider.Items.Add(new ProviderItem(LocalizationService.Get("provider_mgm"), "mgm"));
        _cmbProvider.Items.Add(new ProviderItem(LocalizationService.Get("provider_ecmwf"), "ecmwf"));
        _cmbProvider.Items.Add(new ProviderItem(LocalizationService.Get("provider_gfs"), "gfs"));
        _cmbProvider.Items.Add(new ProviderItem(LocalizationService.Get("provider_dwd"), "dwd"));
        SelectProviderItem(_settings.WeatherProvider);
        _cmbProvider.SelectedIndexChanged += async (s, e) =>
        {
            if (_cmbProvider.SelectedItem is ProviderItem pItem)
            {
                _settings.WeatherProvider = pItem.Code;
                ApplyLiveChanges();
                await _weatherService.GetWeatherAsync(_settings);
                UpdateLivePreview();
            }
        };
        cy += 48;

        var lblInterval = new Label { Text = LocalizationService.Get("gen_update_interval"), Location = new Point(16, cy), Size = new Size(contentW - 32, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ColorTextSecondary };
        cy += 24;
        _cmbInterval = new ComboBox { Location = new Point(16, cy), Size = new Size(contentW - 32, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ColorBgInput, ForeColor = ColorTextPrimary, FlatStyle = FlatStyle.Flat };
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("min_30"), 0));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_1"), 1));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_3"), 3));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_6"), 6));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_12"), 12));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_24"), 24));
        SelectIntervalItem(_settings.UpdateIntervalHours);
        _cmbInterval.SelectedIndexChanged += (s, e) => ApplyLiveChanges();
        cy += 48;

        // Yan Yana Sıcaklık Birimi ve Rüzgar Birimi (2 Sütun)
        int unitW = (contentW - 48) / 2;
        var lblTempUnit = new Label { Text = LocalizationService.Get("gen_temp_unit"), Location = new Point(16, cy), Size = new Size(unitW, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ColorTextSecondary };
        var lblWindUnit = new Label { Text = LocalizationService.Get("gen_wind_unit"), Location = new Point(24 + unitW, cy), Size = new Size(unitW, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ColorTextSecondary };
        cy += 24;

        _cmbTempUnit = new ComboBox { Location = new Point(16, cy), Size = new Size(unitW, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ColorBgInput, ForeColor = ColorTextPrimary, FlatStyle = FlatStyle.Flat };
        _cmbTempUnit.Items.Add("Celsius (°C)");
        _cmbTempUnit.Items.Add("Fahrenheit (°F)");
        _cmbTempUnit.SelectedIndex = _settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        _cmbTempUnit.SelectedIndexChanged += (s, e) => ApplyLiveChanges();

        _cmbWindUnit = new ComboBox { Location = new Point(24 + unitW, cy), Size = new Size(unitW, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ColorBgInput, ForeColor = ColorTextPrimary, FlatStyle = FlatStyle.Flat };
        _cmbWindUnit.Items.Add("Kilometre / Saat (km/h)");
        _cmbWindUnit.Items.Add("Mil / Saat (mph)");
        _cmbWindUnit.Items.Add("Metre / Saniye (m/s)");
        _cmbWindUnit.SelectedIndex = _settings.WindSpeedUnit.Equals("mph", StringComparison.OrdinalIgnoreCase) ? 1 : (_settings.WindSpeedUnit.Equals("ms", StringComparison.OrdinalIgnoreCase) ? 2 : 0);
        _cmbWindUnit.SelectedIndexChanged += (s, e) => ApplyLiveChanges();

        pnlCard.Controls.AddRange(new Control[] {
            lblProvider, _cmbProvider, lblInterval, _cmbInterval, lblTempUnit, _cmbTempUnit, lblWindUnit, _cmbWindUnit
        });

        _pnlGeneralTab.Controls.AddRange(new Control[] { lblHeader, pnlCard });
    }

    // =========================================================================
    // 4. SEKME: 🌐 DİL SEÇİMİ (CANLI ANINDA YENİDEN OLUŞTURMA)
    // =========================================================================
    private void BuildLanguageTab()
    {
        if (_pnlLanguageTab == null)
            _pnlLanguageTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 8, 8) };
        else
            _pnlLanguageTab.Controls.Clear();

        int y = 0;
        int contentW = 620;

        var lblHeader = new Label { Text = "🌐 Dil / Language Selection", Location = new Point(0, y), Size = new Size(contentW, 26), Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = ColorAccentCyan };
        y += 34;

        (string Code, string Name, string Flag)[] languages = {
            ("tr", "Türkçe", "🇹🇷"),
            ("en", "English", "🇬🇧"),
            ("de", "Deutsch", "🇩🇪"),
            ("es", "Español", "🇪🇸"),
            ("pt", "Português", "🇵🇹"),
            ("ar", "العربية", "🇸🇦"),
            ("ru", "Русский", "🇷🇺")
        };

        foreach (var (code, name, flag) in languages)
        {
            bool isSelected = _settings.Language.Equals(code, StringComparison.OrdinalIgnoreCase);
            var btnLang = new Button
            {
                Text = $"  {flag}   {name}  ({code.ToUpper()})" + (isSelected ? "   ✓ [Seçili / Active]" : ""),
                Location = new Point(0, y),
                Size = new Size(contentW, 46),
                BackColor = isSelected ? ColorAccent : ColorBgCard,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                Padding = new Padding(18, 0, 0, 0)
            };
            btnLang.FlatAppearance.BorderSize = 0;
            btnLang.FlatAppearance.MouseOverBackColor = isSelected ? Color.FromArgb(20, 160, 255) : ColorBgCardHover;
            btnLang.Click += (s, e) =>
            {
                _settings.Language = code;
                LocalizationService.SetLanguage(code);
                ConfigManager.SaveSettings(_settings);
                _onSettingsSaved(_settings);
                
                RebuildAllTabsAfterLanguageChange();
            };
            _pnlLanguageTab.Controls.Add(btnLang);
            y += 52;
        }

        _pnlLanguageTab.Controls.Add(lblHeader);
    }

    // =========================================================================
    // 5. SEKME: ℹ️ HAKKINDA (TIKLANABİLİR LİNKLER)
    // =========================================================================
    private void BuildAboutTab()
    {
        _pnlAboutTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 8, 8) };
        int y = 0;
        int contentW = 620;

        var lblHeader = new Label { Text = LocalizationService.Get("about_header"), Location = new Point(0, y), Size = new Size(contentW, 26), Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = ColorAccentCyan };
        y += 34;

        var pnlAboutCard = new Panel { Location = new Point(0, y), Size = new Size(contentW, 350), BackColor = ColorBgCard, Padding = new Padding(24) };
        pnlAboutCard.Paint += (s, e) => DrawSubtleBorder(e.Graphics, pnlAboutCard.ClientRectangle);

        var lblTitle = new Label { Text = "HaYTooL Weather", Location = new Point(20, 16), Size = new Size(420, 28), Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = ColorTextPrimary };
        var lblVer = new Label { Text = string.Format(LocalizationService.Get("about_ver"), AppVersion.GetCurrentVersion()), Location = new Point(20, 46), Size = new Size(500, 20), Font = new Font("Segoe UI", 9f), ForeColor = ColorTextSecondary };

        var lblDev = new Label { Text = LocalizationService.Get("about_dev"), Location = new Point(20, 80), Size = new Size(420, 22), Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = ColorAccentCyan };

        // 1. Tıklanabilir E-Posta Linki
        var lblEmailPrefix = new Label { Text = LocalizationService.Get("about_contact"), Location = new Point(20, 108), Size = new Size(80, 22), Font = new Font("Segoe UI", 9.5f), ForeColor = ColorTextSecondary };
        var lnkEmail = new LinkLabel
        {
            Text = "korazhayto@gmail.com",
            Location = new Point(104, 108),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            LinkColor = ColorAccentCyan,
            ActiveLinkColor = Color.FromArgb(100, 220, 255),
            VisitedLinkColor = ColorAccentCyan,
            Cursor = Cursors.Hand
        };
        lnkEmail.LinkClicked += (s, e) => OpenUrl("mailto:korazhayto@gmail.com");

        // 2. Tıklanabilir X (Twitter) Linki
        var lblXPrefix = new Label { Text = "X (Twitter):", Location = new Point(20, 134), Size = new Size(80, 22), Font = new Font("Segoe UI", 9.5f), ForeColor = ColorTextSecondary };
        var lnkX = new LinkLabel
        {
            Text = "https://x.com/HaYTo",
            Location = new Point(104, 134),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            LinkColor = ColorAccentCyan,
            ActiveLinkColor = Color.FromArgb(100, 220, 255),
            VisitedLinkColor = ColorAccentCyan,
            Cursor = Cursors.Hand
        };
        lnkX.LinkClicked += (s, e) => OpenUrl("https://x.com/HaYTo");

        // 3. Tıklanabilir GitHub Linki
        var lblGitPrefix = new Label { Text = "GitHub:", Location = new Point(20, 160), Size = new Size(80, 22), Font = new Font("Segoe UI", 9.5f), ForeColor = ColorTextSecondary };
        var lnkGit = new LinkLabel
        {
            Text = "https://github.com/HaYToKoRaZ/HaYTooL-Weather",
            Location = new Point(104, 160),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            LinkColor = ColorAccentCyan,
            ActiveLinkColor = Color.FromArgb(100, 220, 255),
            VisitedLinkColor = ColorAccentCyan,
            Cursor = Cursors.Hand
        };
        lnkGit.LinkClicked += (s, e) => OpenUrl("https://github.com/HaYToKoRaZ/HaYTooL-Weather");

        // 4. Güncelleme Kontrol Bölümü
        var btnCheckUpdate = new Button
        {
            Text = LocalizationService.Get("about_check_updates"),
            Location = new Point(20, 196),
            Size = new Size(190, 34),
            BackColor = Color.FromArgb(36, 44, 62),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnCheckUpdate.FlatAppearance.BorderSize = 0;
        btnCheckUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 58, 82);

        _chkAutoCheckUpdates = new CheckBox
        {
            Text = LocalizationService.Get("gen_auto_check_updates"),
            Location = new Point(220, 200),
            Size = new Size(380, 26),
            Checked = _settings.AutoCheckUpdates,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorSuccess
        };
        _chkAutoCheckUpdates.CheckedChanged += (s, e) => ApplyLiveChanges();

        var lblUpdateStatus = new Label
        {
            Text = "",
            Location = new Point(20, 238),
            Size = new Size(570, 22),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorSuccess
        };

        btnCheckUpdate.Click += async (s, e) =>
        {
            btnCheckUpdate.Enabled = false;
            lblUpdateStatus.ForeColor = ColorAccentCyan;
            lblUpdateStatus.Text = LocalizationService.Get("about_checking_updates");

            var result = await UpdateService.CheckForUpdatesAsync();
            btnCheckUpdate.Enabled = true;

            if (result.IsUpdateAvailable)
            {
                lblUpdateStatus.ForeColor = Color.FromArgb(255, 215, 0);
                lblUpdateStatus.Text = string.Format(LocalizationService.Get("about_update_available"), result.LatestVersion);

                var btnDownload = new Button
                {
                    Text = LocalizationService.Get("about_update_btn"),
                    Location = new Point(20, 266),
                    Size = new Size(200, 32),
                    BackColor = ColorAccent,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                btnDownload.FlatAppearance.BorderSize = 0;
                btnDownload.Click += (ds, de) => OpenUrl(result.ReleaseUrl);
                pnlAboutCard.Controls.Add(btnDownload);
            }
            else if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                lblUpdateStatus.ForeColor = ColorSuccess;
                lblUpdateStatus.Text = string.Format(LocalizationService.Get("about_latest_version"), result.CurrentVersion);
            }
            else
            {
                lblUpdateStatus.ForeColor = Color.FromArgb(255, 120, 120);
                lblUpdateStatus.Text = "Hata: " + result.ErrorMessage;
            }
        };

        var lblCopy = new Label { Text = LocalizationService.Get("about_copy"), Location = new Point(20, 310), Size = new Size(500, 20), Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = ColorTextMuted };

        pnlAboutCard.Controls.AddRange(new Control[] { lblTitle, lblVer, lblDev, lblEmailPrefix, lnkEmail, lblXPrefix, lnkX, lblGitPrefix, lnkGit, btnCheckUpdate, _chkAutoCheckUpdates, lblUpdateStatus, lblCopy });
        _pnlAboutTab.Controls.AddRange(new Control[] { lblHeader, pnlAboutCard });
    }

    private static void DrawSubtleBorder(Graphics g, Rectangle rect)
    {
        using var pen = new Pen(ColorBorder, 1);
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch { }
    }

    private void PickColor(Action<string> onColorPicked, string currentColor, Panel previewPanel)
    {
        using var cd = new ColorDialog
        {
            Color = IconGenerator.ParseHexColor(currentColor),
            FullOpen = true
        };
        if (cd.ShowDialog() == DialogResult.OK)
        {
            var hex = IconGenerator.ColorToHex(cd.Color);
            onColorPicked(hex);
            previewPanel.BackColor = cd.Color;
            ApplyLiveChanges();
        }
    }

    private void ApplyLiveChanges()
    {
        if (_isInitializing) return;

        if (_cmbTrayMode != null)
        {
            _settings.TrayDisplayMode = _cmbTrayMode.SelectedIndex switch
            {
                1 => "temp_only",
                2 => "weather_only",
                3 => "single_compact",
                _ => "dual"
            };
        }

        if (_tbWeatherScale != null) _settings.WeatherIconScale = _tbWeatherScale.Value;
        if (_tbWeatherOpacity != null) _settings.WeatherBgOpacity = _tbWeatherOpacity.Value;

        if (_tbTempScale != null) _settings.TempTextScale = _tbTempScale.Value;
        if (_tbTempOpacity != null) _settings.TempBgOpacity = _tbTempOpacity.Value;

        if (_cmbProvider?.SelectedItem is ProviderItem pItem) _settings.WeatherProvider = pItem.Code;
        if (_cmbInterval?.SelectedItem is IntervalItem intItem) _settings.UpdateIntervalHours = intItem.Hours;
        if (_cmbTempUnit != null) _settings.TemperatureUnit = _cmbTempUnit.SelectedIndex == 1 ? "fahrenheit" : "celsius";
        if (_cmbWindUnit != null) _settings.WindSpeedUnit = _cmbWindUnit.SelectedIndex switch { 1 => "mph", 2 => "ms", _ => "kmh" };
        if (_chkAutoCheckUpdates != null) _settings.AutoCheckUpdates = _chkAutoCheckUpdates.Checked;
        if (_chkHighContrast != null) _settings.HighContrastTrayIcon = _chkHighContrast.Checked;

        ConfigManager.SaveSettings(_settings);
        _onSettingsSaved(_settings);

        UpdateLivePreview();
    }

    private void SelectProviderItem(string code)
    {
        if (_cmbProvider == null) return;
        for (int i = 0; i < _cmbProvider.Items.Count; i++)
        {
            if (_cmbProvider.Items[i] is ProviderItem item && item.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                _cmbProvider.SelectedIndex = i;
                return;
            }
        }
        _cmbProvider.SelectedIndex = 0;
    }

    private void SelectIntervalItem(int hours)
    {
        if (_cmbInterval == null) return;
        for (int i = 0; i < _cmbInterval.Items.Count; i++)
        {
            if (_cmbInterval.Items[i] is IntervalItem item && item.Hours == hours)
            {
                _cmbInterval.SelectedIndex = i;
                return;
            }
        }
        _cmbInterval.SelectedIndex = 3;
    }

    private class ProviderItem
    {
        public string Label { get; }
        public string Code { get; }
        public ProviderItem(string label, string code) { Label = label; Code = code; }
        public override string ToString() => Label;
    }

    private class IntervalItem
    {
        public string Label { get; }
        public int Hours { get; }
        public IntervalItem(string label, int hours) { Label = label; Hours = hours; }
        public override string ToString() => Label;
    }
}
