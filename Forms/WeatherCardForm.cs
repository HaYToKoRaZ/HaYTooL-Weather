using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using HaYTooLWeather.Models;
using HaYTooLWeather.Services;

namespace HaYTooLWeather.Forms;

/// <summary>
/// Sistem tepsisi simgesine tıklandığında açılan modern, cam efektli detaylı hava durumu kartı.
/// </summary>
public class WeatherCardForm : Form
{
    private readonly ProcessedWeatherData _weather;
    private readonly AppSettings _settings;
    private readonly Action _onRefreshRequested;

    public WeatherCardForm(ProcessedWeatherData weather, AppSettings settings, Action onRefreshRequested)
    {
        _weather = weather;
        _settings = settings;
        _onRefreshRequested = onRefreshRequested;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        Size = new Size(390, 500);
        BackColor = Color.FromArgb(24, 27, 34);
        ForeColor = Color.White;
        DoubleBuffered = true;

        // Dışarı tıklandığında veya ESC basıldığında kapat
        Deactivate += (s, e) => Close();
        KeyPreview = true;
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

        InitializeCardUI();
        PositionNearTaskbar();
    }

    private void PositionNearTaskbar()
    {
        var screen = Screen.PrimaryScreen?.WorkingArea ?? Screen.GetWorkingArea(this);
        int x = screen.Right - Width - 12;
        int y = screen.Bottom - Height - 12;
        Location = new Point(x, y);
    }

    private void InitializeCardUI()
    {
        // 1. Üst Başlık Paneli (Konum + Web Linki + Yenile + Kapat)
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(18, 14, 18, 0),
            BackColor = Color.Transparent
        };

        var lblLocation = new Label
        {
            Text = $"📍 {_weather.LocationName}",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(210, 24),
            Location = new Point(16, 14)
        };

        var lblLastUpdated = new Label
        {
            Text = $"{LocalizationService.Get("last_update")}: {_weather.LastUpdated:HH:mm}",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(150, 160, 175),
            AutoSize = false,
            Size = new Size(210, 18),
            Location = new Point(18, 38)
        };

