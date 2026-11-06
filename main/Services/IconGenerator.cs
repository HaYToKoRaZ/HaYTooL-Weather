using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace HaYTooLWeather.Services;

/// <summary>
/// Sistem tepsisi (System Tray) için tam boy hava durumu ikonu, dev boyutta sıcaklık derecesi, bağımsız ölçekler ve özelleştirilebilir arka plan rengi/opaklığı üreten servis.
/// </summary>
public static class IconGenerator
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool DestroyIcon(IntPtr handle);

    /// <summary>
    /// Sadece hava durumu simgesini tam boy, belirtilen ölçekte ve arka plan rengi/opaklığı ile çizer.
    /// </summary>
    public static (Icon Icon, IntPtr Handle) GenerateWeatherOnlyIcon(int weatherCode, bool isDay, int scalePercent = 100, string hexBgColor = "#000000", int bgOpacity = 0)
    {
        const int size = 64;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        // 1. Arka Planı Çiz (Eğer opaklık > 0 ise)
        DrawSlotBackground(g, size, hexBgColor, bgOpacity);

        // 2. Hava Durumu Simgesini Ölçekli Çiz
        float scale = Math.Clamp(scalePercent / 100.0f, 0.50f, 1.85f);
        if (Math.Abs(scale - 1.0f) > 0.01f)
        {
            g.TranslateTransform(size / 2f, size / 2f);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-size / 2f, -size / 2f);
        }

        DrawFullWeatherSymbol(g, weatherCode, isDay, size);

        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);
        return (icon, hIcon);
    }

    /// <summary>
    /// Sıcaklık derecesini belirtilen ölçekte, yüksek kontrast/canlı renk ve arka plan rengi/opaklığı ile çizer.
    /// </summary>
    public static (Icon Icon, IntPtr Handle) GenerateTemperatureOnlyIcon(double temperature, bool highContrast, int scalePercent = 100, string hexBgColor = "#000000", int bgOpacity = 0)
    {
        const int size = 64;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        // 1. Arka Planı Çiz (Eğer opaklık > 0 ise)
        DrawSlotBackground(g, size, hexBgColor, bgOpacity);

        int tempInt = (int)Math.Round(temperature);
        string text = $"{tempInt}°";

        float scale = Math.Clamp(scalePercent / 100.0f, 0.50f, 1.85f);

        // Temel font boyutu ve ölçek çarpımı
        float baseFontSize = text.Length switch
        {
            <= 2 => 54f, // Örn: "8°"
            3 => 46f,    // Örn: "24°", "-5°"
            _ => 36f     // Örn: "-15°"
        };
        float fontSize = baseFontSize * scale;

        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
        var textSize = g.MeasureString(text, font);

        float x = (size - textSize.Width) / 2f + 1f;
        float y = (size - textSize.Height) / 2f + 2f;

        // Çok yönlü dış çerçeve (Halo effect)
        using var outlineBrush = new SolidBrush(Color.FromArgb(245, 0, 0, 0));
        float outlineSpread = highContrast ? Math.Max(3f, 4f * scale) : Math.Max(2f, 2.5f * scale);
        for (float ox = -outlineSpread; ox <= outlineSpread; ox += 1f)
        {
            for (float oy = -outlineSpread; oy <= outlineSpread; oy += 1f)
            {
                if (Math.Abs(ox) + Math.Abs(oy) <= outlineSpread + 1f && (ox != 0 || oy != 0))
                {
                    g.DrawString(text, font, outlineBrush, x + ox, y + oy);
                }
            }
        }

        // Ana Renk (Yüksek kontrast beyaz veya sıcaklık tonu)
        Color textColor = highContrast ? Color.FromArgb(255, 255, 255) : GetTemperatureColor(tempInt);
        using var textBrush = new SolidBrush(textColor);
        g.DrawString(text, font, textBrush, x, y);

        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);
        return (icon, hIcon);
    }

    /// <summary>
    /// Tek alan için kompakt ikon üretir.
    /// </summary>
    public static (Icon Icon, IntPtr Handle) GenerateCompactIcon(double temperature, int weatherCode, bool isDay, bool highContrast, int scalePercent = 100, string hexBgColor = "#000000", int bgOpacity = 0)
    {
        const int size = 64;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        // Arka plan
        DrawSlotBackground(g, size, hexBgColor, bgOpacity);

        float scale = Math.Clamp(scalePercent / 100.0f, 0.50f, 1.85f);

        // Sol ikon
        DrawSmallWeatherSymbol(g, weatherCode, isDay);

        // Sağ derece
        int tempInt = (int)Math.Round(temperature);
        string text = $"{tempInt}°";
        float baseFontSize = text.Length <= 2 ? 34f : (text.Length == 3 ? 30f : 24f);
        float fontSize = baseFontSize * scale;

        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
        var textSize = g.MeasureString(text, font);

        float x = 26f + (38f - textSize.Width) / 2f;
        if (x < 24f) x = 24f;
        float y = (64f - textSize.Height) / 2f + 1f;

        using var outlineBrush = new SolidBrush(Color.FromArgb(240, 0, 0, 0));
        float outlineSpread = highContrast ? 3f : 2f;
        for (float ox = -outlineSpread; ox <= outlineSpread; ox += 1f)
        {
            for (float oy = -outlineSpread; oy <= outlineSpread; oy += 1f)
            {
                if (ox != 0 || oy != 0) g.DrawString(text, font, outlineBrush, x + ox, y + oy);
            }
        }

        Color textColor = highContrast ? Color.FromArgb(255, 255, 255) : GetTemperatureColor(tempInt);
        using var textBrush = new SolidBrush(textColor);
        g.DrawString(text, font, textBrush, x, y);

        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);
        return (icon, hIcon);
    }

    /// <summary>
    /// Tepsi simgesi için yuvarlatılmış köşeli arka plan rozeti çizer.
    /// </summary>
    private static void DrawSlotBackground(Graphics g, int size, string hexColor, int opacityPercent)
    {
        if (opacityPercent <= 0) return;

        Color baseColor = ParseHexColor(hexColor);
        int alpha = (int)(255 * Math.Clamp(opacityPercent / 100.0f, 0f, 1f));
        if (alpha <= 0) return;

        Color fillColor = Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
        Color borderColor = Color.FromArgb(Math.Min(255, alpha + 40), Math.Min(255, baseColor.R + 30), Math.Min(255, baseColor.G + 30), Math.Min(255, baseColor.B + 30));

        using var path = CreateRoundedRectangle(3, 3, size - 6, size - 6, 16);
        using var fillBrush = new SolidBrush(fillColor);
        using var borderPen = new Pen(borderColor, 1.5f);

        g.FillPath(fillBrush, path);
        if (alpha > 40)
        {
            g.DrawPath(borderPen, path);
        }
    }

    private static GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(x, y, d, d, 180, 90);
        path.AddArc(x + width - d, y, d, d, 270, 90);
        path.AddArc(x + width - d, y + height - d, d, d, 0, 90);
        path.AddArc(x, y + height - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static Color ParseHexColor(string hex)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hex)) return Color.Black;
            hex = hex.Trim().TrimStart('#');
            if (hex.Length == 6)
            {
                int r = Convert.ToInt32(hex[..2], 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                return Color.FromArgb(r, g, b);
            }
        }
        catch { }
        return Color.Black;
    }

    public static string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Tüm 64x64 alana tam boy, zengin ve parlak hava durumu sembolü çizer.
    /// </summary>
    private static void DrawFullWeatherSymbol(Graphics g, int code, bool isDay, int size)
    {
        int cx = size / 2;
        int cy = size / 2;

        switch (code)
        {
            case 0: // Açık / Güneşli veya Gece Ay
                if (isDay)
                {
                    int r = 16;
                    using var sunBrush = new SolidBrush(Color.FromArgb(255, 215, 0));
                    using var rayPen = new Pen(Color.FromArgb(255, 195, 0), 3.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

                    g.FillEllipse(sunBrush, cx - r, cy - r, r * 2, r * 2);

                    for (int i = 0; i < 8; i++)
                    {
                        double angle = i * Math.PI / 4;
                        float x1 = (float)(cx + (r + 3) * Math.Cos(angle));
                        float y1 = (float)(cy + (r + 3) * Math.Sin(angle));
                        float x2 = (float)(cx + (r + 8) * Math.Cos(angle));
                        float y2 = (float)(cy + (r + 8) * Math.Sin(angle));
                        g.DrawLine(rayPen, x1, y1, x2, y2);
                    }
                }
                else
                {
                    int r = 20;
                    using var moonBrush = new SolidBrush(Color.FromArgb(255, 240, 160));
                    using var path = new GraphicsPath();
                    path.AddEllipse(cx - r, cy - r, r * 2, r * 2);

                    using var clipPath = new GraphicsPath();
                    clipPath.AddEllipse(cx - r + 9, cy - r - 2, r * 2, r * 2);

                    var region = new Region(path);
                    region.Exclude(clipPath);
                    g.FillRegion(moonBrush, region);
                }
                break;

            case 1:
            case 2: // Parçalı Bulutlu
                if (isDay)
                {
                    using var miniSun = new SolidBrush(Color.FromArgb(255, 210, 0));
                    g.FillEllipse(miniSun, cx - 18, cy - 20, 22, 22);
                }
                DrawCloud(g, cx + 4, cy + 4, 46, 26, Color.FromArgb(240, 245, 255));
                break;

            case 3: // Kapalı / Bulutlu
                DrawCloud(g, cx - 6, cy - 5, 42, 24, Color.FromArgb(160, 175, 195));
                DrawCloud(g, cx + 5, cy + 5, 48, 28, Color.FromArgb(225, 235, 245));
                break;

            case 45:
            case 48: // Sisli
                using (var fogPen = new Pen(Color.FromArgb(200, 220, 245), 4.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(fogPen, cx - 22, cy - 12, cx + 22, cy - 12);
                    g.DrawLine(fogPen, cx - 26, cy, cx + 26, cy);
                    g.DrawLine(fogPen, cx - 18, cy + 12, cx + 18, cy + 12);
                }
                break;

            case >= 51 and <= 67:
            case >= 80 and <= 82: // Yağmurlu / Sağanak
                DrawCloud(g, cx, cy - 8, 48, 26, Color.FromArgb(160, 180, 205));
                using (var rainPen = new Pen(Color.FromArgb(0, 190, 255), 3.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(rainPen, cx - 14, cy + 10, cx - 18, cy + 24);
                    g.DrawLine(rainPen, cx, cy + 10, cx - 4, cy + 24);
                    g.DrawLine(rainPen, cx + 14, cy + 10, cx + 10, cy + 24);
                }
                break;

            case >= 71 and <= 77:
            case 85:
            case 86: // Karlı
                DrawCloud(g, cx, cy - 8, 48, 26, Color.FromArgb(190, 210, 230));
                using (var snowBrush = new SolidBrush(Color.White))
                {
                    g.FillEllipse(snowBrush, cx - 14, cy + 12, 6, 6);
                    g.FillEllipse(snowBrush, cx, cy + 16, 7, 7);
                    g.FillEllipse(snowBrush, cx + 14, cy + 12, 6, 6);
                }
                break;

            case >= 95: // Fırtına / Şimşek
                DrawCloud(g, cx, cy - 10, 50, 26, Color.FromArgb(110, 125, 145));
                using (var boltBrush = new SolidBrush(Color.FromArgb(255, 220, 0)))
                {
                    Point[] bolt = {
                        new(cx + 2, cy + 2),
                        new(cx - 8, cy + 14),
                        new(cx - 1, cy + 14),
                        new(cx - 5, cy + 27),
                        new(cx + 8, cy + 11),
                        new(cx + 1, cy + 11)
                    };
                    g.FillPolygon(boltBrush, bolt);
                }
                break;

            default:
                DrawCloud(g, cx, cy, 48, 28, Color.FromArgb(230, 240, 250));
                break;
        }
    }

    private static void DrawSmallWeatherSymbol(Graphics g, int code, bool isDay)
    {
        int cx = 14;
        int cy = 32;
        int radius = 10;

        switch (code)
        {
            case 0:
                if (isDay)
                {
                    using var sunBrush = new SolidBrush(Color.FromArgb(255, 220, 0));
                    using var rayPen = new Pen(Color.FromArgb(255, 200, 0), 2.2f);
                    g.FillEllipse(sunBrush, cx - radius + 1, cy - radius + 1, (radius - 1) * 2, (radius - 1) * 2);
                    for (int i = 0; i < 8; i++)
                    {
                        double angle = i * Math.PI / 4;
                        float x1 = (float)(cx + (radius + 1) * Math.Cos(angle));
                        float y1 = (float)(cy + (radius + 1) * Math.Sin(angle));
                        float x2 = (float)(cx + (radius + 4) * Math.Cos(angle));
                        float y2 = (float)(cy + (radius + 4) * Math.Sin(angle));
                        g.DrawLine(rayPen, x1, y1, x2, y2);
                    }
                }
                else
                {
                    using var moonBrush = new SolidBrush(Color.FromArgb(255, 245, 170));
                    using var path = new GraphicsPath();
                    path.AddEllipse(cx - radius + 1, cy - radius + 1, radius * 2, radius * 2);
                    using var clipPath = new GraphicsPath();
                    clipPath.AddEllipse(cx - radius + 5, cy - radius, radius * 2, radius * 2);
                    var region = new Region(path);
                    region.Exclude(clipPath);
                    g.FillRegion(moonBrush, region);
                }
                break;
            case 1:
            case 2:
                if (isDay)
                {
                    using var miniSun = new SolidBrush(Color.FromArgb(255, 215, 0));
                    g.FillEllipse(miniSun, cx - 4, cy - 14, 12, 12);
                }
                DrawCloud(g, cx, cy + 2, 24, 14, Color.FromArgb(240, 245, 255));
                break;
            case 3:
                DrawCloud(g, cx - 2, cy - 2, 22, 13, Color.FromArgb(170, 180, 195));
                DrawCloud(g, cx + 2, cy + 3, 24, 14, Color.FromArgb(225, 235, 245));
                break;
            case >= 51 and <= 67:
            case >= 80 and <= 82:
                DrawCloud(g, cx, cy - 2, 25, 14, Color.FromArgb(170, 190, 215));
                using (var rainPen = new Pen(Color.FromArgb(0, 190, 255), 2.2f))
                {
                    g.DrawLine(rainPen, cx - 6, cy + 8, cx - 8, cy + 15);
                    g.DrawLine(rainPen, cx + 1, cy + 8, cx - 1, cy + 15);
                    g.DrawLine(rainPen, cx + 8, cy + 8, cx + 6, cy + 15);
                }
                break;
            default:
                DrawCloud(g, cx, cy, 24, 14, Color.FromArgb(230, 240, 250));
                break;
        }
    }

    private static void DrawCloud(Graphics g, int cx, int cy, int width, int height, Color color)
    {
        using var brush = new SolidBrush(color);
        int left = cx - width / 2;
        int top = cy - height / 2;

        g.FillEllipse(brush, left + 2, top + 3, height - 2, height - 2);
        g.FillEllipse(brush, left + width - height, top + 2, height - 1, height - 1);
        g.FillEllipse(brush, left + width / 4, top - 2, height + 3, height + 3);
        g.FillRectangle(brush, left + height / 2, top + 3, width - height, height - 2);
    }

    /// <summary>
    /// Renkli mod için sıcaklık derecesine göre canlı ve parlak renk döner.
    /// </summary>
    private static Color GetTemperatureColor(int temp)
    {
        return temp switch
        {
            <= 0 => Color.FromArgb(0, 230, 255),    // Don / Buz Cyan
            > 0 and <= 10 => Color.FromArgb(70, 190, 255), // Canlı Gök Mavisi
            > 10 and <= 18 => Color.FromArgb(90, 240, 160), // Ferah Bahar Yeşili
            > 18 and <= 25 => Color.FromArgb(255, 220, 40), // Parlak Güneş Sarısı
            > 25 and <= 33 => Color.FromArgb(255, 140, 20), // Sıcak Turuncu
            _ => Color.FromArgb(255, 75, 75)        // Sıcak Kırmızı
        };
    }
}
