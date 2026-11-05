using MauiApp1.Models;
using MauiApp1.Repository;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private readonly PersonRepository _repository;
        private Persona? _personaEditando;

        public MainPage(PersonRepository repository)
        {
            InitializeComponent();
            _repository = repository;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarPersonasAsync();
        }

        private async Task CargarPersonasAsync()
        {
            var personas = await _repository.GetPersonsAsync();
            PersonasView.ItemsSource = personas;
            EstadoLabel.Text = $"Total: {personas.Count}";
        }

        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NombreEntry.Text))
            {
                EstadoLabel.Text = "El nombre es obligatorio.";
                return;
            }

            var persona = _personaEditando ?? new Persona();
            persona.Nombre = NombreEntry.Text.Trim();

            bool guardado = await _repository.SavePersonAsync(persona);
            EstadoLabel.Text = guardado ? _repository.Message : $"Error: {_repository.Message}";

            if (!guardado)
                return;

            IdEntry.Text = persona.Id.ToString();
            _personaEditando = null;
            NombreEntry.Text = string.Empty;
            await CargarPersonasAsync();
        }

        private void OnLimpiarClicked(object? sender, EventArgs e)
        {
            _personaEditando = null;
            IdEntry.Text = string.Empty;
            NombreEntry.Text = string.Empty;
            EstadoLabel.Text = string.Empty;
        }
    }
}