        // 🌐 Web'de Detaylı Gör Butonu
        var btnWeb = new Button
        {
            Text = "🌐",
            Size = new Size(32, 32),
            Location = new Point(260, 14),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(35, 42, 56),
            ForeColor = Color.FromArgb(0, 200, 255),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10f)
        };
        btnWeb.FlatAppearance.BorderSize = 0;
        var tt = new ToolTip();
        tt.SetToolTip(btnWeb, "Web'de / Tarayıcıda Detaylı İncele (Google Weather)");
        btnWeb.Click += (s, e) =>
        {
            try
            {
                string searchUrl = $"https://www.google.com/search?q=weather+{Uri.EscapeDataString(_weather.LocationName)}";
                Process.Start(new ProcessStartInfo { FileName = searchUrl, UseShellExecute = true });
            }
            catch { }
        };

        // 🔄 Yenile Butonu
        var btnRefresh = new Button
        {
            Text = "🔄",
            Size = new Size(32, 32),
            Location = new Point(300, 14),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 45, 56),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10f)
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        tt.SetToolTip(btnRefresh, LocalizationService.Get("menu_refresh_now"));
        btnRefresh.Click += (s, e) => { _onRefreshRequested(); Close(); };

        // ✕ Kapat Butonu
        var btnClose = new Button
        {
            Text = "✕",
            Size = new Size(32, 32),
            Location = new Point(340, 14),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 45, 56),
            ForeColor = Color.FromArgb(180, 190, 205),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (s, e) => Close();

        pnlHeader.Controls.AddRange(new Control[] { lblLocation, lblLastUpdated, btnWeb, btnRefresh, btnClose });

        // 2. Ana Hava Durumu Paneli (Büyük Sıcaklık, Açıklama, Hissedilen)
        var pnlHero = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(18, 0, 18, 0),
            BackColor = Color.Transparent
        };

        var unitSymbol = _settings.TemperatureUnit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase) ? "°F" : "°C";

        var lblTemp = new Label
        {
            Text = $"{_weather.Temperature:0}{unitSymbol}",
            Font = new Font("Segoe UI", 36f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(16, 2)
        };

        var conditionText = LocalizationService.GetWeatherDescription(_weather.WeatherCode);
        var lblCondition = new Label
        {
            Text = conditionText,
            Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 180, 255),
            AutoSize = true,
            Location = new Point(175, 14)
        };

        var lblFeelsLike = new Label
        {
            Text = $"{LocalizationService.Get("feels_like")}: {_weather.ApparentTemperature:0}{unitSymbol}",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(170, 180, 195),
            AutoSize = true,
            Location = new Point(175, 42)
        };

        pnlHero.Controls.AddRange(new Control[] { lblTemp, lblCondition, lblFeelsLike });

        // 3. Metrik Rozetleri (Nem, Rüzgar, Yağış İhtimali)
        var pnlMetrics = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 65,
            Padding = new Padding(16, 0, 16, 0),
            BackColor = Color.Transparent,
            WrapContents = false
        };

        var windUnit = _settings.WindSpeedUnit.Equals("mph", StringComparison.OrdinalIgnoreCase) ? "mph" : "km/h";
        var metric1 = CreateMetricBadge("💧 " + LocalizationService.Get("humidity"), $"%{_weather.Humidity}");
        var metric2 = CreateMetricBadge("💨 " + LocalizationService.Get("wind"), $"{_weather.WindSpeed} {windUnit}");
        var metric3 = CreateMetricBadge("🌧️ " + LocalizationService.Get("rain_chance"), $"%{_weather.DailyForecasts.FirstOrDefault()?.RainChance ?? 0}");

        pnlMetrics.Controls.AddRange(new Control[] { metric1, metric2, metric3 });

        // 4. 7 Günlük Tahmin Başlık Çubuğu (Yağış ve Sıcaklık Sütun Açıklamalı)
        var pnlForecastHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 28,
            Padding = new Padding(16, 4, 16, 0),
            BackColor = Color.FromArgb(20, 23, 30)
        };

        var lblTitleLeft = new Label
        {
            Text = $"📅 {LocalizationService.Get("daily_7")}",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 215, 235),
            Location = new Point(16, 4),
            AutoSize = true
        };

        var lblLegendRain = new Label
        {
            Text = "💧 Yağış",
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(80, 190, 255),
            Location = new Point(220, 5),
            AutoSize = true
        };

        var lblLegendTemp = new Label
        {
            Text = "🌡️ Sıcaklık",
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(220, 230, 245),
            Location = new Point(290, 5),
            AutoSize = true
        };

        pnlForecastHeader.Controls.AddRange(new Control[] { lblTitleLeft, lblLegendRain, lblLegendTemp });

        // 5. 7 Günlük Tahmin Listesi
        var pnlDaily = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 4, 16, 12),
            AutoScroll = true
        };

        int itemY = 4;
        foreach (var daily in _weather.DailyForecasts)
        {
            var dayRow = CreateDailyForecastRow(daily, unitSymbol);
            dayRow.Location = new Point(0, itemY);
            pnlDaily.Controls.Add(dayRow);
            itemY += 34;
        }

        // Panelleri Forma Ekle (Footer kaldırıldı!)
        Controls.Add(pnlDaily);
        Controls.Add(pnlForecastHeader);
        Controls.Add(pnlMetrics);
        Controls.Add(pnlHero);
        Controls.Add(pnlHeader);
    }

    private Panel CreateMetricBadge(string title, string value)
    {
        var pnl = new Panel
        {
            Size = new Size(114, 56),
            Margin = new Padding(2),
            BackColor = Color.FromArgb(35, 39, 48)
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 7.8f, FontStyle.Regular),
            ForeColor = Color.FromArgb(160, 170, 185),
            Dock = DockStyle.Top,
            Height = 22,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblVal = new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };

        pnl.Controls.Add(lblVal);
        pnl.Controls.Add(lblTitle);
        return pnl;
    }

    private Panel CreateDailyForecastRow(ProcessedDailyForecast daily, string unitSymbol)
    {
        var row = new Panel
        {
            Size = new Size(345, 30),
            BackColor = Color.Transparent
        };

        var isToday = daily.Date.Date == DateTime.Today;
        var dayName = isToday ? LocalizationService.Get("today") : LocalizationService.GetDayName(daily.Date.DayOfWeek);

        var lblDay = new Label
        {
            Text = dayName,
            Font = new Font("Segoe UI", 9.5f, isToday ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = isToday ? Color.FromArgb(0, 190, 255) : Color.White,
            Size = new Size(68, 26),
            Location = new Point(4, 3),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var desc = LocalizationService.GetWeatherDescription(daily.WeatherCode);
        var lblDesc = new Label
        {
            Text = desc,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(160, 175, 195),
            Size = new Size(130, 26),
            Location = new Point(76, 3),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Yağış İhtimali (% ile)
        var rainText = daily.RainChance > 0 ? $"💧 %{daily.RainChance}" : "-";
        var lblRain = new Label
        {
            Text = rainText,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = daily.RainChance > 0 ? Color.FromArgb(80, 200, 255) : Color.FromArgb(100, 110, 130),
            Size = new Size(55, 26),
            Location = new Point(208, 3),
            TextAlign = ContentAlignment.MiddleRight
        };

        // En Yüksek / En Düşük Sıcaklık
        var lblTemps = new Label
        {
            Text = $"{daily.MaxTemp:0}° / {daily.MinTemp:0}°",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            Size = new Size(72, 26),
            Location = new Point(268, 3),
            TextAlign = ContentAlignment.MiddleRight
        };

        row.Controls.AddRange(new Control[] { lblDay, lblDesc, lblRain, lblTemps });
        return row;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        // İnce ve estetik modern kenarlık
        using var pen = new Pen(Color.FromArgb(60, 70, 85), 1.5f);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
