using System.ComponentModel;
using VeterinariaDrFabio.App.ViewModels;

namespace VeterinariaDrFabio.App.Infraestructura;

/// <summary>
/// Navegación entre vistas sin que los ViewModels conozcan WPF (§1.3). La vista que se muestra depende del
/// ViewModel actual. Sin sesión iniciada no se puede navegar (RF-01, RN-13).
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
    /// <summary>ViewModel de la vista que se muestra en el área de contenido; nulo si no hay ninguna.</summary>
    BaseViewModel? ViewModelActual { get; }

    bool PuedeVolver { get; }

    /// <summary>Muestra la vista de <typeparamref name="TViewModel"/> y recuerda la anterior para <see cref="Volver"/>.</summary>
    /// <param name="inicializar">Entrega al ViewModel los datos que necesita antes de mostrarse (por ejemplo la mascota).</param>
    /// <exception cref="InvalidOperationException">No hay sesión iniciada.</exception>
    void NavegarA<TViewModel>(Action<TViewModel>? inicializar = null)
        where TViewModel : BaseViewModel;

    /// <summary>Como <see cref="NavegarA{TViewModel}"/> pero descarta el historial: es la entrada a una sección del menú.</summary>
    void NavegarASeccion<TViewModel>(Action<TViewModel>? inicializar = null)
        where TViewModel : BaseViewModel;

    void Volver();

    /// <summary>Quita la vista actual y el historial; se usa al cerrar sesión.</summary>
    void Limpiar();
}
