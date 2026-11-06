using MauiApp1.Models;
using SQLite;

namespace MauiApp1.Util
{
    public static class FileAccessHelper
    {
        public const string DatabaseFileName = "uisrael.db";

        public static SQLiteOpenFlags DatabaseFlags =>
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        public static string GetLocalFilePath(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("El nombre del archivo no puede estar vacío.", nameof(filename));

            return Path.Combine(FileSystem.AppDataDirectory, filename);
        }

        public static string GetDatabasePath()
        {
            return GetLocalFilePath(DatabaseFileName);
        }

        public static SQLiteConnection CrearBaseDeDatos(bool forceCreate = false)
        {
            var path = GetDatabasePath();

            if (forceCreate && File.Exists(path))
                File.Delete(path);

            var connection = new SQLiteConnection(path, DatabaseFlags);
            EnsurePersonaSchema(connection);

            return connection;
        }

        public static async Task<SQLiteAsyncConnection> CrearBaseDeDatosAsync(bool forceCreate = false)
        {
            var path = GetDatabasePath();

            if (forceCreate && File.Exists(path))
                File.Delete(path);

            var connection = new SQLiteAsyncConnection(path, DatabaseFlags);
            await EnsurePersonaSchemaAsync(connection);

            return connection;
        }

        public static bool FileExists(string filename)
        {
            return File.Exists(GetLocalFilePath(filename));
        }

        public static bool DatabaseExists()
        {
            return File.Exists(GetDatabasePath());
        }

        public static long GetFileSize(string filename)
        {
            var path = GetLocalFilePath(filename);
            return File.Exists(path) ? new FileInfo(path).Length : 0;
        }

        public static bool DeleteFile(string filename)
        {
            var path = GetLocalFilePath(filename);

            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }

        public static bool DeleteDatabase()
        {
            return DeleteFile(DatabaseFileName);
        }

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
