namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Opción del menú lateral del shell (P-02); marca cuál es la sección actual.</summary>
public class ItemMenu : BaseViewModel
{
    private bool _esActual;

    public ItemMenu(string titulo)
    {
        Titulo = titulo;
    }

    public string Titulo { get; }

    public bool EsActual
    {
        get => _esActual;
        set => SetProperty(ref _esActual, value);
    }
}
