using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Forms;

namespace HaYTooLWeather.Services;

/// <summary>
/// GitHub Releases API üzerinden güncellemeleri kontrol eden, doğrudan indiren ve uygulamayı yeniden başlatan servis.
/// </summary>
public static class UpdateService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    public const string RepoOwner = "HaYToKoRaZ";
    public const string RepoName = "HaYTooL-Weather";
    public const string ReleasesPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";

    static UpdateService()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HaYTooL-Weather-Updater/1.0 (Windows NT; by HaYTo)");
        HttpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
    }

    /// <summary>
    /// Merkezi AppVersion sınıfı üzerinden anlık sürümü döner.
    /// </summary>
    public static string GetCurrentVersion() => AppVersion.GetCurrentVersion();

    /// <summary>
    /// GitHub API'sinden en son yayınlanan sürümü sorgular, doğrudan indirme bağlantısını bulur ve mevcut sürümle karşılaştırır.
    /// </summary>
    public static async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var currentVer = GetCurrentVersion();

        try
        {
            var apiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            var jsonStr = await HttpClient.GetStringAsync(apiUrl, cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(jsonStr);

            var root = doc.RootElement;
            var latestTag = root.TryGetProperty("tag_name", out var tagProp) ? (tagProp.GetString() ?? "") : "";
            var releaseUrl = root.TryGetProperty("html_url", out var urlProp) ? (urlProp.GetString() ?? ReleasesPageUrl) : ReleasesPageUrl;
            var releaseTitle = root.TryGetProperty("name", out var nameProp) ? (nameProp.GetString() ?? "") : "";
            var releaseNotes = root.TryGetProperty("body", out var bodyProp) ? (bodyProp.GetString() ?? "") : "";

            if (string.IsNullOrWhiteSpace(latestTag))
            {
                return new UpdateCheckResult(false, currentVer, currentVer, ReleasesPageUrl, "", "", null, "Geçersiz sürüm bilgisi.");
            }

            // Doğrudan indirme bağlantısını (Direct Download URL) assets listesinden ayıkla
            string? directDownloadUrl = null;
            if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsProp.EnumerateArray())
                {
                    var name = asset.TryGetProperty("name", out var nProp) ? (nProp.GetString() ?? "") : "";
                    var downloadUrl = asset.TryGetProperty("browser_download_url", out var uProp) ? (uProp.GetString() ?? "") : "";

                    if (!string.IsNullOrEmpty(downloadUrl))
                    {
                        // Öncelikle tek parça HaYTooLWeather.exe tercih et
                        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            directDownloadUrl = downloadUrl;
                            break;
                        }
                        // Yoksa Portable .zip paketini al
                        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            directDownloadUrl = downloadUrl;
                        }
                    }
                }
            }

            bool isNewer = IsNewerVersion(latestTag, currentVer);

            return new UpdateCheckResult(isNewer, currentVer, latestTag, releaseUrl, releaseTitle, releaseNotes, directDownloadUrl);
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult(false, currentVer, currentVer, ReleasesPageUrl, "", "", null, ex.Message);
        }
    }

    /// <summary>
    /// Yeni sürümü doğrudan indirir, dosyayı hazırlar ve uygulamayı otomatik güncelleyerek yeniden başlatır.
    /// </summary>
    public static async Task DownloadAndInstallUpdateAsync(string downloadUrl, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        var currentExe = Environment.ProcessPath ?? Application.ExecutablePath;
        var exeDir = Path.GetDirectoryName(currentExe) ?? AppDomain.CurrentDomain.BaseDirectory;
        var tempFile = Path.Combine(exeDir, $"update_{Guid.NewGuid():N}.tmp");
        var targetNewExe = Path.Combine(exeDir, "HaYTooLWeather.new.exe");

        // 1. Dosyayı indir ve ilerlemeyi bildir
        using (var response = await HttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
        {
            response.EnsureSuccessStatusCode();
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                totalRead += bytesRead;
                if (totalBytes > 0 && progress != null)
                {
                    int percent = (int)((totalRead * 100) / totalBytes);
                    progress.Report(percent);
                }
            }
        }

        // 2. Eğer ZIP paketi indirilmişse içindeki EXE'yi ayıkla
        bool isZip = downloadUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
        if (isZip)
        {
            using (var archive = ZipFile.OpenRead(tempFile))
            {
                var exeEntry = archive.Entries.FirstOrDefault(e => e.Name.Equals("HaYTooLWeather.exe", StringComparison.OrdinalIgnoreCase));
                if (exeEntry == null)
                {
                    throw new InvalidOperationException("İndirilen zip paketi içinde 'HaYTooLWeather.exe' bulunamadı.");
                }

                if (File.Exists(targetNewExe)) File.Delete(targetNewExe);
                exeEntry.ExtractToFile(targetNewExe, overwrite: true);
            }
            try { File.Delete(tempFile); } catch { }
        }
        else
        {
            if (File.Exists(targetNewExe)) File.Delete(targetNewExe);
            File.Move(tempFile, targetNewExe, true);
        }

        // 3. Mevcut süreci kapat, yeni EXE'yi yerine taşı ve uygulamayı başlat
        var currentExeName = Path.GetFileName(currentExe);
        var cmd = $"/c timeout /t 1 /nobreak > NUL & move /y \"{targetNewExe}\" \"{currentExe}\" & start \"\" \"{currentExe}\"";

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = cmd,
            CreateNoWindow = true,
            UseShellExecute = false
        });

        // 4. Mevcut süreci sonlandır
        Application.Exit();
        Environment.Exit(0);
    }

    /// <summary>
    /// İki sürüm dizesini (örn: v4.6 ile v4.5) karşılaştırır.
    /// </summary>
    public static bool IsNewerVersion(string latestVer, string currentVer)
    {
        try
        {
            var cleanLatest = latestVer.Trim().TrimStart('v', 'V');
            var cleanCurrent = currentVer.Trim().TrimStart('v', 'V');

            if (Version.TryParse(cleanLatest, out var vLatest) && Version.TryParse(cleanCurrent, out var vCurrent))
            {
                return vLatest > vCurrent;
            }

            return string.Compare(cleanLatest, cleanCurrent, StringComparison.OrdinalIgnoreCase) > 0;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Güncelleme kontrol sonucunu taşıyan model.
/// </summary>
public record UpdateCheckResult(
    bool IsUpdateAvailable,
    string CurrentVersion,
    string LatestVersion,
    string ReleaseUrl,
    string ReleaseTitle,
    string ReleaseNotes,
    string? DirectDownloadUrl = null,
    string? ErrorMessage = null
);
