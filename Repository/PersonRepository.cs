using MauiApp1.Models;
using MauiApp1.Util;
using SQLite;

namespace MauiApp1.Repository
{
    public class PersonRepository
    {
        private const string DatabaseFileName = FileAccessHelper.DatabaseFileName;

        private static readonly SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private SQLiteAsyncConnection? _database;

        public string Message { get; internal set; } = string.Empty;

        public static string DatabasePath =>
            FileAccessHelper.GetDatabasePath();

        public async Task InitAsync()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(DatabasePath, Flags);
            await FileAccessHelper.EnsurePersonaSchemaAsync(_database);
        }

        public async Task<int> AddPersonAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la persona no puede estar vacío.", nameof(name));

            try
            {
                await InitAsync();

                var persona = new Persona
                {
                    Nombre = name.Trim()
                };

                int result = await _database!.InsertAsync(persona);

                if (result > 0)
                {
                    Message = $"Persona agregada correctamente (Id {persona.Id}).";
                    return persona.Id;
                }

                Message = "No se pudo agregar la persona.";
                return 0;
            }
            catch (Exception ex)
            {
                Message = $"Error al agregar la persona: {ex.Message}";
                return 0;
            }
        }

        public async Task<List<Persona>> GetPersonsAsync()
        {
            try
            {
                await InitAsync();

                return await _database!.Table<Persona>()
                    .OrderBy(p => p.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Message = $"Error al obtener las personas: {ex.Message}";
                return new List<Persona>();
            }
        }

        public async Task<Persona?> GetPersonByIdAsync(int id)
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Persona>().FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                Message = $"Error al buscar la persona: {ex.Message}";
                return null;
            }
        }

        public async Task<bool> SavePersonAsync(Persona persona)
        {
            ArgumentNullException.ThrowIfNull(persona);

            if (string.IsNullOrWhiteSpace(persona.Nombre))
                throw new ArgumentException("El nombre de la persona no puede estar vacío.", nameof(persona));

            try
            {
                await InitAsync();

                int result = persona.Id != 0
                    ? await _database!.UpdateAsync(persona)
                    : await _database!.InsertAsync(persona);

                Message = result > 0 ? "Cambios guardados." : "No se guardaron cambios.";
                return result > 0;
            }
            catch (Exception ex)
            {
                Message = $"Error al guardar la persona: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> DeletePersonAsync(int id)
        {
            try
            {
                await InitAsync();

                int result = await _database!.DeleteAsync<Persona>(id);
                Message = result > 0 ? "Persona eliminada." : "La persona no existe.";
                return result > 0;
            }
            catch (Exception ex)
            {
                Message = $"Error al eliminar la persona: {ex.Message}";
                return false;
            }
        }

        public async Task<int> DeleteAllAsync()
        {
            try
            {
                await InitAsync();
                return await _database!.DeleteAllAsync<Persona>();
            }
            catch (Exception ex)
            {
                Message = $"Error al limpiar la tabla: {ex.Message}";
                return 0;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                await InitAsync();
                return await _database!.Table<Persona>().CountAsync();
            }
            catch (Exception ex)
            {
                Message = $"Error al contar las personas: {ex.Message}";
                return 0;
            }
        }

        public async Task CloseAsync()
        {
            if (_database is null)
                return;

            await _database.CloseAsync();
            _database = null;
        }
    }
}
