using System.Drawing;

namespace HaYTooLWeather.Models;

/// <summary>
/// Türkiye şehirlerinden ilham alan Theme Factory renk paletleri.
/// </summary>
public class CityTheme
{
    public string Id { get; set; } = "";
    public string NameTr { get; set; } = "";
    public string DescriptionTr { get; set; } = "";
    public Color Primary { get; set; }
    public Color Accent { get; set; }
    public Color Background { get; set; }
    public Color Surface { get; set; }
    public Color CardBg { get; set; }
    public Color Text { get; set; }
    public Color TextMuted { get; set; }
}

public static class ThemeFactory
{
    public static readonly List<CityTheme> CityThemes = new()
    {
        // 1. İstanbul (Boğaz & Derin Deniz Mavisi - Ocean Depths esintisi)
        new CityTheme
        {
            Id = "istanbul",
            NameTr = "İstanbul (Boğaz Mavisi)",
            DescriptionTr = "Boğaziçi derinlikleri, tarihi deniz ışıltısı ve dingin lacivert tonlar.",
            Primary = Color.FromArgb(0, 180, 240),
            Accent = Color.FromArgb(58, 123, 213),
            Background = Color.FromArgb(10, 15, 26),
            Surface = Color.FromArgb(18, 25, 42),
            CardBg = Color.FromArgb(24, 34, 56),
            Text = Color.FromArgb(245, 248, 255),
            TextMuted = Color.FromArgb(148, 163, 184)
        },
        // 2. Ankara (Bozkır Altını & Gece Grisi - Golden Hour)
        new CityTheme
        {
            Id = "ankara",
            NameTr = "Ankara (Bozkır Altını)",
            DescriptionTr = "Başkent asaleti, bozkır gün batımı ışıltısı ve sofistike kömür grisi.",
            Primary = Color.FromArgb(245, 158, 11),
            Accent = Color.FromArgb(217, 119, 6),
            Background = Color.FromArgb(18, 19, 23),
            Surface = Color.FromArgb(28, 30, 36),
            CardBg = Color.FromArgb(38, 41, 50),
            Text = Color.FromArgb(254, 250, 240),
            TextMuted = Color.FromArgb(163, 163, 163)
        },
        // 3. İzmir (Ege Günbatımı & Kordon Turuncusu - Sunset Boulevard)
        new CityTheme
        {
            Id = "izmir",
            NameTr = "İzmir (Kordon Günbatımı)",
            DescriptionTr = "Kordon boyundaki sıcacık eflatun-turuncu gün batımı ve canlı Akdeniz enerjisi.",
            Primary = Color.FromArgb(249, 115, 22),
            Accent = Color.FromArgb(236, 72, 153),
            Background = Color.FromArgb(22, 14, 20),
            Surface = Color.FromArgb(35, 20, 30),
            CardBg = Color.FromArgb(48, 26, 40),
            Text = Color.FromArgb(255, 245, 247),
            TextMuted = Color.FromArgb(190, 155, 175)
        },
        // 4. Antalya (Akdeniz Turkuazı & Güneş - Tech Innovation/Vibrant)
        new CityTheme
        {
            Id = "antalya",
            NameTr = "Antalya (Akdeniz Turkuazı)",
            DescriptionTr = "Falezlerin kristal turkuaz suları ve Torosların ferah Akdeniz esintisi.",
            Primary = Color.FromArgb(20, 184, 166),
            Accent = Color.FromArgb(6, 182, 212),
            Background = Color.FromArgb(9, 20, 23),
            Surface = Color.FromArgb(15, 33, 38),
            CardBg = Color.FromArgb(21, 46, 54),
            Text = Color.FromArgb(240, 253, 250),
            TextMuted = Color.FromArgb(140, 175, 185)
        },
        // 5. Bursa (Yeşil Bursa & Uludağ Göknarı - Forest Canopy)
        new CityTheme
        {
            Id = "bursa",
            NameTr = "Bursa (Yeşil Çam & Uludağ)",
            DescriptionTr = "Ulu çınarlar, yemyeşil doğa ve çam ormanlarının dingin zümrüt ahengi.",
            Primary = Color.FromArgb(34, 197, 94),
            Accent = Color.FromArgb(16, 185, 129),
            Background = Color.FromArgb(10, 20, 15),
            Surface = Color.FromArgb(18, 34, 26),
            CardBg = Color.FromArgb(25, 48, 37),
            Text = Color.FromArgb(240, 253, 244),
            TextMuted = Color.FromArgb(145, 180, 160)
        },
        // 6. Trabzon (Karadeniz Ormanı & Yayla Yeşili - Botanical Garden)
        new CityTheme
        {
            Id = "trabzon",
            NameTr = "Trabzon (Karadeniz Yaylası)",
            DescriptionTr = "Fırtına Deresi, yayla sisleri ve Karadeniz'in coşkun zümrüt-çam dokusu.",
            Primary = Color.FromArgb(13, 148, 136),
            Accent = Color.FromArgb(74, 222, 128),
            Background = Color.FromArgb(10, 22, 22),
            Surface = Color.FromArgb(16, 36, 35),
            CardBg = Color.FromArgb(24, 51, 50),
            Text = Color.FromArgb(236, 253, 245),
            TextMuted = Color.FromArgb(145, 175, 168)
        },
        // 7. Nevşehir / Kapadokya (Peri Bacaları & Gün Doğumu - Desert Rose)
        new CityTheme
        {
            Id = "nevsehir",
            NameTr = "Kapadokya (Peri Bacaları)",
            DescriptionTr = "Sıcak hava balonları, tüf kayaların pembe-kiremit tonları ve masalsı gün doğumu.",
            Primary = Color.FromArgb(244, 63, 94),
            Accent = Color.FromArgb(251, 146, 60),
            Background = Color.FromArgb(24, 15, 18),
            Surface = Color.FromArgb(38, 22, 27),
            CardBg = Color.FromArgb(54, 30, 38),
            Text = Color.FromArgb(255, 241, 242),
            TextMuted = Color.FromArgb(195, 160, 170)
        },
        // 8. Erzurum (Palandöken Kristal Karı - Arctic Frost)
        new CityTheme
        {
            Id = "erzurum",
            NameTr = "Erzurum (Palandöken Kristali)",
            DescriptionTr = "Palandöken doruklarında parıldayan kristal beyaz-buz mavisi hava durumu hissi.",
            Primary = Color.FromArgb(56, 189, 248),
            Accent = Color.FromArgb(147, 197, 253),
            Background = Color.FromArgb(12, 18, 28),
            Surface = Color.FromArgb(20, 29, 44),
            CardBg = Color.FromArgb(30, 42, 64),
            Text = Color.FromArgb(240, 249, 255),
            TextMuted = Color.FromArgb(160, 185, 210)
        },
        // 9. Gaziantep (Bakır & Fıstık Yeşili - Rich Artisanal)
        new CityTheme
        {
            Id = "gaziantep",
            NameTr = "Gaziantep (Antep Fıstığı & Bakır)",
            DescriptionTr = "Geleneksel dövme bakır parıltısı ile taze antep fıstığının benzersiz tonları.",
            Primary = Color.FromArgb(163, 230, 53),
            Accent = Color.FromArgb(217, 119, 6),
            Background = Color.FromArgb(19, 18, 12),
            Surface = Color.FromArgb(32, 30, 18),
            CardBg = Color.FromArgb(46, 42, 25),
            Text = Color.FromArgb(254, 252, 232),
            TextMuted = Color.FromArgb(180, 175, 145)
        },
        // 10. Muğla (Gökova Gece Mavisi & Samanyolu - Midnight Galaxy)
        new CityTheme
        {
            Id = "mugla",
            NameTr = "Muğla (Gökova Samanyolu)",
            DescriptionTr = "Gökova koylarında yıldızlarla dolu berrak gece gökyüzü ve derin ametist moru.",
            Primary = Color.FromArgb(168, 85, 247),
            Accent = Color.FromArgb(129, 140, 248),
            Background = Color.FromArgb(15, 12, 26),
            Surface = Color.FromArgb(25, 20, 44),
            CardBg = Color.FromArgb(36, 28, 62),
            Text = Color.FromArgb(250, 245, 255),
            TextMuted = Color.FromArgb(175, 160, 200)
        }
    };

    public static CityTheme GetTheme(string? themeId)
    {
        if (string.IsNullOrWhiteSpace(themeId))
            return CityThemes[0];

        var found = CityThemes.FirstOrDefault(t => t.Id.Equals(themeId, StringComparison.OrdinalIgnoreCase));
        return found ?? CityThemes[0];
    }
}
