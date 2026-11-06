using System.Text;
using System.Text.Json;

namespace HaYTooLWeather.Services;

/// <summary>
/// HaYTooL Pulse: Merkezi canlı kullanıcı sayacı istemcisi (pc_weather).
/// Tamamen anonim, sıfır kişisel veri ve oturum bazlı kalp atışı/nabız (heartbeat) gönderir.
/// </summary>
public static class PulseTelemetryService
{
    private const string TelemetryUrl = "https://hayto-telemetry.korazhayto.workers.dev/api/ping";
    private const string AppId = "pc_weather";
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

    private static System.Threading.Timer? _pulseTimer;
    private static readonly string SessionId = "pc_" + Guid.NewGuid().ToString("N")[..10];
    private static bool _isFirst = true;

    /// <summary>
    /// Telemetri servisini arka planda baslatir (2 dakikada bir heartbeat).
    /// </summary>
    public static void Start()
    {
        // Ilk nabzi arka plan gorevinde hemen at
        _ = Task.Run(SendPulseAsync);

        // 2 dakikada bir tetikle (120.000 ms)
        _pulseTimer = new System.Threading.Timer(
            async _ => await SendPulseAsync(),
            null,
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(2)
        );
    }

    private static async Task SendPulseAsync()
    {
        try
        {
            var payload = new
            {
                app = AppId,
                session_id = SessionId,
                is_new_session = _isFirst
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync(TelemetryUrl, content).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                _isFirst = false;
            }
        }
        catch
        {
            // Sessizce yut - baglanti hatasinda uygulamayi asla etkilemez
        }
    }
}
