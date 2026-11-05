using MauiApp1.Models;
using MauiApp1.Util;
using SQLite;

namespace MauiApp1.Data
{
    /// <summary>
    /// Servicio que administra la conexión con la base de datos SQLite
    /// y las operaciones CRUD sobre la tabla Persona.
    /// </summary>
    public class PersonaDatabase
    {
        private const string DatabaseFileName = FileAccessHelper.DatabaseFileName;

        private const SQLiteOpenFlags Flags =
            // Abre la base de datos en modo lectura/escritura
            SQLiteOpenFlags.ReadWrite |
            // Crea la base de datos si no existe
            SQLiteOpenFlags.Create |
            // Permite acceso desde varios hilos (necesario en apps MAUI)
            SQLiteOpenFlags.SharedCache;

        private SQLiteAsyncConnection? _database;

        /// <summary>
        /// Ruta física donde se guardará el archivo .db3 en cada plataforma,
        /// resuelta a través de <see cref="FileAccessHelper"/>.
        /// </summary>
        public static string DatabasePath =>
            FileAccessHelper.GetDatabasePath();

        /// <summary>
        /// Crea la conexión (si no existe) y la tabla Persona (si no existe).
        /// </summary>
        private async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_database is not null)
                return _database;

            _database = new SQLiteAsyncConnection(DatabasePath, Flags);
            await FileAccessHelper.EnsurePersonaSchemaAsync(_database);

            return _database;
        }

        /// <summary>Obtiene todas las personas registradas.</summary>
        public async Task<List<Persona>> ObtenerPersonasAsync()
        {
            var db = await GetConnectionAsync();
            return await db.Table<Persona>().OrderBy(p => p.Nombre).ToListAsync();
        }

        /// <summary>Obtiene una persona por su Id (o null si no existe).</summary>
        public async Task<Persona?> ObtenerPersonaAsync(int id)
        {
            var db = await GetConnectionAsync();
            return await db.Table<Persona>().FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>Inserta una persona nueva o actualiza una existente.</summary>
        public async Task<int> GuardarPersonaAsync(Persona persona)
        {
            var db = await GetConnectionAsync();

            if (persona.Id != 0)
                return await db.UpdateAsync(persona);

            return await db.InsertAsync(persona);
        }

        /// <summary>Elimina una persona de la base de datos.</summary>
        public async Task<int> EliminarPersonaAsync(Persona persona)
        {
            var db = await GetConnectionAsync();
            return await db.DeleteAsync(persona);
        }

        /// <summary>Elimina todas las personas de la tabla.</summary>
        public async Task<int> EliminarTodasAsync()
        {
            var db = await GetConnectionAsync();
            return await db.DeleteAllAsync<Persona>();
        }

        /// <summary>Cuenta cuántos registros hay en la tabla Persona.</summary>
        public async Task<int> ContarPersonasAsync()
        {
            var db = await GetConnectionAsync();
            return await db.Table<Persona>().CountAsync();
        }

        // ------------------------------------------------------------------
        // API síncrona (Init + Message), estilo DAO clásico
        // ------------------------------------------------------------------

        private SQLiteConnection? _db;

        /// <summary>
        /// Último mensaje de resultado o de error producido por esta clase.
        /// </summary>
        public string Message { get; private set; } = string.Empty;

        /// <summary>
        /// Abre la conexión síncrona (si no está abierta) y crea la tabla.
        /// Se puede llamar varias veces sin problema.
        /// </summary>
        public void Init()
        {
            if (_db is not null)
                return;

            _db = new SQLiteConnection(DatabasePath, Flags);
            FileAccessHelper.EnsurePersonaSchema(_db);
        }

        /// <summary>Inserta una persona (solo Nombre).</summary>
        public bool AddPerson(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la persona no puede estar vacío.", nameof(name));

            try
            {
                Init();

                var person = new Persona
                {
                    Nombre = name.Trim()
                };

                int result = _db!.Insert(person);

                if (result > 0)
                {
                    Message = $"Persona agregada correctamente (Id {person.Id}).";
                    return true;
                }

                Message = "No se pudo agregar la persona.";
                return false;
            }
            catch (Exception ex)
            {
                Message = $"Error al agregar la persona: {ex.Message}";
                return false;
            }
        }
    }
}
