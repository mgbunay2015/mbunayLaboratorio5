using MauiApp1.Models;
using MauiApp1.Util;
using SQLite;

namespace MauiApp1.Repository
{
    /// <summary>
    /// Repositorio de acceso a datos para la tabla Persona.
    /// Encapsula la conexión SQLite y expone las operaciones CRUD,
    /// de modo que las vistas/páginas no conozcan SQLite directamente.
    /// </summary>
    public class PersonRepository
    {
        private const string DatabaseFileName = FileAccessHelper.DatabaseFileName;

        private static readonly SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private SQLiteAsyncConnection? _database;

        /// <summary>Último mensaje de resultado o de error.</summary>
        public string Message { get; internal set; } = string.Empty;

        /// <summary>Ruta física del archivo de base de datos.</summary>
        public static string DatabasePath =>
            FileAccessHelper.GetDatabasePath();

        /// <summary>
        /// Abre la conexión (una sola vez) y crea la tabla si no existe.
        /// Es seguro llamarlo varias veces o desde varios hilos.
        /// </summary>
        public async Task InitAsync()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(DatabasePath, Flags);
            await FileAccessHelper.EnsurePersonaSchemaAsync(_database);
        }

        /// <summary>Inserta una persona nueva (solo Nombre; Id autogenerado).</summary>
        /// <returns>Id generado, o 0 si hubo error.</returns>
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

        /// <summary>Devuelve todas las personas ordenadas por apellido y nombre.</summary>
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

        /// <summary>Busca una persona por Id. Devuelve null si no existe.</summary>
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

        /// <summary>Inserta o actualiza según el Id de la persona.</summary>
        /// <returns>true si la operación se aplicó.</returns>
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

        /// <summary>Elimina una persona por Id.</summary>
        /// <returns>true si se eliminó.</returns>
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

        /// <summary>Elimina todos los registros de la tabla.</summary>
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

        /// <summary>Total de personas registradas.</summary>
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

        /// <summary>
        /// Cierra la conexión. Útil al cerrar la app o en pruebas.
        /// </summary>
        public async Task CloseAsync()
        {
            if (_database is null)
                return;

            await _database.CloseAsync();
            _database = null;
        }
    }
}
