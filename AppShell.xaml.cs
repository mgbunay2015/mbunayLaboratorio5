using MauiApp1.Views;

namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell(vPersona persona)
        {
            InitializeComponent();

            // Se asigna la instancia ya construida por el contenedor de
            // dependencias, así vPersona recibe PersonRepository por constructor.
            PersonasContent.Content = persona;
        }
    }
}
