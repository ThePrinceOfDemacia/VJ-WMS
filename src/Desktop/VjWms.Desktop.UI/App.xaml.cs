using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VjWms.Desktop.Infrastructure.SQLite;

namespace VjWms.Desktop.UI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static string AppDataPath { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup Velopack
        Velopack.VelopackApp.Build().Run();

        // Setup AppData directory
        AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "vj-wms");
        Directory.CreateDirectory(AppDataPath);

        // Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(AppDataPath, "logs", "vjwms-desktop-.log"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("VJ-WMS Desktop v0.1.0-alpha starting");
        Log.Information("AppData path: {AppDataPath}", AppDataPath);

        // Setup DI
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Note: LocalDbContext will be configured per-user after login
        // For now, register a factory that can create contexts on demand
        services.AddTransient<Func<string, LocalDbContext>>(sp => (dbPath) =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<LocalDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            var context = new LocalDbContext(optionsBuilder.Options);
            context.Database.EnsureCreated();
            return context;
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("VJ-WMS Desktop shutting down");
        Log.CloseAndFlush();
        base.OnExit(e);
        Environment.Exit(0);
    }

    public static void SetLanguage(string langCode)
    {
        var dict = new ResourceDictionary();
        dict.Source = new Uri($"Resources/Strings.{langCode}.xaml", UriKind.Relative);

        // Find and replace the string resource dictionary
        var oldDict = Current.Resources.MergedDictionaries.FirstOrDefault(d => 
            d.Source != null && d.Source.OriginalString.StartsWith("Resources/Strings."));
        
        if (oldDict != null)
        {
            Current.Resources.MergedDictionaries.Remove(oldDict);
        }
        
        Current.Resources.MergedDictionaries.Add(dict);
    }
}
