using System.Net.Http.Json;
using System.Text.Json;

namespace HaYTooLWeather.Services;

/// <summary>
/// GitHub Releases API üzerinden güncellemeleri kontrol eden ve yeni sürüm bildiren servis.
/// </summary>
public static class UpdateService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
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
    /// GitHub API'sinden en son yayınlanan sürümü sorgular ve mevcut sürümle karşılaştırır.
    /// </summary>
    public static async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var currentVer = GetCurrentVersion();

        try
        {
            var apiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            var jsonStr = await HttpClient.GetStringAsync(apiUrl, cancellationToken);
            using var doc = JsonDocument.Parse(jsonStr);

            var root = doc.RootElement;
            var latestTag = root.TryGetProperty("tag_name", out var tagProp) ? (tagProp.GetString() ?? "") : "";
            var releaseUrl = root.TryGetProperty("html_url", out var urlProp) ? (urlProp.GetString() ?? ReleasesPageUrl) : ReleasesPageUrl;
            var releaseTitle = root.TryGetProperty("name", out var nameProp) ? (nameProp.GetString() ?? "") : "";
            var releaseNotes = root.TryGetProperty("body", out var bodyProp) ? (bodyProp.GetString() ?? "") : "";

            if (string.IsNullOrWhiteSpace(latestTag))
            {
                return new UpdateCheckResult(false, currentVer, currentVer, ReleasesPageUrl, "", "", "Geçersiz sürüm bilgisi.");
            }

            bool isNewer = IsNewerVersion(latestTag, currentVer);

            return new UpdateCheckResult(isNewer, currentVer, latestTag, releaseUrl, releaseTitle, releaseNotes);
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult(false, currentVer, currentVer, ReleasesPageUrl, "", "", ex.Message);
        }
    }

    /// <summary>
    /// İki sürüm dizesini (örn: v1.0.1 ile v1.0.0) karşılaştırır.
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

            // Basit string karşılaştırma yedeği
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
    string? ErrorMessage = null
);
