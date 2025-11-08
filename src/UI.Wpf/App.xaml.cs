using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Core.DataAccess.Interfaces;
using Core.DataAccess.Services;
using Core.Nlp.Interfaces;
using Core.Nlp.Implementations;
using Core.Services.Interfaces;
using Core.Services.Services;
using UI.Wpf.ViewModels;
using UI.Wpf.Views;

namespace UI.Wpf;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        ConfigureLogging();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(Log.Logger);

                services.AddSingleton<IQueryExecutor, SqlQueryExecutor>();
                services.AddSingleton<ISchemaReader, SqlServerSchemaReader>();
                services.AddSingleton<INaturalLanguageQueryTranslator, RuleBasedNaturalLanguageTranslator>();

                services.AddSingleton<IConnectionService, ConnectionService>();
                services.AddSingleton<IQueryService, QueryService>();
                services.AddSingleton<ISchemaService, SchemaService>();

                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<ConnectionManagerViewModel>();
                services.AddTransient<SchemaBrowserViewModel>();
                services.AddTransient<QueryEditorViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    private void ConfigureLogging()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logPath = Path.Combine(appDataPath, "SqlStudioPro", "Logs", "app.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .WriteTo.Debug()
            .CreateLogger();

        Log.Information("Application starting...");
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainWindowViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Application shutting down...");
        await _host.StopAsync();
        _host.Dispose();
        Log.CloseAndFlush();

        base.OnExit(e);
    }
}
