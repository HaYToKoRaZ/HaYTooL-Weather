using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32;
using HaYTooLWeather.Models;
using HaYTooLWeather.Services;

namespace HaYTooLWeather.Forms;

/// <summary>
/// HaYTooL Weather Kontrol Merkezi - Windows 11 Fluent 2 & Acrylic Dark Mimarisi.
/// Tamamen sıfırdan tasarlanan modern kart tabanlı düzen, akrilik derinlik, 240px navigasyon ve kaydırmasız simetrik arayüz.
/// </summary>
public class SettingsForm : Form
{
    private readonly WeatherService _weatherService;
    private readonly AppSettings _settings;
    private readonly Action<AppSettings> _onSettingsSaved;
    private bool _isInitializing = true;

    // Fluent 2 Design Tokens (Windows 11 Mica & Acrylic System)
    private static readonly Color ColorBgMica = Color.FromArgb(13, 17, 24);
    private static readonly Color ColorBgSidebar = Color.FromArgb(18, 24, 34);
    private static readonly Color ColorBgCard = Color.FromArgb(24, 32, 46);
    private static readonly Color ColorBgCardHover = Color.FromArgb(32, 42, 60);
    private static readonly Color ColorBgInput = Color.FromArgb(15, 20, 29);
    private static readonly Color ColorBorder = Color.FromArgb(38, 50, 72);
    private static readonly Color ColorBorderCard = Color.FromArgb(44, 58, 84);
    private static readonly Color ColorAccentBlue = Color.FromArgb(0, 120, 242);
    private static readonly Color ColorAccentCyan = Color.FromArgb(0, 210, 255);
    private static readonly Color ColorEmerald = Color.FromArgb(16, 185, 129);
    private static readonly Color ColorGold = Color.FromArgb(245, 158, 11);
    private static readonly Color ColorTextPrimary = Color.FromArgb(248, 250, 255);
    private static readonly Color ColorTextSecondary = Color.FromArgb(156, 172, 198);
    private static readonly Color ColorTextMuted = Color.FromArgb(102, 118, 144);

    // Navigasyon Butonları
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

    // 1. Konum Sekmesi
    private TextBox? _txtSearch;
    private ListBox? _lstSearchResults;
    private Label? _lblCurrentLocationName;
    private Label? _lblCurrentLocationCoords;
    private readonly List<GeoLocation> _searchResults = new();

    // 2. Görünüm Sekmesi
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

