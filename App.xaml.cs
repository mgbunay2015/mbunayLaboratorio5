using MauiApp1.Data;
using MauiApp1.Repository;
using MauiApp1.Util;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace MauiApp1
{
    public partial class App : Application
    {
        /// <summary>
        /// Servicio de base de datos disponible para todas las páginas.
        /// </summary>
        public static PersonaDatabase Database { get; private set; } = null!;

        /// <summary>
        /// Repositorio de personas disponible para todas las páginas.
        /// </summary>
        public static PersonRepository Repository { get; private set; } = null!;

        private readonly IServiceProvider _services;

        public App(PersonaDatabase database, PersonRepository repository, IServiceProvider services)
        {
            InitializeComponent();

            Database = database;
            Repository = repository;
            _services = services;

            // Se crea el archivo uisrael.db y la tabla Persona de forma
            // SÍNCRONA, antes de que se muestre cualquier pantalla.
            // Así vPersona nunca encuentra la tabla sin crear ni compite
            // con una tarea en segundo plano.
            InicializarBaseDeDatos();
        }

        /// <summary>
        /// Crea el archivo de base de datos y la tabla Persona usando
        /// <see cref="FileAccessHelper"/>. La conexión se cierra al salir
        /// para que el repositorio no encuentre el archivo bloqueado.
        /// </summary>
        private static void InicializarBaseDeDatos()
        {
            try
            {
                using (var connection = FileAccessHelper.CrearBaseDeDatos())
                {
                    // La conexión se abre, se crea la tabla y se cierra aquí mismo.
                }

                Repository.Message = $"Base de datos lista: {FileAccessHelper.DatabaseFileName}";

                Debug.WriteLine($"[SQLite] Ruta: {FileAccessHelper.GetDatabasePath()}");
                Debug.WriteLine($"[SQLite] Existe: {FileAccessHelper.DatabaseExists()}");
                Debug.WriteLine($"[SQLite] Tamaño: {FileAccessHelper.GetFileSize(FileAccessHelper.DatabaseFileName)} bytes");
            }
            catch (Exception ex)
            {
                // Se guarda el error en vez de tragárselo en silencio.
                Repository.Message = $"Error al inicializar la base de datos: {ex.Message}";
                Debug.WriteLine($"[SQLite] {Repository.Message}");
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // AppShell se resuelve desde el contenedor para que reciba
            // vPersona (y ésta el PersonRepository) por constructor.
            var shell = _services.GetRequiredService<AppShell>();

            return new Window(shell);
        }
    }
}
