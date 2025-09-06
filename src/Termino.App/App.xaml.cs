using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.IO;
using Termino.Data;
using Termino.Data.Repositories;
using Termino.Localization.Core;
using Termino.App.Services;

namespace Termino.App;
public partial class App : Application
{
    public static IHost? AppHost { get; private set; }
    public static SettingsService? Settings { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Localizer.Instance.Load("ru");

        var dbPath = Path.Combine(AppContext.BaseDirectory, "termino.db");
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        Settings = new SettingsService(settingsPath);

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices(s => {
                s.AddDbContext<TerminoDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));
                s.AddScoped<ITermRepository, TermRepository>();
            })
            .Build();

        using(var scope = AppHost.Services.CreateScope()){
            var ctx = scope.ServiceProvider.GetRequiredService<TerminoDbContext>();
            ctx.Database.EnsureCreated();
        }

        AppHost.Start();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try{ Settings?.Save(); }catch{}
        if (AppHost != null) await AppHost.StopAsync();
        base.OnExit(e);
    }
}