namespace HaYTooLWeather;

/// <summary>
/// Projenin tek ve merkezi sürüm yöneticisi.
/// Tüm sürüm bilgileri buradan yönetilir, kod tabanında başka hiçbir yerde elle sürüm yazılmaz.
/// </summary>
public static class AppVersion
{
    /// <summary>
    /// Uygulama anlık sürüm numarası.
    /// </summary>
    public const string Version = "v4.0";

    /// <summary>
    /// Sadece sayısal/temiz sürüm (derleme ve paketleme için, örn: "4.0").
    /// </summary>
    public static string CleanVersion => Version.TrimStart('v', 'V');

    /// <summary>
    /// Tam sürüm açıklaması (örn: "v4.0 - Windows Native Desktop Application").
    /// </summary>
    public static string DisplayVersion => $"{Version} - Windows Native Desktop Application";

    /// <summary>
    /// Çalışma anındaki aktif sürümü getirir (önce derleme metaverisi, sonra bu sabit).
    /// </summary>
    public static string GetCurrentVersion()
    {
        try
        {
            var assembly = typeof(AppVersion).Assembly;
            var infoVersion = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>(assembly)?.InformationalVersion;
            if (!string.IsNullOrWhiteSpace(infoVersion))
            {
                var clean = infoVersion.Split('+')[0].Trim();
                if (!clean.StartsWith("v", StringComparison.OrdinalIgnoreCase)) clean = "v" + clean;
                return clean;
            }

            var asmVersion = assembly.GetName().Version;
            if (asmVersion != null)
            {
                return asmVersion.Build > 0 
                    ? $"v{asmVersion.Major}.{asmVersion.Minor}.{asmVersion.Build}"
                    : $"v{asmVersion.Major}.{asmVersion.Minor}";
            }
        }
        catch { }

        return Version;
    }
}
