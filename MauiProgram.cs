using MauiApp1.Data;
using MauiApp1.Repository;
using MauiApp1.Views;
using Microsoft.Extensions.Logging;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Registro del servicio de base de datos SQLite (inyección de dependencias)
            builder.Services.AddSingleton<PersonaDatabase>();
            builder.Services.AddSingleton<PersonRepository>();

            // Páginas que reciben dependencias por constructor
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<vPersona>();

            // El Shell también se construye por DI para recibir la página
            builder.Services.AddTransient<AppShell>();

            return builder.Build();
        }
    }
}
