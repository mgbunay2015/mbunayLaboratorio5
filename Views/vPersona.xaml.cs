using MauiApp1.Models;
using MauiApp1.Repository;
namespace MauiApp1.Views;

public partial class vPersona : ContentPage
{
    private readonly PersonRepository _repository;

    public vPersona(PersonRepository repository)
    {
        InitializeComponent();
        _repository = repository;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        PersonasView.ItemsSource = Array.Empty<Persona>();

        await _repository.InitAsync();
    }

    private async void OnInsertarClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            await DisplayAlertAsync("Insertar", "Escribe un nombre.", "OK");
            return;
        }

        try
        {
            var persona = new Persona { Nombre = NombreEntry.Text.Trim() };
            // InsertAsync escribe en el archivo SQLite (uisrael.db) de forma permanente.
            bool ok = await _repository.SavePersonAsync(persona);

            if (!ok)
            {
                await DisplayAlertAsync("Insertar", _repository.Message, "OK");
                return;
            }

            NombreEntry.Text = string.Empty;
            IdEntry.Text = "*";
            await MostrarSegunBusquedaAsync();
        }
        catch (ArgumentException ex)
        {
            await DisplayAlertAsync("Insertar", ex.Message, "OK");
        }
    }

    private async void OnMostrarClicked(object? sender, EventArgs e)
    {
        await MostrarSegunBusquedaAsync();
    }

    private async void OnEliminarClicked(object? sender, EventArgs e)
    {
        var texto = IdEntry.Text?.Trim() ?? string.Empty;

        if (texto == "*" || string.IsNullOrEmpty(texto))
        {
            await DisplayAlertAsync("Eliminar", "Escribe el Id numérico a eliminar (no uses *).", "OK");
            return;
        }

        if (!int.TryParse(texto, out var id) || id <= 0)
        {
            await DisplayAlertAsync("Eliminar", "Id no válido.", "OK");
            return;
        }

        if (!await DisplayAlertAsync("Eliminar", $"¿Eliminar el registro Id {id}?", "Sí", "No"))
            return;

        bool ok = await _repository.DeletePersonAsync(id);
        if (!ok)
        {
            await DisplayAlertAsync("Eliminar", _repository.Message, "OK");
            return;
        }

        IdEntry.Text = "*";
        await MostrarSegunBusquedaAsync();
    }

    /// <summary>
    /// Despliega en el listado según la caja Id: * = todos, número = ese Id.
    /// </summary>
    private async Task MostrarSegunBusquedaAsync()
    {
        var texto = IdEntry.Text?.Trim() ?? string.Empty;

        if (texto == "*")
        {
            var todas = await _repository.GetPersonsAsync();
            PersonasView.ItemsSource = todas;
            return;
        }

        if (!int.TryParse(texto, out var id) || id <= 0)
        {
            await DisplayAlertAsync("Mostrar", "Escribe un Id numérico o * para todos.", "OK");
            PersonasView.ItemsSource = Array.Empty<Persona>();
            return;
        }

        var persona = await _repository.GetPersonByIdAsync(id);
        PersonasView.ItemsSource = persona is null
            ? Array.Empty<Persona>()
            : new List<Persona> { persona };
    }
}
