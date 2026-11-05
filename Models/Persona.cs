using SQLite;

namespace MauiApp1.Models
{
    [Table("Persona")]
    public class Persona
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100), NotNull]
        public string Nombre { get; set; } = string.Empty;
    }
}
