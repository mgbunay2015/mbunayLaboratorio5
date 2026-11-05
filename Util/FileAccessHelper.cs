using MauiApp1.Models;
using SQLite;

namespace MauiApp1.Util
{
    /// <summary>
    /// Utilidades para el manejo de archivos locales de la aplicación.
    /// Cada plataforma (Android, iOS, Mac, Windows) expone su propio
    /// directorio privado de datos a través de <see cref="FileSystem.AppDataDirectory"/>.
    /// </summary>
    public static class FileAccessHelper
    {
        /// <summary>
        /// Nombre del archivo de la base de datos SQLite de la aplicación.
        /// </summary>
        public const string DatabaseFileName = "uisrael.db";

        /// <summary>
        /// Banderas de apertura usadas por todas las conexiones SQLite de la app.
        /// </summary>
        public static SQLiteOpenFlags DatabaseFlags =>
            SQLiteOpenFlags.ReadWrite |   // lectura y escritura
            SQLiteOpenFlags.Create |      // crea el archivo si no existe
            SQLiteOpenFlags.SharedCache;  // permite acceso desde varios hilos

        /// <summary>
        /// Devuelve la ruta física absoluta de un archivo dentro del
        /// directorio privado de datos de la app. Si el archivo no existe
        /// la ruta se devuelve igual (útil para crearlo después).
        /// </summary>
        /// <param name="filename">Nombre del archivo, por ejemplo "uisrael.db".</param>
        public static string GetLocalFilePath(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("El nombre del archivo no puede estar vacío.", nameof(filename));

            return Path.Combine(FileSystem.AppDataDirectory, filename);
        }

        /// <summary>
        /// Ruta absoluta del archivo de base de datos "uisrael.db".
        /// </summary>
        public static string GetDatabasePath()
        {
            return GetLocalFilePath(DatabaseFileName);
        }

        /// <summary>
        /// Crea la base de datos SQLite "uisrael.db" si no existe y
        /// crea la tabla Persona. Devuelve la conexión abierta.
        /// </summary>
        /// <param name="forceCreate">
        /// Si es true, borra el archivo existente y vuelve a crearlo desde cero.
        /// </param>
        public static SQLiteConnection CrearBaseDeDatos(bool forceCreate = false)
        {
            var path = GetDatabasePath();

            if (forceCreate && File.Exists(path))
                File.Delete(path);

            var connection = new SQLiteConnection(path, DatabaseFlags);
            EnsurePersonaSchema(connection);

            return connection;
        }

        /// <summary>
        /// Crea la base de datos SQLite "uisrael.db" de forma asíncrona
        /// si no existe y crea la tabla Persona. Devuelve la conexión abierta.
        /// </summary>
        /// <param name="forceCreate">
        /// Si es true, borra el archivo existente y vuelve a crearlo desde cero.
        /// </param>
        public static async Task<SQLiteAsyncConnection> CrearBaseDeDatosAsync(bool forceCreate = false)
        {
            var path = GetDatabasePath();

            if (forceCreate && File.Exists(path))
                File.Delete(path);

            var connection = new SQLiteAsyncConnection(path, DatabaseFlags);
            await EnsurePersonaSchemaAsync(connection);

            return connection;
        }

        /// <summary>Indica si el archivo ya existe en el almacenamiento local.</summary>
        public static bool FileExists(string filename)
        {
            return File.Exists(GetLocalFilePath(filename));
        }

        /// <summary>Indica si la base de datos "uisrael.db" ya existe.</summary>
        public static bool DatabaseExists()
        {
            return File.Exists(GetDatabasePath());
        }

        /// <summary>Tamaño del archivo en bytes (0 si no existe).</summary>
        public static long GetFileSize(string filename)
        {
            var path = GetLocalFilePath(filename);
            return File.Exists(path) ? new FileInfo(path).Length : 0;
        }

        /// <summary>
        /// Elimina el archivo local si existe. Útil para reiniciar
        /// la base de datos durante pruebas.
        /// </summary>
        /// <returns>true si el archivo fue eliminado.</returns>
        public static bool DeleteFile(string filename)
        {
            var path = GetLocalFilePath(filename);

            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }

        /// <summary>Elimina el archivo de base de datos "uisrael.db".</summary>
        public static bool DeleteDatabase()
        {
            return DeleteFile(DatabaseFileName);
        }

        /// <summary>
        /// Crea o ajusta la tabla Persona para que solo tenga Id y Nombre.
        /// </summary>
        public static void EnsurePersonaSchema(SQLiteConnection connection)
        {
            connection.CreateTable<Persona>();

            var columnas = connection.Query<ColumnaInfo>("PRAGMA table_info(Persona)");
            if (columnas.Count == 2 &&
                columnas.Any(c => c.name.Equals("Id", StringComparison.OrdinalIgnoreCase)) &&
                columnas.Any(c => c.name.Equals("Nombre", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (columnas.Count == 0)
            {
                connection.CreateTable<Persona>();
                return;
            }

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS Persona_new (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                    Nombre varchar(100) NOT NULL);
                """);
            connection.Execute("INSERT INTO Persona_new (Id, Nombre) SELECT Id, Nombre FROM Persona");
            connection.Execute("DROP TABLE Persona");
            connection.Execute("ALTER TABLE Persona_new RENAME TO Persona");
        }

        /// <summary>Versión asíncrona de <see cref="EnsurePersonaSchema"/>.</summary>
        public static async Task EnsurePersonaSchemaAsync(SQLiteAsyncConnection connection)
        {
            await connection.CreateTableAsync<Persona>();

            var columnas = await connection.QueryAsync<ColumnaInfo>("PRAGMA table_info(Persona)");
            if (columnas.Count == 2 &&
                columnas.Any(c => c.name.Equals("Id", StringComparison.OrdinalIgnoreCase)) &&
                columnas.Any(c => c.name.Equals("Nombre", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (columnas.Count == 0)
            {
                await connection.CreateTableAsync<Persona>();
                return;
            }

            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Persona_new (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                    Nombre varchar(100) NOT NULL);
                """);
            await connection.ExecuteAsync("INSERT INTO Persona_new (Id, Nombre) SELECT Id, Nombre FROM Persona");
            await connection.ExecuteAsync("DROP TABLE Persona");
            await connection.ExecuteAsync("ALTER TABLE Persona_new RENAME TO Persona");
        }

        private class ColumnaInfo
        {
            public string name { get; set; } = string.Empty;
        }
    }
}
