using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VjWms.Desktop.Infrastructure.SQLite;

namespace VjWms.Desktop.UI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static string AppDataPath { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch anything that would otherwise silently crash the process, so the user
        // gets an error dialog instead of the app freezing and disappearing.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // Setup Velopack (must be first)
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

        // Check for updates in background (non-blocking)
        _ = Task.Run(() => CheckForUpdatesAsync());
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var mgr = new Velopack.UpdateManager(
                new Velopack.Sources.GithubSource(
                    "https://github.com/ThePrinceOfDemacia/VJ-WMS", null, false));

            // Check if there's a newer version
            var updateInfo = await mgr.CheckForUpdatesAsync();
            if (updateInfo == null)
            {
                Log.Information("No updates available.");
                return;
            }

            Log.Information("Update available: {Version}", updateInfo.TargetFullRelease.Version);

            // Download the update
            await mgr.DownloadUpdatesAsync(updateInfo);

            // Ask user if they want to restart
            var result = MessageBox.Show(
                $"Phiên bản mới {updateInfo.TargetFullRelease.Version} đã sẵn sàng!\nBạn có muốn cập nhật và khởi động lại không?\n\nNew version {updateInfo.TargetFullRelease.Version} is ready!\nWould you like to update and restart?",
                "VJ-WMS Update / Cập nhật",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                mgr.ApplyUpdatesAndRestart(updateInfo);
            }
        }
        catch (Exception ex)
        {
            // Don't block the app if update check fails (e.g. no internet)
            Log.Warning(ex, "Update check failed (this is normal in dev mode or offline)");
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Note: LocalDbContext will be configured per-user after login
        // For now, register a factory that can create contexts on demand
        services.AddSingleton<Func<string, LocalDbContext>>(sp => (dbPath) =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<LocalDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            var context = new LocalDbContext(optionsBuilder.Options);
            context.Database.EnsureCreated();
            EnsureEditHistoriesTable(context);
            return context;
        });

        // Services
        services.AddSingleton<UI.Services.INavigationService, UI.Services.NavigationService>();
        services.AddSingleton<UI.Services.Scanner.IScannerService, UI.Services.Scanner.ScannerService>();
        services.AddTransient<UI.Services.AttachmentService>();

        services.AddSingleton<Func<Type, UI.ViewModels.BaseViewModel>>(sp => type => (UI.ViewModels.BaseViewModel)sp.GetRequiredService(type));

        // Register ViewModels
        services.AddTransient<UI.ViewModels.LoginViewModel>();
        services.AddTransient<UI.ViewModels.ShellViewModel>();
        services.AddTransient<UI.ViewModels.DashboardViewModel>();
        services.AddTransient<UI.ViewModels.InventoryViewModel>();
        
        services.AddTransient<UI.ViewModels.MasterData.WarehouseListViewModel>();
        services.AddTransient<UI.ViewModels.MasterData.ProductListViewModel>();

        services.AddTransient<UI.ViewModels.Receipts.ReceiptListViewModel>();
        services.AddTransient<UI.ViewModels.Receipts.ReceiptCreateViewModel>();

        services.AddTransient<UI.ViewModels.Issues.IssueListViewModel>();
        services.AddTransient<UI.ViewModels.Issues.IssueCreateViewModel>();

        services.AddTransient<UI.ViewModels.Transfers.TransferListViewModel>();
        services.AddTransient<UI.ViewModels.Transfers.TransferCreateViewModel>();

        services.AddTransient<UI.ViewModels.TransactionDetailViewModel>();

        // Register current DB context scoped to the active user (hacky for WPF but works for single user session)
        services.AddTransient<LocalDbContext>(sp => 
        {
            // The shell window sets this static property after login
            var dbPath = Path.Combine(AppDataPath, "users", CurrentUsername ?? "admin", "local.db");
            return sp.GetRequiredService<Func<string, LocalDbContext>>()(dbPath);
        });
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception on UI thread");

        MessageBox.Show(
            $"Đã xảy ra lỗi không mong muốn. Thao tác vừa rồi chưa được lưu.\n\n" +
            $"An unexpected error occurred. The last action was not saved.\n\n{e.Exception.Message}",
            "Lỗi / Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // Prevents WPF from tearing down the process for this exception.
        e.Handled = true;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Exceptions on non-UI threads can't be suppressed and will still terminate the
        // process (IsTerminating is almost always true here) - at least log it before it dies.
        Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception on background thread (IsTerminating: {IsTerminating})", e.IsTerminating);
        Log.CloseAndFlush();
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        // Mark observed so the finalizer thread doesn't rethrow and crash the process.
        e.SetObserved();
    }

    // EnsureCreated() only builds the schema for a brand-new database file - it does nothing
    // for a file that already exists, even when the EF model has grown new tables since. The
    // EditHistories table (Phase 2.5) was added after existing users' local.db files, so it
    // never got created for them, causing "no such table: EditHistories" on every edit-save.
    // Patch it in directly if missing; a no-op for databases where it already exists.
    private static void EnsureEditHistoriesTable(LocalDbContext context)
    {
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""EditHistories"" (
                ""Id"" TEXT NOT NULL CONSTRAINT ""PK_EditHistories"" PRIMARY KEY,
                ""DocumentId"" TEXT NOT NULL,
                ""DocumentType"" TEXT NOT NULL,
                ""Action"" TEXT NOT NULL,
                ""OldValues"" TEXT NULL,
                ""NewValues"" TEXT NULL,
                ""ChangedBy"" TEXT NOT NULL,
                ""ChangedAt"" TEXT NOT NULL,
                ""Notes"" TEXT NULL
            )");
    }

    public static string? CurrentUsername { get; set; }

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
