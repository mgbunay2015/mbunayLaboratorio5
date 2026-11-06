using MauiApp1.Models;
using MauiApp1.Util;
using SQLite;

namespace MauiApp1.Data
{
    public class PersonaDatabase
    {
        private const string DatabaseFileName = FileAccessHelper.DatabaseFileName;

        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private SQLiteAsyncConnection? _database;

        public static string DatabasePath =>
            FileAccessHelper.GetDatabasePath();

        private async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_database is not null)
                return _database;

            _database = new SQLiteAsyncConnection(DatabasePath, Flags);
            await FileAccessHelper.EnsurePersonaSchemaAsync(_database);

            return _database;
        }

        public async Task<List<Persona>> ObtenerPersonasAsync()
        {
            var db = await GetConnectionAsync();
            return await db.Table<Persona>().OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task<Persona?> ObtenerPersonaAsync(int id)
        {
            var db = await GetConnectionAsync();
            return await db.Table<Persona>().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<int> GuardarPersonaAsync(Persona persona)
        {
            var db = await GetConnectionAsync();

            if (persona.Id != 0)
                return await db.UpdateAsync(persona);

            return await db.InsertAsync(persona);
        }

        public async Task<int> EliminarPersonaAsync(Persona persona)
        {
            var db = await GetConnectionAsync();
            return await db.DeleteAsync(persona);
        }

        public async Task<int> EliminarTodasAsync()
        {
            var db = await GetConnectionAsync();
            return await db.DeleteAllAsync<Persona>();
        }

        public async Task<int> ContarPersonasAsync()
        {
            var db = await GetConnectionAsync();
            return await db.Table<Persona>().CountAsync();
        }

        private SQLiteConnection? _db;

        public string Message { get; private set; } = string.Empty;

        public void Init()
        {
            if (_db is not null)
                return;

            _db = new SQLiteConnection(DatabasePath, Flags);
            FileAccessHelper.EnsurePersonaSchema(_db);
        }

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