    // 3. Genel Sekmesi
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
        Size = new Size(940, 690);
        MinimumSize = new Size(940, 690);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = ColorBgMica;
        ForeColor = ColorTextPrimary;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        // =========================================================================
        // 1. SOL KENAR ÇUBUĞU (Sidebar: Windows 11 Fluent 2 Tarzı 240px)
        // =========================================================================
        var pnlSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 240,
            BackColor = ColorBgSidebar,
            Padding = new Padding(14, 20, 14, 16)
        };
        pnlSidebar.Paint += (s, e) =>
        {
            using var pen = new Pen(ColorBorder, 1);
            e.Graphics.DrawLine(pen, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
        };

        // Logo ve Başlık
        var pnlBrandHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = Color.Transparent,
            Padding = new Padding(6, 4, 6, 8)
        };

        var lblAppIcon = new Label
        {
            Text = "🌤️",
            Location = new Point(2, 6),
            Size = new Size(34, 34),
            Font = new Font("Segoe UI Emoji", 16f)
        };

        var lblLogoTitle = new Label
        {
            Text = "HaYTooL Weather",
            Location = new Point(40, 6),
            Size = new Size(168, 24),
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = ColorTextPrimary
        };

        var lblAppSubtitle = new Label
        {
            Text = "Windows Tray Companion",
            Location = new Point(42, 30),
            Size = new Size(166, 18),
            Font = new Font("Segoe UI", 8f),
            ForeColor = ColorAccentCyan
        };

        pnlBrandHeader.Controls.AddRange(new Control[] { lblAppIcon, lblLogoTitle, lblAppSubtitle });

        // Navigasyon Buton Grubu
        var pnlNavButtons = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 12, 0, 0)
        };

        _btnTabLocation = CreateSidebarNavButton("📍  " + LocalizationService.Get("tab_location"));
        _btnTabAppearance = CreateSidebarNavButton("🎨  " + LocalizationService.Get("tab_appearance"));
        _btnTabGeneral = CreateSidebarNavButton("⚙️  " + LocalizationService.Get("tab_general"));
        _btnTabLanguage = CreateSidebarNavButton("🌐  " + LocalizationService.Get("tab_language"));
        _btnTabAbout = CreateSidebarNavButton("ℹ️  " + LocalizationService.Get("tab_about"));

        _btnTabLocation.Click += (s, e) => SwitchTab(_btnTabLocation, _pnlLocationTab!);
        _btnTabAppearance.Click += (s, e) => SwitchTab(_btnTabAppearance, _pnlAppearanceTab!);
        _btnTabGeneral.Click += (s, e) => SwitchTab(_btnTabGeneral, _pnlGeneralTab!);
        _btnTabLanguage.Click += (s, e) => SwitchTab(_btnTabLanguage, _pnlLanguageTab!);
        _btnTabAbout.Click += (s, e) => SwitchTab(_btnTabAbout, _pnlAboutTab!);

        pnlNavButtons.Controls.AddRange(new Control[] {
            _btnTabAbout, _btnTabLanguage, _btnTabGeneral, _btnTabAppearance, _btnTabLocation
        });

        pnlSidebar.Controls.Add(pnlNavButtons);
        pnlSidebar.Controls.Add(pnlBrandHeader);

        // =========================================================================
        // 2. ALT DURUM VE AKSİYON BARI (Bottom Bar)
        // =========================================================================
        var pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            BackColor = Color.FromArgb(16, 20, 30),
            Padding = new Padding(24, 10, 24, 10)
        };
        pnlBottom.Paint += (s, e) =>
        {
            using var pen = new Pen(ColorBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, pnlBottom.Width, 0);
        };

        var lblFooterHint = new Label
        {
            Text = "🔒 %100 Yerel Veri • Sıfır Telemetri • Anında Canlı Önizleme",
            Dock = DockStyle.Left,
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = ColorTextMuted,
            Padding = new Padding(0, 8, 0, 0)
        };

        _btnClose = new Button
        {
            Text = LocalizationService.Get("btn_close"),
            Dock = DockStyle.Right,
            Width = 140,
            BackColor = ColorAccentBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        _btnClose.FlatAppearance.BorderSize = 0;
        _btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 140, 255);
        _btnClose.Click += (s, e) => Close();

        pnlBottom.Controls.Add(lblFooterHint);
        pnlBottom.Controls.Add(_btnClose);

        // =========================================================================
        // 3. SAĞ ÇALIŞMA ALANI (Main Content Area)
        // =========================================================================
        _pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorBgMica,
            Padding = new Padding(26, 20, 26, 16)
        };

        // Sekmeleri Sıfırdan İnşa Et
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

    private Button CreateSidebarNavButton(string text)
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
        btn.FlatAppearance.MouseOverBackColor = ColorBgCardHover;
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
        _currentActiveTabBtn.BackColor = ColorAccentBlue;
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

        _btnTabLocation.Text = "📍  " + LocalizationService.Get("tab_location");
        _btnTabAppearance.Text = "🎨  " + LocalizationService.Get("tab_appearance");
        _btnTabGeneral.Text = "⚙️  " + LocalizationService.Get("tab_general");
        _btnTabLanguage.Text = "🌐  " + LocalizationService.Get("tab_language");
        _btnTabAbout.Text = "ℹ️  " + LocalizationService.Get("tab_about");

        BuildLocationTab();
        BuildAppearanceTab();
        BuildGeneralTab();
        BuildLanguageTab();
        BuildAboutTab();

        SwitchTab(_btnTabLanguage, _pnlLanguageTab!);
    }

    // =========================================================================
    // 1. SEKME: 📍 KONUM VE ŞEHİR KEŞFİ
    // =========================================================================
    private void BuildLocationTab()
    {
        _pnlLocationTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 10, 10) };
        int y = 0;
        int contentW = 636;

        var lblHeader = new Label
        {
            Text = LocalizationService.Get("loc_title"),
            Location = new Point(0, y),
            Size = new Size(contentW, 28),
            Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
            ForeColor = ColorTextPrimary
        };
        y += 36;

        // 1. Aktif Konum Kartı (Hero Style Card)
        var pnlCurrentLoc = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 76),
            BackColor = ColorBgCard,
            Padding = new Padding(16, 12, 16, 12)
        };
        pnlCurrentLoc.Paint += (s, e) => DrawFluentCardBorder(e.Graphics, pnlCurrentLoc.ClientRectangle, ColorEmerald);

        var lblBadge = new Label
        {
            Text = "● " + LocalizationService.Get("loc_active_badge"),
            Location = new Point(16, 10),
            Size = new Size(200, 16),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = ColorEmerald
        };

        _lblCurrentLocationName = new Label
        {
            Text = $"{WeatherService.FormatLocationTitle(_settings.City, _settings.District)}, {_settings.Country}",
            Location = new Point(16, 28),
            Size = new Size(380, 32),
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = ColorTextPrimary
        };

        _lblCurrentLocationCoords = new Label
        {
            Text = $"🌐 {_settings.Latitude:F4}°N, {_settings.Longitude:F4}°E",
            Location = new Point(contentW - 240, 32),
            Size = new Size(224, 24),
            TextAlign = ContentAlignment.TopRight,
            Font = new Font("Segoe UI", 9f),
            ForeColor = ColorTextSecondary
        };

        pnlCurrentLoc.Controls.AddRange(new Control[] { lblBadge, _lblCurrentLocationName, _lblCurrentLocationCoords });
        y += 88;

        // 2. Hızlı Başkentler Seçici (Fluent Chips)
        var lblQuick = new Label
        {
            Text = LocalizationService.Get("loc_popular"),
            Location = new Point(0, y),
            Size = new Size(contentW, 20),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = ColorTextSecondary
        };
        y += 24;

        var flpChips = new FlowLayoutPanel
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 40),
            BackColor = Color.Transparent,
            AutoScroll = false
        };

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
                BackColor = query == "Istanbul" ? ColorAccentBlue : ColorBgCard,
                ForeColor = ColorTextPrimary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(10, 0, 10, 0)
            };
            chip.FlatAppearance.BorderSize = 0;
            chip.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 140, 255);
            chip.Click += async (s, e) => await QuickSelectCity(query);
            flpChips.Controls.Add(chip);
        }
        y += 48;

        // 3. Arama Çubuğu (Search Container)
        var lblSearchTitle = new Label
        {
            Text = LocalizationService.Get("loc_search_title"),
            Location = new Point(0, y),
            Size = new Size(contentW, 20),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = ColorTextSecondary
        };
        y += 24;

        var pnlSearchBox = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 42),
            BackColor = ColorBgInput
        };
        pnlSearchBox.Paint += (s, e) => DrawFluentCardBorder(e.Graphics, pnlSearchBox.ClientRectangle, ColorBorder);

        _txtSearch = new TextBox
        {
            Location = new Point(16, 10),
            Size = new Size(contentW - 140, 24),
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
            Location = new Point(contentW - 116, 5),
            Size = new Size(110, 32),
            BackColor = ColorAccentBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 140, 255);
        btnSearch.Click += async (s, e) => await PerformSearchAsync();

        pnlSearchBox.Controls.Add(_txtSearch);
        pnlSearchBox.Controls.Add(btnSearch);
        y += 50;

        // 4. Sonuç Listesi Paneli
        _lstSearchResults = new ListBox
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 136),
            BackColor = ColorBgCard,
            ForeColor = ColorTextPrimary,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9.5f),
            ItemHeight = 28
        };
        y += 144;

        var lblLocationStatus = new Label
        {
            Text = "",
            Location = new Point(0, y + 6),
            Size = new Size(400, 24),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorEmerald
        };
        _lstSearchResults.DoubleClick += async (s, e) => await ApplySelectedLocationAsync(lblLocationStatus);

        var btnApplyLocation = new Button
        {
            Text = LocalizationService.Get("loc_apply_btn"),
            Location = new Point(contentW - 220, y),
            Size = new Size(220, 36),
            BackColor = ColorEmerald,
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
            _lblCurrentLocationCoords.Text = $"🌐 {_settings.Latitude:F4}°N, {_settings.Longitude:F4}°E";

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
                statusLabel.ForeColor = ColorEmerald;
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
    // 2. SEKME: 🎨 GÖRÜNÜM VE BOYUT (SIFIR SCROLL / MÜKEMMEL 2 SÜTUN GRİD)
    // =========================================================================
    private void BuildAppearanceTab()
    {
        _pnlAppearanceTab = new Panel { AutoScroll = false, BackColor = Color.Transparent };
        int y = 0;
        int contentW = 636;

        var lblHeader = new Label
        {
            Text = LocalizationService.Get("app_title"),
            Location = new Point(0, y),
            Size = new Size(contentW, 28),
            Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
            ForeColor = ColorTextPrimary
        };
        y += 34;

        // 1. Canlı Görev Çubuğu Önizleme Kartı
        var pnlPreviewBox = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 66),
            BackColor = ColorBgCard,
            Padding = new Padding(14)
        };
        pnlPreviewBox.Paint += (s, e) => DrawFluentCardBorder(e.Graphics, pnlPreviewBox.ClientRectangle, ColorBorderCard);

        var lblPreviewTitle = new Label
        {
            Text = "🔴 " + LocalizationService.Get("app_live_preview"),
            Location = new Point(14, 6),
            Size = new Size(260, 16),
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = ColorEmerald
        };

        _pbLivePreview = new PictureBox
        {
            Location = new Point(14, 24),
            Size = new Size(contentW - 28, 34),
            BackColor = Color.FromArgb(12, 16, 24),
            BorderStyle = BorderStyle.None
        };
        _pbLivePreview.Paint += (s, e) => PaintLivePreview(e.Graphics);

        pnlPreviewBox.Controls.Add(lblPreviewTitle);
        pnlPreviewBox.Controls.Add(_pbLivePreview);
        y += 74;

        // 2. Mod & Şehir Teması Seçim Kartı
        var pnlSelectors = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 72),
            BackColor = ColorBgCard,
            Padding = new Padding(14, 10, 14, 10)
        };
        pnlSelectors.Paint += (s, e) => DrawFluentCardBorder(e.Graphics, pnlSelectors.ClientRectangle, ColorBorderCard);

        var lblMode = new Label
        {
            Text = LocalizationService.Get("app_mode"),
            Location = new Point(14, 12),
            Size = new Size(130, 22),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorTextSecondary
        };

        _cmbTrayMode = new ComboBox
        {
            Location = new Point(145, 10),
            Size = new Size(contentW - 165, 26),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = ColorBgInput,
            ForeColor = ColorTextPrimary,
            FlatStyle = FlatStyle.Flat
        };
        _cmbTrayMode.Items.Add("⭐ Çift Alan (Solda Tam Boy İkon + Sağda Dev Sıcaklık)");
        _cmbTrayMode.Items.Add("🌡️ Sadece Dev Sıcaklık Derecesi (Tek Alan)");
        _cmbTrayMode.Items.Add("🌤️ Sadece Hava Durumu İkonu (Tek Alan)");
        _cmbTrayMode.Items.Add("🔹 Tek Alan Kompakt (Küçük İkon + Derece)");
        _cmbTrayMode.SelectedIndex = _settings.TrayDisplayMode.ToLowerInvariant() switch { "temp_only" => 1, "weather_only" => 2, "single_compact" => 3, _ => 0 };
        _cmbTrayMode.SelectedIndexChanged += (s, e) => ApplyLiveChanges();

        var lblTheme = new Label
        {
            Text = "🏙️ Şehir Teması:",
            Location = new Point(14, 42),
            Size = new Size(130, 22),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorAccentCyan
        };

        _cmbCityTheme = new ComboBox
        {
            Location = new Point(145, 40),
            Size = new Size(contentW - 165, 26),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = ColorBgInput,
            ForeColor = ColorTextPrimary,
            FlatStyle = FlatStyle.Flat
        };
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

        pnlSelectors.Controls.AddRange(new Control[] { lblMode, _cmbTrayMode, lblTheme, _cmbCityTheme });
        y += 80;

        // 3. YAN YANA 2 SÜTUN KARTLARI (Simetrik 311px + 14px Boşluk)
        int colWidth = (contentW - 14) / 2; // 311px
        int colGap = 14;
        int cardHeight = 310;

        // SOL KART: HAVA DURUMU SİMGESİ
        var pnlWeather = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(colWidth, cardHeight),
            BackColor = ColorBgCard,
            Padding = new Padding(16)
        };
        pnlWeather.Paint += (s, e) => DrawFluentCardBorder(e.Graphics, pnlWeather.ClientRectangle, ColorGold);

        var lblWTitle = new Label
        {
            Text = "🌤️ " + LocalizationService.Get("app_weather_icon"),
            Location = new Point(14, 12),
            Size = new Size(colWidth - 28, 24),
            ForeColor = ColorGold,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
        };
        int gy1 = 44;

        var lblWScale = new Label { Text = LocalizationService.Get("app_icon_size"), Location = new Point(14, gy1), Size = new Size(180, 20), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _lblWeatherScaleVal = new Label { Text = $"%{_settings.WeatherIconScale}", Location = new Point(colWidth - 84, gy1), Size = new Size(70, 20), TextAlign = ContentAlignment.TopRight, ForeColor = ColorGold, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy1 += 24;
        _tbWeatherScale = new TrackBar { Location = new Point(10, gy1), Size = new Size(colWidth - 20, 30), Minimum = 40, Maximum = 200, Value = Math.Clamp(_settings.WeatherIconScale, 40, 200), TickStyle = TickStyle.None };
        _tbWeatherScale.Scroll += (s, e) => { _lblWeatherScaleVal.Text = $"%{_tbWeatherScale.Value}"; ApplyLiveChanges(); };
        gy1 += 44;

        var lblWBg = new Label { Text = LocalizationService.Get("app_bg_color"), Location = new Point(14, gy1 + 3), Size = new Size(110, 22), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _pnlWeatherColorPreview = new Panel { Location = new Point(125, gy1), Size = new Size(32, 26), BackColor = IconGenerator.ParseHexColor(_settings.WeatherBgColor), BorderStyle = BorderStyle.FixedSingle };
        var btnWBgColor = new Button { Text = LocalizationService.Get("app_pick_color"), Location = new Point(165, gy1), Size = new Size(colWidth - 180, 26), BackColor = Color.FromArgb(40, 52, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5f) };
        btnWBgColor.FlatAppearance.BorderSize = 0;
        btnWBgColor.Click += (s, e) => PickColor(c => _settings.WeatherBgColor = c, _settings.WeatherBgColor, _pnlWeatherColorPreview);
        gy1 += 48;

        var lblWOp = new Label { Text = LocalizationService.Get("app_bg_opacity"), Location = new Point(14, gy1), Size = new Size(180, 20), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _lblWeatherOpacityVal = new Label { Text = $"%{_settings.WeatherBgOpacity}", Location = new Point(colWidth - 84, gy1), Size = new Size(70, 20), TextAlign = ContentAlignment.TopRight, ForeColor = ColorAccentCyan, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy1 += 24;
        _tbWeatherOpacity = new TrackBar { Location = new Point(10, gy1), Size = new Size(colWidth - 20, 30), Minimum = 0, Maximum = 100, Value = Math.Clamp(_settings.WeatherBgOpacity, 0, 100), TickStyle = TickStyle.None };
        _tbWeatherOpacity.Scroll += (s, e) => { _lblWeatherOpacityVal.Text = $"%{_tbWeatherOpacity.Value}"; ApplyLiveChanges(); };

        pnlWeather.Controls.AddRange(new Control[] { lblWTitle, lblWScale, _lblWeatherScaleVal, _tbWeatherScale, lblWBg, _pnlWeatherColorPreview, btnWBgColor, lblWOp, _lblWeatherOpacityVal, _tbWeatherOpacity });

        // SAĞ KART: SICAKLIK DERECESİ
        var pnlTemp = new Panel
        {
            Location = new Point(colWidth + colGap, y),
            Size = new Size(colWidth, cardHeight),
            BackColor = ColorBgCard,
            Padding = new Padding(16)
        };
        pnlTemp.Paint += (s, e) => DrawFluentCardBorder(e.Graphics, pnlTemp.ClientRectangle, ColorAccentCyan);

        var lblTTitle = new Label
        {
            Text = "🌡️ " + LocalizationService.Get("app_temp_text"),
            Location = new Point(14, 12),
            Size = new Size(colWidth - 28, 24),
            ForeColor = ColorAccentCyan,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
        };
        int gy2 = 44;

        var lblTScale = new Label { Text = LocalizationService.Get("app_text_size"), Location = new Point(14, gy2), Size = new Size(180, 20), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _lblTempScaleVal = new Label { Text = $"%{_settings.TempTextScale}", Location = new Point(colWidth - 84, gy2), Size = new Size(70, 20), TextAlign = ContentAlignment.TopRight, ForeColor = ColorAccentCyan, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy2 += 24;
        _tbTempScale = new TrackBar { Location = new Point(10, gy2), Size = new Size(colWidth - 20, 30), Minimum = 40, Maximum = 200, Value = Math.Clamp(_settings.TempTextScale, 40, 200), TickStyle = TickStyle.None };
        _tbTempScale.Scroll += (s, e) => { _lblTempScaleVal.Text = $"%{_tbTempScale.Value}"; ApplyLiveChanges(); };
        gy2 += 44;

        var lblTBg = new Label { Text = LocalizationService.Get("app_bg_color"), Location = new Point(14, gy2 + 3), Size = new Size(110, 22), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _pnlTempColorPreview = new Panel { Location = new Point(125, gy2), Size = new Size(32, 26), BackColor = IconGenerator.ParseHexColor(_settings.TempBgColor), BorderStyle = BorderStyle.FixedSingle };
        var btnTBgColor = new Button { Text = LocalizationService.Get("app_pick_color"), Location = new Point(165, gy2), Size = new Size(colWidth - 180, 26), BackColor = Color.FromArgb(40, 52, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5f) };
        btnTBgColor.FlatAppearance.BorderSize = 0;
        btnTBgColor.Click += (s, e) => PickColor(c => _settings.TempBgColor = c, _settings.TempBgColor, _pnlTempColorPreview);
        gy2 += 48;

        var lblTOp = new Label { Text = LocalizationService.Get("app_bg_opacity"), Location = new Point(14, gy2), Size = new Size(180, 20), ForeColor = ColorTextPrimary, Font = new Font("Segoe UI", 9f) };
        _lblTempOpacityVal = new Label { Text = $"%{_settings.TempBgOpacity}", Location = new Point(colWidth - 84, gy2), Size = new Size(70, 20), TextAlign = ContentAlignment.TopRight, ForeColor = ColorAccentCyan, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        gy2 += 24;
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
            ForeColor = Color.FromArgb(255, 235, 140)
        };
        _chkHighContrast.CheckedChanged += (s, e) => ApplyLiveChanges();

        pnlTemp.Controls.AddRange(new Control[] { lblTTitle, lblTScale, _lblTempScaleVal, _tbTempScale, lblTBg, _pnlTempColorPreview, btnTBgColor, lblTOp, _lblTempOpacityVal, _tbTempOpacity, _chkHighContrast });

        _pnlAppearanceTab.Controls.AddRange(new Control[] {
            lblHeader, pnlPreviewBox, pnlSelectors, pnlWeather, pnlTemp
        });
    }

    private void PaintLivePreview(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(12, 16, 24));

        // Windows Görev Çubuğu Saati Çizimi
        using var fontClock = new Font("Segoe UI", 9f, FontStyle.Regular);
        using var brushClock = new SolidBrush(ColorTextSecondary);
        string timeStr = DateTime.Now.ToString("HH:mm");
        g.DrawString(timeStr, fontClock, brushClock, 550, 8);

        // Canlı Görev Çubuğu Simgeleri
        int temp = _weatherService.LastWeatherData != null ? (int)Math.Round(_weatherService.LastWeatherData.Temperature) : 24;
        int weatherCode = _weatherService.LastWeatherData?.WeatherCode ?? 0;
        bool isDay = _weatherService.LastWeatherData?.IsDay ?? true;

        var (wIcon, wHicon) = IconGenerator.GenerateWeatherOnlyIcon(weatherCode, isDay, _settings.WeatherIconScale, _settings.WeatherBgColor, _settings.WeatherBgOpacity);
        var (tIcon, tHicon) = IconGenerator.GenerateTemperatureOnlyIcon(temp, _settings.HighContrastTrayIcon, _settings.TempTextScale, _settings.TempBgColor, _settings.TempBgOpacity);

        g.DrawIcon(wIcon, new Rectangle(465, 2, 30, 30));
        g.DrawIcon(tIcon, new Rectangle(503, 2, 30, 30));

        using var fontLabel = new Font("Segoe UI", 8.5f, FontStyle.Italic);
        using var brushLabel = new SolidBrush(ColorTextMuted);
        g.DrawString("Masaüstü Görev Çubuğu Önizlemesi ➔", fontLabel, brushLabel, 220, 9);

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
    // 3. SEKME: ⚙️ GENEL VE SİSTEM TERCİHLERİ
    // =========================================================================
    private void BuildGeneralTab()
    {
        _pnlGeneralTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 10, 10) };
        int y = 0;
        int contentW = 636;

        var lblHeader = new Label
        {
            Text = LocalizationService.Get("gen_title"),
            Location = new Point(0, y),
            Size = new Size(contentW, 28),
            Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
            ForeColor = ColorTextPrimary
        };
        y += 36;

        // Ayar Kartı (Fluent Glass Panel)
        var pnlCard = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 360),
            BackColor = ColorBgCard,
            Padding = new Padding(20)
        };
        pnlCard.Paint += (s, e) => DrawFluentCardBorder(e.Graphics, pnlCard.ClientRectangle, ColorBorderCard);
        int cy = 16;

        // Hava Durumu Modeli / Sağlayıcı
        var lblProvider = new Label
        {
            Text = LocalizationService.Get("gen_weather_provider"),
            Location = new Point(16, cy),
            Size = new Size(contentW - 32, 20),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = ColorTextSecondary
        };
        cy += 24;

        _cmbProvider = new ComboBox
        {
            Location = new Point(16, cy),
            Size = new Size(contentW - 32, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = ColorBgInput,
            ForeColor = ColorTextPrimary,
            FlatStyle = FlatStyle.Flat
        };
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
        cy += 50;

        // Güncelleme Sıklığı
        var lblInterval = new Label
        {
            Text = LocalizationService.Get("gen_update_interval"),
            Location = new Point(16, cy),
            Size = new Size(contentW - 32, 20),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = ColorTextSecondary
        };
        cy += 24;

        _cmbInterval = new ComboBox
        {
            Location = new Point(16, cy),
            Size = new Size(contentW - 32, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = ColorBgInput,
            ForeColor = ColorTextPrimary,
            FlatStyle = FlatStyle.Flat
        };
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("min_30"), 0));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_1"), 1));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_3"), 3));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_6"), 6));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_12"), 12));
        _cmbInterval.Items.Add(new IntervalItem(LocalizationService.Get("hours_24"), 24));
        SelectIntervalItem(_settings.UpdateIntervalHours);
        _cmbInterval.SelectedIndexChanged += (s, e) => ApplyLiveChanges();
        cy += 50;

        // Yan Yana Birim Seçimleri (Simetrik 2 Sütun)
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
    // 4. SEKME: 🌐 DİL TERCİHLERİ (7 DİL ANINDA GEÇİŞ)
    // =========================================================================
    private void BuildLanguageTab()
    {
        if (_pnlLanguageTab == null)
            _pnlLanguageTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 10, 10) };
        else
            _pnlLanguageTab.Controls.Clear();

        int y = 0;
        int contentW = 636;

        var lblHeader = new Label
        {
            Text = "🌐 Dil & Yerelleştirme / Language",
            Location = new Point(0, y),
            Size = new Size(contentW, 28),
            Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
            ForeColor = ColorTextPrimary
        };
        y += 36;

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
                Text = $"  {flag}   {name}  ({code.ToUpper()})" + (isSelected ? "   ✓  [Seçili / Active]" : ""),
                Location = new Point(0, y),
                Size = new Size(contentW, 46),
                BackColor = isSelected ? ColorAccentBlue : ColorBgCard,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                Padding = new Padding(18, 0, 0, 0)
            };
            btnLang.FlatAppearance.BorderSize = 0;
            btnLang.FlatAppearance.MouseOverBackColor = isSelected ? Color.FromArgb(20, 140, 255) : ColorBgCardHover;
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
    // 5. SEKME: ℹ️ HAKKINDA VE GÜNCELLEMELER
    // =========================================================================
    private void BuildAboutTab()
    {
        _pnlAboutTab = new Panel { AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 10, 10) };
        int y = 0;
        int contentW = 636;

        var lblHeader = new Label
        {
            Text = LocalizationService.Get("about_header"),
            Location = new Point(0, y),
            Size = new Size(contentW, 28),
            Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
            ForeColor = ColorTextPrimary
        };
        y += 36;

        var pnlAboutCard = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(contentW, 350),
            BackColor = ColorBgCard,
            Padding = new Padding(24)
        };
        pnlAboutCard.Paint += (s, e) => DrawFluentCardBorder(e.Graphics, pnlAboutCard.ClientRectangle, ColorBorderCard);

        var lblTitle = new Label { Text = "HaYTooL Weather", Location = new Point(20, 16), Size = new Size(420, 28), Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = ColorTextPrimary };
        var lblVer = new Label { Text = string.Format(LocalizationService.Get("about_ver"), AppVersion.GetCurrentVersion()), Location = new Point(20, 46), Size = new Size(500, 20), Font = new Font("Segoe UI", 9f), ForeColor = ColorTextSecondary };

        var lblDev = new Label { Text = LocalizationService.Get("about_dev"), Location = new Point(20, 80), Size = new Size(420, 22), Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = ColorAccentCyan };

        // 1. E-Posta
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

        // 2. X (Twitter)
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

        // 3. GitHub
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

        // 4. Güncelleme Denetimi
        var btnCheckUpdate = new Button
        {
            Text = LocalizationService.Get("about_check_updates"),
            Location = new Point(20, 196),
            Size = new Size(190, 34),
            BackColor = Color.FromArgb(36, 48, 68),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnCheckUpdate.FlatAppearance.BorderSize = 0;
        btnCheckUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 62, 88);

        _chkAutoCheckUpdates = new CheckBox
        {
            Text = LocalizationService.Get("gen_auto_check_updates"),
            Location = new Point(220, 200),
            Size = new Size(390, 26),
            Checked = _settings.AutoCheckUpdates,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorEmerald
        };
        _chkAutoCheckUpdates.CheckedChanged += (s, e) => ApplyLiveChanges();

        var lblUpdateStatus = new Label
        {
            Text = "",
            Location = new Point(20, 238),
            Size = new Size(580, 22),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorEmerald
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
                lblUpdateStatus.ForeColor = ColorGold;
                lblUpdateStatus.Text = string.Format(LocalizationService.Get("about_update_available"), result.LatestVersion);

                var btnDownload = new Button
                {
                    Text = LocalizationService.Get("about_update_btn"),
                    Location = new Point(20, 266),
                    Size = new Size(200, 32),
                    BackColor = ColorAccentBlue,
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
                lblUpdateStatus.ForeColor = ColorEmerald;
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

    private static void DrawFluentCardBorder(Graphics g, Rectangle rect, Color borderColor)
    {
        using var pen = new Pen(borderColor, 1);
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
