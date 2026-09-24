using System.Diagnostics;
using System.Drawing;
using HaYTooLWeather.Services;

namespace HaYTooLWeather.Forms;

/// <summary>
/// HaYTo imzası ve proje bağlantılarını içeren Hakkında penceresi.
/// </summary>
public class AboutForm : Form
{
    public AboutForm()
    {
        Text = LocalizationService.Get("about_title");
        Size = new Size(440, 360);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(24, 27, 34);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        var pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24)
        };

        var lblTitle = new Label
        {
            Text = "🌤️ HaYTooL Weather",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(24, 20)
        };

        var lblVersion = new Label
        {
            Text = $"Sürüm {AppVersion.GetCurrentVersion()} (Windows Native Edition)",
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            ForeColor = Color.FromArgb(0, 170, 255),
            AutoSize = true,
            Location = new Point(26, 52)
        };

        var lblDesc = new Label
        {
            Text = LocalizationService.Get("about_desc"),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(190, 200, 215),
            AutoSize = false,
            Size = new Size(380, 42),
            Location = new Point(26, 80)
        };

        var lblDev = new Label
        {
            Text = "👨‍💻 " + LocalizationService.Get("about_dev"),
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 215, 0),
            AutoSize = true,
            Location = new Point(26, 126)
        };

        // Bağlantılar
        var lblContactHeader = new Label
        {
            Text = LocalizationService.Get("about_contact"),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(26, 160)
        };

        var lnkPortal = new LinkLabel
        {
            Text = "🌐 HaYTooL PoRTaL: https://haytokoraz.github.io/",
            LinkColor = Color.FromArgb(0, 220, 180),
            ActiveLinkColor = Color.FromArgb(100, 255, 220),
            VisitedLinkColor = Color.FromArgb(0, 220, 180),
            AutoSize = true,
            Location = new Point(26, 185),
            Cursor = Cursors.Hand
        };
        lnkPortal.LinkClicked += (s, e) => OpenUrl("https://haytokoraz.github.io/");

        var lnkEmail = new LinkLabel
        {
            Text = "📧 E-posta: korazhayto@gmail.com",
            LinkColor = Color.FromArgb(0, 180, 255),
            ActiveLinkColor = Color.FromArgb(100, 210, 255),
            VisitedLinkColor = Color.FromArgb(0, 180, 255),
            AutoSize = true,
            Location = new Point(26, 208),
            Cursor = Cursors.Hand
        };
        lnkEmail.LinkClicked += (s, e) => OpenUrl("mailto:korazhayto@gmail.com");

        var lnkX = new LinkLabel
        {
            Text = "🐦 X (Twitter): https://x.com/HaYTo",
            LinkColor = Color.FromArgb(0, 180, 255),
            ActiveLinkColor = Color.FromArgb(100, 210, 255),
            VisitedLinkColor = Color.FromArgb(0, 180, 255),
            AutoSize = true,
            Location = new Point(26, 230),
            Cursor = Cursors.Hand
        };
        lnkX.LinkClicked += (s, e) => OpenUrl("https://x.com/HaYTo");

        var lnkGithub = new LinkLabel
        {
            Text = "🐙 GitHub: https://github.com/HaYToKoRaZ/HaYTooL-Weather",
            LinkColor = Color.FromArgb(0, 180, 255),
            ActiveLinkColor = Color.FromArgb(100, 210, 255),
            VisitedLinkColor = Color.FromArgb(0, 180, 255),
            AutoSize = true,
            Location = new Point(26, 252),
            Cursor = Cursors.Hand
        };
        lnkGithub.LinkClicked += (s, e) => OpenUrl("https://github.com/HaYToKoRaZ/HaYTooL-Weather");

        var lblPrivacy = new Label
        {
            Text = LocalizationService.Get("about_privacy"),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
            ForeColor = Color.FromArgb(120, 200, 140),
            AutoSize = true,
            Location = new Point(26, 280)
        };

        var btnClose = new Button
        {
            Text = "Tamam",
            Location = new Point(310, 285),
            Size = new Size(95, 32),
            BackColor = Color.FromArgb(0, 122, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (s, e) => Close();

        pnlContent.Controls.AddRange(new Control[] {
            lblTitle, lblVersion, lblDesc,
            lblDev, lblContactHeader,
            lnkPortal, lnkEmail, lnkX, lnkGithub,
            lblPrivacy, btnClose
        });

        Controls.Add(pnlContent);
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
        catch
        {
            // Tarayıcı açılamazsa hata fırlatma
        }
    }
}
