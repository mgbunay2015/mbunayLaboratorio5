using MauiApp1.Views;

namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell(vPersona persona)
        {
            InitializeComponent();
            PersonasContent.Content = persona;
        }
    }
}
