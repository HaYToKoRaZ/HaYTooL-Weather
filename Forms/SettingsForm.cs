using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32;
using HaYTooLWeather.Models;
using HaYTooLWeather.Services;

namespace HaYTooLWeather.Forms;

/// <summary>
/// HaYTooL Weather Kontrol Merkezi - 6 Popüler Şehir (Kocaeli dahil), Anında Tam Dil Değişimi, Tıklanabilir Linkler ve Kaydırmasız Görünüm Düzeni.
/// </summary>
public class SettingsForm : Form
{
    private readonly WeatherService _weatherService;
    private readonly AppSettings _settings;
    private readonly Action<AppSettings> _onSettingsSaved;
    private bool _isInitializing = true;

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

        Text = "HaYTooL Weather - " + LocalizationService.Get("settings_title");
        Size = new Size(890, 690);
        MinimumSize = new Size(890, 690);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(16, 18, 24);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        // 1. SOL KENAR ÇUBUĞU (Sidebar)
        var pnlSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 230,
            BackColor = Color.FromArgb(22, 25, 34),
            Padding = new Padding(12, 16, 12, 16)
        };

        var lblLogo = new Label
        {
            Text = "🌤️ HaYTooL Weather",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 190, 255)
        };
        var lblSubtitle = new Label
        {
            Text = "Control Center & Settings",
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(120, 135, 160)
        };

        var pnlNavButtons = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 24, 0, 0)
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

        pnlNavButtons.Controls.AddRange(new Control[] {
            _btnTabAbout, _btnTabLanguage, _btnTabGeneral, _btnTabAppearance, _btnTabLocation
        });

        pnlSidebar.Controls.Add(pnlNavButtons);
        pnlSidebar.Controls.Add(lblSubtitle);
        pnlSidebar.Controls.Add(lblLogo);

        // 2. ALT BAR (Kapat Butonu)
        var pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            BackColor = Color.FromArgb(20, 23, 30),
            Padding = new Padding(24, 10, 24, 10)
        };
        _btnClose = new Button
        {
            Text = LocalizationService.Get("btn_close"),
            Dock = DockStyle.Right,
            Width = 160,
            BackColor = Color.FromArgb(0, 122, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        _btnClose.FlatAppearance.BorderSize = 0;
        _btnClose.Click += (s, e) => Close();
        pnlBottom.Controls.Add(_btnClose);

        // 3. SAĞ İÇERİK ALANI (Content Area)
        _pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(16, 18, 24),
            Padding = new Padding(20, 16, 20, 16)
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
            ForeColor = Color.FromArgb(200, 210, 230),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 8)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 40, 54);
        return btn;
    }

    private void SwitchTab(Button activeBtn, Panel targetPanel)
    {
        if (_currentActiveTabBtn != null)
        {
            _currentActiveTabBtn.BackColor = Color.Transparent;
            _currentActiveTabBtn.ForeColor = Color.FromArgb(200, 210, 230);
        }

        _currentActiveTabBtn = activeBtn;
        _currentActiveTabBtn.BackColor = Color.FromArgb(0, 122, 255);
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
        // 1. Pencere Başlığı ve Alt Bar
        Text = "HaYTooL Weather - " + LocalizationService.Get("settings_title");
        _btnClose.Text = LocalizationService.Get("btn_close");

        // 2. Kenar Çubuğu Buton Metinleri
        _btnTabLocation.Text = LocalizationService.Get("tab_location");
        _btnTabAppearance.Text = LocalizationService.Get("tab_appearance");
        _btnTabGeneral.Text = LocalizationService.Get("tab_general");
        _btnTabLanguage.Text = LocalizationService.Get("tab_language");
        _btnTabAbout.Text = LocalizationService.Get("tab_about");

        // 3. Sekme İçeriklerini Baştan Oluştur
        BuildLocationTab();
        BuildAppearanceTab();
        BuildGeneralTab();
        BuildLanguageTab();
        BuildAboutTab();

        // 4. Aktif Paneli Yenile
        SwitchTab(_btnTabLanguage, _pnlLanguageTab!);
    }

    // =========================================================================
    // 1. SEKME: 📍 KONUM & ŞEHİR SEÇİMİ (6 POPÜLER ŞEHİR: İSTANBUL, ANKARA, İZMİR, BURSA, ANTALYA, KOCAELİ)
    // =========================================================================
    private void BuildLocationTab()
    {
        _pnlLocationTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 10, 10) };
        int y = 0;

        var lblHeader = new Label { Text = LocalizationService.Get("loc_title"), Location = new Point(0, y), Size = new Size(580, 26), Font = new Font("Segoe UI", 12.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 190, 255) };
        y += 32;

        // Aktif Konum Kartı
        var pnlCurrentLoc = new Panel { Location = new Point(0, y), Size = new Size(590, 64), BackColor = Color.FromArgb(26, 30, 42), Padding = new Padding(14, 8, 14, 8) };
        var lblCurTitle = new Label { Text = LocalizationService.Get("loc_active_badge"), Location = new Point(14, 6), Size = new Size(200, 16), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 220, 160) };
        _lblCurrentLocationName = new Label { Text = $"{WeatherService.FormatLocationTitle(_settings.City, _settings.District)}, {_settings.Country}", Location = new Point(14, 24), Size = new Size(330, 26), Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.White };
        _lblCurrentLocationCoords = new Label { Text = $"Koordinat: {_settings.Latitude:F4}°N, {_settings.Longitude:F4}°E", Location = new Point(350, 26), Size = new Size(225, 22), TextAlign = ContentAlignment.TopRight, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(140, 155, 180) };
        pnlCurrentLoc.Controls.AddRange(new Control[] { lblCurTitle, _lblCurrentLocationName, _lblCurrentLocationCoords });
        y += 72;

        // Popüler Şehirler (Desteklenen Dillerin Başkentleri + Türkiye'den İstanbul)
        var lblQuick = new Label { Text = LocalizationService.Get("loc_popular"), Location = new Point(0, y), Size = new Size(590, 20), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 215, 240) };
        y += 22;

        var flpChips = new FlowLayoutPanel { Location = new Point(0, y), Size = new Size(590, 38), BackColor = Color.Transparent, AutoScroll = false };
        
        // Desteklenen 7 dilin temsilcileri (TR: İstanbul, EN: London, DE: Berlin, ES: Madrid, PT: Lisbon, RU: Moscow, AR: Riyadh)
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
                Height = 30,
                AutoSize = true,
                BackColor = query == "Istanbul" ? Color.FromArgb(0, 100, 210) : Color.FromArgb(32, 38, 52),
                ForeColor = Color.FromArgb(220, 235, 255),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 0),
                Padding = new Padding(8, 0, 8, 0)
            };
            chip.FlatAppearance.BorderSize = 0;
            chip.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 122, 255);
            chip.Click += async (s, e) => await QuickSelectCity(query);
            flpChips.Controls.Add(chip);
        }
        y += 42;

        // Arama Alanı
        var lblSearchTitle = new Label { Text = LocalizationService.Get("loc_search_title"), Location = new Point(0, y), Size = new Size(590, 20), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 215, 240) };
        y += 22;

        var pnlSearchBox = new Panel { Location = new Point(0, y), Size = new Size(590, 36), BackColor = Color.FromArgb(28, 33, 46) };
        _txtSearch = new TextBox
        {
            Location = new Point(12, 7),
            Size = new Size(470, 24),
            BackColor = Color.FromArgb(28, 33, 46),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10f),
            PlaceholderText = LocalizationService.Get("loc_search_placeholder")
        };
        _txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await PerformSearchAsync(); } };

        var btnSearch = new Button
        {
            Text = LocalizationService.Get("loc_search_btn"),
            Location = new Point(490, 3),
            Size = new Size(95, 30),
            BackColor = Color.FromArgb(0, 122, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += async (s, e) => await PerformSearchAsync();

        pnlSearchBox.Controls.Add(_txtSearch);
        pnlSearchBox.Controls.Add(btnSearch);
        y += 42;

        // Sonuç Listesi
        _lstSearchResults = new ListBox
        {
            Location = new Point(0, y),
            Size = new Size(590, 160),
            BackColor = Color.FromArgb(24, 28, 38),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9.5f),
            ItemHeight = 26
        };
        y += 166;

        var lblLocationStatus = new Label
        {
            Text = "",
            Location = new Point(0, y + 6),
            Size = new Size(370, 24),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 220, 160)
        };
        _lstSearchResults.DoubleClick += async (s, e) => await ApplySelectedLocationAsync(lblLocationStatus);

        var btnApplyLocation = new Button
        {
            Text = LocalizationService.Get("loc_apply_btn"),
            Location = new Point(370, y),
            Size = new Size(220, 34),
            BackColor = Color.FromArgb(0, 180, 120),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        btnApplyLocation.FlatAppearance.BorderSize = 0;
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

        // İlçe ve İl ayrımı
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
            statusLabel.ForeColor = Color.FromArgb(0, 200, 255);
        }

        ConfigManager.SaveSettings(_settings);
        _onSettingsSaved(_settings);

        try
        {
            var weather = await _weatherService.GetWeatherAsync(_settings);
            if (weather != null && statusLabel != null)
            {
                statusLabel.Text = $"✅ {weather.LocationName}: {weather.Temperature:0}°C - OK!";
                statusLabel.ForeColor = Color.FromArgb(0, 230, 160);
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

        var lblHeader = new Label { Text = LocalizationService.Get("app_title"), Location = new Point(0, y), Size = new Size(580, 26), Font = new Font("Segoe UI", 12.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 190, 255) };
        y += 30;

        // 1. Canlı Görev Çubuğu Önizleme Kutusu
        var pnlPreviewBox = new Panel { Location = new Point(0, y), Size = new Size(590, 62), BackColor = Color.FromArgb(24, 28, 38), Padding = new Padding(10) };
        var lblPreviewTitle = new Label { Text = LocalizationService.Get("app_live_preview"), Location = new Point(10, 6), Size = new Size(250, 14), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 220, 160) };
        _pbLivePreview = new PictureBox { Location = new Point(10, 22), Size = new Size(570, 34), BackColor = Color.FromArgb(14, 16, 22), BorderStyle = BorderStyle.None };
        _pbLivePreview.Paint += (s, e) => PaintLivePreview(e.Graphics);
        pnlPreviewBox.Controls.Add(lblPreviewTitle);
        pnlPreviewBox.Controls.Add(_pbLivePreview);
        y += 70;

        // 2. Gösterim Modu Satırı
        var lblMode = new Label { Text = LocalizationService.Get("app_mode"), Location = new Point(0, y + 4), Size = new Size(110, 24), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 215, 240) };
        _cmbTrayMode = new ComboBox { Location = new Point(115, y), Size = new Size(475, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(32, 38, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        _cmbTrayMode.Items.Add("⭐ Çift Alan (Solda Tam Boy İkon + Sağda Dev Sıcaklık)");
        _cmbTrayMode.Items.Add("🌡️ Sadece Dev Sıcaklık Derecesi (Tek Alan)");
        _cmbTrayMode.Items.Add("🌤️ Sadece Hava Durumu İkonu (Tek Alan)");
        _cmbTrayMode.Items.Add("🔹 Tek Alan Kompakt (Küçük İkon + Derece)");
        _cmbTrayMode.SelectedIndex = _settings.TrayDisplayMode.ToLowerInvariant() switch { "temp_only" => 1, "weather_only" => 2, "single_compact" => 3, _ => 0 };
        _cmbTrayMode.SelectedIndexChanged += (s, e) => ApplyLiveChanges();
        y += 38;

        // 3. YAN YANA 2 SÜTUN KARTLARI
        int colWidth = 288;
        int colGap = 14;
        int cardHeight = 310;

        // SOL SÜTUN: HAVA DURUMU SİMGESİ KARTI
        var grpWeather = new GroupBox
        {
            Text = LocalizationService.Get("app_weather_icon"),
            Location = new Point(0, y),
            Size = new Size(colWidth, cardHeight),
            ForeColor = Color.FromArgb(255, 215, 0),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            BackColor = Color.FromArgb(22, 26, 36)
        };
        int gy1 = 26;

        var lblWScale = new Label { Text = LocalizationService.Get("app_icon_size"), Location = new Point(12, gy1), Size = new Size(160, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
        _lblWeatherScaleVal = new Label { Text = $"%{_settings.WeatherIconScale}", Location = new Point(205, gy1), Size = new Size(65, 20), TextAlign = ContentAlignment.TopRight, ForeColor = Color.FromArgb(255, 220, 100), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy1 += 22;
        _tbWeatherScale = new TrackBar { Location = new Point(8, gy1), Size = new Size(270, 30), Minimum = 40, Maximum = 200, Value = Math.Clamp(_settings.WeatherIconScale, 40, 200), TickStyle = TickStyle.None };
        _tbWeatherScale.Scroll += (s, e) => { _lblWeatherScaleVal.Text = $"%{_tbWeatherScale.Value}"; ApplyLiveChanges(); };
        gy1 += 44;

        var lblWBg = new Label { Text = LocalizationService.Get("app_bg_color"), Location = new Point(12, gy1 + 3), Size = new Size(110, 22), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
        _pnlWeatherColorPreview = new Panel { Location = new Point(125, gy1), Size = new Size(32, 26), BackColor = IconGenerator.ParseHexColor(_settings.WeatherBgColor), BorderStyle = BorderStyle.FixedSingle };
        var btnWBgColor = new Button { Text = LocalizationService.Get("app_pick_color"), Location = new Point(165, gy1), Size = new Size(105, 26), BackColor = Color.FromArgb(42, 50, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5f) };
        btnWBgColor.FlatAppearance.BorderSize = 0;
        btnWBgColor.Click += (s, e) => PickColor(c => _settings.WeatherBgColor = c, _settings.WeatherBgColor, _pnlWeatherColorPreview);
        gy1 += 48;

        var lblWOp = new Label { Text = LocalizationService.Get("app_bg_opacity"), Location = new Point(12, gy1), Size = new Size(160, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
        _lblWeatherOpacityVal = new Label { Text = $"%{_settings.WeatherBgOpacity}", Location = new Point(205, gy1), Size = new Size(65, 20), TextAlign = ContentAlignment.TopRight, ForeColor = Color.FromArgb(170, 230, 255), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy1 += 22;
        _tbWeatherOpacity = new TrackBar { Location = new Point(8, gy1), Size = new Size(270, 30), Minimum = 0, Maximum = 100, Value = Math.Clamp(_settings.WeatherBgOpacity, 0, 100), TickStyle = TickStyle.None };
        _tbWeatherOpacity.Scroll += (s, e) => { _lblWeatherOpacityVal.Text = $"%{_tbWeatherOpacity.Value}"; ApplyLiveChanges(); };

        grpWeather.Controls.AddRange(new Control[] { lblWScale, _lblWeatherScaleVal, _tbWeatherScale, lblWBg, _pnlWeatherColorPreview, btnWBgColor, lblWOp, _lblWeatherOpacityVal, _tbWeatherOpacity });

        // SAĞ SÜTUN: SICAKLIK DERECESİ KARTI
        var grpTemp = new GroupBox
        {
            Text = LocalizationService.Get("app_temp_text"),
            Location = new Point(colWidth + colGap, y),
            Size = new Size(colWidth, cardHeight),
            ForeColor = Color.FromArgb(0, 200, 255),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            BackColor = Color.FromArgb(22, 26, 36)
        };
        int gy2 = 26;

        var lblTScale = new Label { Text = LocalizationService.Get("app_text_size"), Location = new Point(12, gy2), Size = new Size(160, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
        _lblTempScaleVal = new Label { Text = $"%{_settings.TempTextScale}", Location = new Point(205, gy2), Size = new Size(65, 20), TextAlign = ContentAlignment.TopRight, ForeColor = Color.FromArgb(100, 220, 255), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy2 += 22;
        _tbTempScale = new TrackBar { Location = new Point(8, gy2), Size = new Size(270, 30), Minimum = 40, Maximum = 200, Value = Math.Clamp(_settings.TempTextScale, 40, 200), TickStyle = TickStyle.None };
        _tbTempScale.Scroll += (s, e) => { _lblTempScaleVal.Text = $"%{_tbTempScale.Value}"; ApplyLiveChanges(); };
        gy2 += 44;

        var lblTBg = new Label { Text = LocalizationService.Get("app_bg_color"), Location = new Point(12, gy2 + 3), Size = new Size(110, 22), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
        _pnlTempColorPreview = new Panel { Location = new Point(125, gy2), Size = new Size(32, 26), BackColor = IconGenerator.ParseHexColor(_settings.TempBgColor), BorderStyle = BorderStyle.FixedSingle };
        var btnTBgColor = new Button { Text = LocalizationService.Get("app_pick_color"), Location = new Point(165, gy2), Size = new Size(105, 26), BackColor = Color.FromArgb(42, 50, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5f) };
        btnTBgColor.FlatAppearance.BorderSize = 0;
        btnTBgColor.Click += (s, e) => PickColor(c => _settings.TempBgColor = c, _settings.TempBgColor, _pnlTempColorPreview);
        gy2 += 48;

        var lblTOp = new Label { Text = LocalizationService.Get("app_bg_opacity"), Location = new Point(12, gy2), Size = new Size(160, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
        _lblTempOpacityVal = new Label { Text = $"%{_settings.TempBgOpacity}", Location = new Point(205, gy2), Size = new Size(65, 20), TextAlign = ContentAlignment.TopRight, ForeColor = Color.FromArgb(170, 230, 255), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy2 += 22;
        _tbTempOpacity = new TrackBar { Location = new Point(8, gy2), Size = new Size(270, 30), Minimum = 0, Maximum = 100, Value = Math.Clamp(_settings.TempBgOpacity, 0, 100), TickStyle = TickStyle.None };
        _tbTempOpacity.Scroll += (s, e) => { _lblTempOpacityVal.Text = $"%{_tbTempOpacity.Value}"; ApplyLiveChanges(); };
        gy2 += 44;

        _chkHighContrast = new CheckBox
        {
            Text = LocalizationService.Get("app_high_contrast"),
            Location = new Point(12, gy2),
            Size = new Size(260, 24),
            Checked = _settings.HighContrastTrayIcon,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(255, 240, 150)
        };
        _chkHighContrast.CheckedChanged += (s, e) => ApplyLiveChanges();

        grpTemp.Controls.AddRange(new Control[] { lblTScale, _lblTempScaleVal, _tbTempScale, lblTBg, _pnlTempColorPreview, btnTBgColor, lblTOp, _lblTempOpacityVal, _tbTempOpacity, _chkHighContrast });

        _pnlAppearanceTab.Controls.AddRange(new Control[] {
            lblHeader, pnlPreviewBox, lblMode, _cmbTrayMode, grpWeather, grpTemp
        });
    }

    private void PaintLivePreview(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(14, 16, 22));

        // Görev Çubuğu Saati Çizimi
        using var fontClock = new Font("Segoe UI", 9f, FontStyle.Regular);
        using var brushClock = new SolidBrush(Color.FromArgb(180, 195, 215));
        string timeStr = DateTime.Now.ToString("HH:mm");
        g.DrawString(timeStr, fontClock, brushClock, 515, 8);

        // Canlı Simge Önizlemesi
        int temp = _weatherService.LastWeatherData != null ? (int)Math.Round(_weatherService.LastWeatherData.Temperature) : 24;
        int weatherCode = _weatherService.LastWeatherData?.WeatherCode ?? 0;
        bool isDay = _weatherService.LastWeatherData?.IsDay ?? true;

        var (wIcon, wHicon) = IconGenerator.GenerateWeatherOnlyIcon(weatherCode, isDay, _settings.WeatherIconScale, _settings.WeatherBgColor, _settings.WeatherBgOpacity);
        var (tIcon, tHicon) = IconGenerator.GenerateTemperatureOnlyIcon(temp, _settings.HighContrastTrayIcon, _settings.TempTextScale, _settings.TempBgColor, _settings.TempBgOpacity);

        g.DrawIcon(wIcon, new Rectangle(435, 2, 30, 30));
        g.DrawIcon(tIcon, new Rectangle(470, 2, 30, 30));

        using var fontLabel = new Font("Segoe UI", 8.5f, FontStyle.Italic);
        using var brushLabel = new SolidBrush(Color.FromArgb(120, 140, 165));
        g.DrawString("Görev Çubuğu Görünümü ➔", fontLabel, brushLabel, 255, 9);

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
        _pnlGeneralTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 10, 10) };
        int y = 0;

        var lblHeader = new Label { Text = LocalizationService.Get("gen_title"), Location = new Point(0, y), Size = new Size(580, 26), Font = new Font("Segoe UI", 12.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 190, 255) };
        y += 34;

        // Hava Durumu Veri Kaynağı / Model Seçimi
        var lblProvider = new Label { Text = LocalizationService.Get("gen_weather_provider"), Location = new Point(0, y), Size = new Size(580, 20), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 215, 240) };
        y += 22;
        _cmbProvider = new ComboBox { Location = new Point(0, y), Size = new Size(580, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(32, 38, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
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
        y += 46;

        var lblInterval = new Label { Text = LocalizationService.Get("gen_update_interval"), Location = new Point(0, y), Size = new Size(580, 20), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 215, 240) };
        y += 22;
        _cmbInterval = new ComboBox { Location = new Point(0, y), Size = new Size(580, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(32, 38, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("min_30"), 0));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_1"), 1));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_3"), 3));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_6"), 6));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_12"), 12));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_24"), 24));
        SelectIntervalItem(_settings.UpdateIntervalHours);
        _cmbInterval.SelectedIndexChanged += (s, e) => ApplyLiveChanges();
        y += 46;

        var lblTempUnit = new Label { Text = LocalizationService.Get("gen_temp_unit"), Location = new Point(0, y), Size = new Size(580, 20), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 215, 240) };
        y += 22;
        _cmbTempUnit = new ComboBox { Location = new Point(0, y), Size = new Size(580, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(32, 38, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        _cmbTempUnit.Items.Add("Celsius (°C)");
        _cmbTempUnit.Items.Add("Fahrenheit (°F)");
        _cmbTempUnit.SelectedIndex = _settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        _cmbTempUnit.SelectedIndexChanged += (s, e) => ApplyLiveChanges();
        y += 46;

        var lblWindUnit = new Label { Text = LocalizationService.Get("gen_wind_unit"), Location = new Point(0, y), Size = new Size(580, 20), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 215, 240) };
        y += 22;
        _cmbWindUnit = new ComboBox { Location = new Point(0, y), Size = new Size(580, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(32, 38, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        _cmbWindUnit.Items.Add("Kilometre / Saat (km/h)");
        _cmbWindUnit.Items.Add("Mil / Saat (mph)");
        _cmbWindUnit.Items.Add("Metre / Saniye (m/s)");
        _cmbWindUnit.SelectedIndex = _settings.WindSpeedUnit.Equals("mph", StringComparison.OrdinalIgnoreCase) ? 1 : (_settings.WindSpeedUnit.Equals("ms", StringComparison.OrdinalIgnoreCase) ? 2 : 0);
        _cmbWindUnit.SelectedIndexChanged += (s, e) => ApplyLiveChanges();

        _pnlGeneralTab.Controls.AddRange(new Control[] {
            lblHeader, lblProvider, _cmbProvider, lblInterval, _cmbInterval, lblTempUnit, _cmbTempUnit, lblWindUnit, _cmbWindUnit
        });
    }

    // =========================================================================
    // 4. SEKME: 🌐 DİL SEÇİMİ (CANLI ANINDA YENİDEN OLUŞTURMA)
    // =========================================================================
    private void BuildLanguageTab()
    {
        if (_pnlLanguageTab == null)
            _pnlLanguageTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 10, 10) };
        else
            _pnlLanguageTab.Controls.Clear();

        int y = 0;

        var lblHeader = new Label { Text = "🌐 Dil / Language Selection", Location = new Point(0, y), Size = new Size(580, 26), Font = new Font("Segoe UI", 12.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 190, 255) };
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
                Size = new Size(580, 44),
                BackColor = isSelected ? Color.FromArgb(0, 122, 255) : Color.FromArgb(26, 30, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                Padding = new Padding(16, 0, 0, 0)
            };
            btnLang.FlatAppearance.BorderSize = 0;
            btnLang.Click += (s, e) =>
            {
                _settings.Language = code;
                LocalizationService.SetLanguage(code);
                ConfigManager.SaveSettings(_settings);
                _onSettingsSaved(_settings);
                
                // Formdaki tüm metinleri ve sekmeleri anında yeni dille baştan oluştur!
                RebuildAllTabsAfterLanguageChange();
            };
            _pnlLanguageTab.Controls.Add(btnLang);
            y += 50;
        }

        _pnlLanguageTab.Controls.Add(lblHeader);
    }

    // =========================================================================
    // 5. SEKME: ℹ️ HAKKINDA (TIKLANABİLİR LİNKLER)
    // =========================================================================
    private void BuildAboutTab()
    {
        _pnlAboutTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 10, 10) };
        int y = 0;

        var lblHeader = new Label { Text = LocalizationService.Get("about_header"), Location = new Point(0, y), Size = new Size(580, 26), Font = new Font("Segoe UI", 12.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 190, 255) };
        y += 34;

        var pnlAboutCard = new Panel { Location = new Point(0, y), Size = new Size(590, 265), BackColor = Color.FromArgb(24, 28, 38), Padding = new Padding(22) };
        var lblTitle = new Label { Text = "HaYTooL Weather", Location = new Point(22, 18), Size = new Size(400, 26), Font = new Font("Segoe UI", 13.5f, FontStyle.Bold), ForeColor = Color.White };
        var lblVer = new Label { Text = LocalizationService.Get("about_ver"), Location = new Point(22, 46), Size = new Size(500, 20), Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 145, 170) };

        var lblDev = new Label { Text = LocalizationService.Get("about_dev"), Location = new Point(22, 82), Size = new Size(400, 22), Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 210, 255) };

        // 1. Tıklanabilir E-Posta Linki
        var lblEmailPrefix = new Label { Text = LocalizationService.Get("about_contact"), Location = new Point(22, 110), Size = new Size(80, 22), Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(200, 215, 235) };
        var lnkEmail = new LinkLabel
        {
            Text = "korazhayto@gmail.com",
            Location = new Point(104, 110),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            LinkColor = Color.FromArgb(0, 190, 255),
            ActiveLinkColor = Color.FromArgb(100, 220, 255),
            VisitedLinkColor = Color.FromArgb(0, 190, 255),
            Cursor = Cursors.Hand
        };
        lnkEmail.LinkClicked += (s, e) => OpenUrl("mailto:korazhayto@gmail.com");

        // 2. Tıklanabilir X (Twitter) Linki
        var lblXPrefix = new Label { Text = "X (Twitter):", Location = new Point(22, 136), Size = new Size(80, 22), Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(200, 215, 235) };
        var lnkX = new LinkLabel
        {
            Text = "https://x.com/HaYTo",
            Location = new Point(104, 136),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            LinkColor = Color.FromArgb(0, 190, 255),
            ActiveLinkColor = Color.FromArgb(100, 220, 255),
            VisitedLinkColor = Color.FromArgb(0, 190, 255),
            Cursor = Cursors.Hand
        };
        lnkX.LinkClicked += (s, e) => OpenUrl("https://x.com/HaYTo");

        // 3. Tıklanabilir GitHub Linki
        var lblGitPrefix = new Label { Text = "GitHub:", Location = new Point(22, 162), Size = new Size(80, 22), Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(200, 215, 235) };
        var lnkGit = new LinkLabel
        {
            Text = "https://github.com/HaYToKoRaZ/HaYTooL-Weather",
            Location = new Point(104, 162),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            LinkColor = Color.FromArgb(0, 190, 255),
            ActiveLinkColor = Color.FromArgb(100, 220, 255),
            VisitedLinkColor = Color.FromArgb(0, 190, 255),
            Cursor = Cursors.Hand
        };
        lnkGit.LinkClicked += (s, e) => OpenUrl("https://github.com/HaYToKoRaZ/HaYTooL-Weather");

        // 4. Güncelleme Kontrol Bölümü (Yan Yana: Buton + Başlangıçta Denetle Onay Kutusu)
        var btnCheckUpdate = new Button
        {
            Text = LocalizationService.Get("about_check_updates"),
            Location = new Point(22, 198),
            Size = new Size(185, 32),
            BackColor = Color.FromArgb(38, 45, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnCheckUpdate.FlatAppearance.BorderSize = 0;

        _chkAutoCheckUpdates = new CheckBox
        {
            Text = LocalizationService.Get("gen_auto_check_updates"),
            Location = new Point(218, 201),
            Size = new Size(355, 26),
            Checked = _settings.AutoCheckUpdates,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 220, 160)
        };
        _chkAutoCheckUpdates.CheckedChanged += (s, e) => ApplyLiveChanges();

        var lblUpdateStatus = new Label
        {
            Text = "",
            Location = new Point(22, 238),
            Size = new Size(550, 22),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 220, 140)
        };

        btnCheckUpdate.Click += async (s, e) =>
        {
            btnCheckUpdate.Enabled = false;
            lblUpdateStatus.ForeColor = Color.FromArgb(0, 190, 255);
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
                    Location = new Point(22, 266),
                    Size = new Size(200, 32),
                    BackColor = Color.FromArgb(0, 122, 255),
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
                lblUpdateStatus.ForeColor = Color.FromArgb(100, 220, 140);
                lblUpdateStatus.Text = string.Format(LocalizationService.Get("about_latest_version"), result.CurrentVersion);
            }
            else
            {
                lblUpdateStatus.ForeColor = Color.FromArgb(255, 120, 120);
                lblUpdateStatus.Text = "Hata: " + result.ErrorMessage;
            }
        };

        var lblCopy = new Label { Text = LocalizationService.Get("about_copy"), Location = new Point(22, 305), Size = new Size(500, 20), Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = Color.FromArgb(110, 125, 145) };

        pnlAboutCard.Size = new Size(590, 335);
        pnlAboutCard.Controls.AddRange(new Control[] { lblTitle, lblVer, lblDev, lblEmailPrefix, lnkEmail, lblXPrefix, lnkX, lblGitPrefix, lnkGit, btnCheckUpdate, _chkAutoCheckUpdates, lblUpdateStatus, lblCopy });
        _pnlAboutTab.Controls.AddRange(new Control[] { lblHeader, pnlAboutCard });
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
        if (_tbTempScale != null) _settings.TempTextScale = _tbTempScale.Value;

        if (_tbWeatherOpacity != null) _settings.WeatherBgOpacity = _tbWeatherOpacity.Value;
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

    private static void SetAutoStartWithWindows(bool enable)
    {
        try
        {
            const string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            using var key = Registry.CurrentUser.OpenSubKey(runKey, true);
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
