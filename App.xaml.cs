using MauiApp1.Data;
using MauiApp1.Repository;
using MauiApp1.Util;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace MauiApp1
{
    public partial class App : Application
    {
        public static PersonaDatabase Database { get; private set; } = null!;

        public static PersonRepository Repository { get; private set; } = null!;

        private readonly IServiceProvider _services;

        public App(PersonaDatabase database, PersonRepository repository, IServiceProvider services)
        {
            InitializeComponent();

            Database = database;
            Repository = repository;
            _services = services;

            InicializarBaseDeDatos();
        }

        private static void InicializarBaseDeDatos()
        {
            try
            {
                using var connection = FileAccessHelper.CrearBaseDeDatos();

                Repository.Message = $"Base de datos lista: {FileAccessHelper.DatabaseFileName}";

                Debug.WriteLine($"[SQLite] Ruta: {FileAccessHelper.GetDatabasePath()}");
                Debug.WriteLine($"[SQLite] Existe: {FileAccessHelper.DatabaseExists()}");
                Debug.WriteLine($"[SQLite] Tamaño: {FileAccessHelper.GetFileSize(FileAccessHelper.DatabaseFileName)} bytes");
            }
            catch (Exception ex)
            {
                Repository.Message = $"Error al inicializar la base de datos: {ex.Message}";
                Debug.WriteLine($"[SQLite] {Repository.Message}");
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = _services.GetRequiredService<AppShell>();

            return new Window(shell);
        }
    }
}
