using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.Infraestructura;

/// <inheritdoc cref="INavigationService"/>
public class NavigationService : BaseViewModel, INavigationService
{
    private readonly IServiceProvider _servicios;
    private readonly IAutenticacionService _autenticacion;
    private readonly Stack<BaseViewModel> _historial = new();
    private BaseViewModel? _viewModelActual;

    public NavigationService(IServiceProvider servicios, IAutenticacionService autenticacion)
    {
        _servicios = servicios;
        _autenticacion = autenticacion;
    }

    public BaseViewModel? ViewModelActual
    {
        get => _viewModelActual;
        private set => SetProperty(ref _viewModelActual, value);
    }

    public bool PuedeVolver => _historial.Count > 0;

    public void NavegarA<TViewModel>(Action<TViewModel>? inicializar = null)
        where TViewModel : BaseViewModel
    {
        var destino = CrearDestino(inicializar);
        if (ViewModelActual is not null)
        {
            _historial.Push(ViewModelActual);
        }

        ViewModelActual = destino;
        OnPropertyChanged(nameof(PuedeVolver));
    }

    public void NavegarASeccion<TViewModel>(Action<TViewModel>? inicializar = null)
        where TViewModel : BaseViewModel
    {
        var destino = CrearDestino(inicializar);
        _historial.Clear();
        ViewModelActual = destino;
        OnPropertyChanged(nameof(PuedeVolver));
    }

    public void Volver()
    {
        if (_historial.Count == 0)
        {
            return;
        }

        ViewModelActual = _historial.Pop();
        OnPropertyChanged(nameof(PuedeVolver));
    }

    public void Limpiar()
    {
        _historial.Clear();
        ViewModelActual = null;
        OnPropertyChanged(nameof(PuedeVolver));
    }

    private TViewModel CrearDestino<TViewModel>(Action<TViewModel>? inicializar)
        where TViewModel : BaseViewModel
    {
        if (!_autenticacion.SesionActiva)
        {
            throw new InvalidOperationException("Debe iniciar sesión para acceder a esta vista.");
        }

        var destino = _servicios.GetRequiredService<TViewModel>();
        inicializar?.Invoke(destino);
        return destino;
    }
}
