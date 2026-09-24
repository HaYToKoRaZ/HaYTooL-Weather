using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HaYTooLWeather;

/// <summary>
/// Uygulama giriş noktası.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Tekil örnek (Single Instance) kontrolü
        var current = Process.GetCurrentProcess();
        var existingProcesses = Process.GetProcessesByName(current.ProcessName);
        if (existingProcesses.Length > 1)
        {
            MessageBox.Show(
                "HaYTooL Weather zaten sistem tepsisinde (saatin yanında) çalışıyor.",
                "HaYTooL Weather",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return;
        }

        // Global hata yakalama
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) => LogException(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex);
            }
        };

        RunApp();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RunApp()
    {
        // High-DPI ve modern görsel stilleri etkinleştir
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // HaYTooL Pulse Telemetri sayacını başlat (pc_weather)
        HaYTooLWeather.Services.PulseTelemetryService.Start();

        // Ana sistem tepsisi bağlamını çalıştır
        Application.Run(new TrayAppContext());
    }

    private static void LogException(Exception ex)
    {
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n\n");
        }
        catch { }
    }
}
