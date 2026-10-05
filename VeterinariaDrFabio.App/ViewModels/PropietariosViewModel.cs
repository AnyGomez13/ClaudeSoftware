using System.Collections.ObjectModel;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-03: lista de propietarios con búsqueda por nombre o teléfono y sus mascotas
/// (RF-03, RNF-01, CU-03).
/// </summary>
public class PropietariosViewModel : BaseViewModel
{
    private readonly IPropietarioService _propietarios;
    private readonly INavigationService _navegacion;
    private string _texto = string.Empty;
    private PropietarioFila? _seleccionado;

    public PropietariosViewModel(IPropietarioService propietarios, INavigationService navegacion)
    {
        _propietarios = propietarios;
        _navegacion = navegacion;
        NuevoPropietarioCommand = new RelayCommand(NuevoPropietario);
        VerCommand = new RelayCommand(parametro => Ver(parametro as PropietarioFila));
        EditarCommand = new RelayCommand(parametro => Editar(parametro as PropietarioFila));
        Buscar();
    }

    public ObservableCollection<PropietarioFila> Propietarios { get; } = [];

    public ObservableCollection<MascotaResumen> Mascotas { get; } = [];

    public RelayCommand NuevoPropietarioCommand { get; }

    public RelayCommand VerCommand { get; }

    public RelayCommand EditarCommand { get; }

    /// <summary>Texto de búsqueda; la lista se actualiza mientras se escribe.</summary>
    public string Texto
    {
        get => _texto;
        set
        {
            if (SetProperty(ref _texto, value))
            {
                Buscar();
            }
        }
    }

    public PropietarioFila? Seleccionado
    {
        get => _seleccionado;
        set
        {
            if (SetProperty(ref _seleccionado, value))
            {
                CargarMascotas();
            }
        }
    }

    public bool SinResultados => Propietarios.Count == 0;

    public bool HaySeleccion => Seleccionado is not null;

    public bool SeleccionSinMascotas => HaySeleccion && Mascotas.Count == 0;

    public string TituloMascotas => Seleccionado is null ? string.Empty : $"Mascotas de {Seleccionado.NombreCompleto}";

    /// <summary>Recarga la lista con el texto de búsqueda actual y conserva la selección si el propietario sigue en ella.</summary>
    public void Buscar()
    {
        var seleccionadoId = Seleccionado?.Id;

        Propietarios.Clear();
        foreach (var propietario in _propietarios.Buscar(Texto))
        {
            Propietarios.Add(new PropietarioFila(propietario));
        }

        OnPropertyChanged(nameof(SinResultados));
        Seleccionado = Propietarios.FirstOrDefault(f => f.Id == seleccionadoId);
        CargarMascotas();
    }

    private void CargarMascotas()
    {
        Mascotas.Clear();
        if (Seleccionado is not null)
        {
            foreach (var mascota in Seleccionado.Propietario.Mascotas.OrderBy(m => m.Nombre))
            {
                Mascotas.Add(new MascotaResumen(mascota));
            }
        }

        OnPropertyChanged(nameof(HaySeleccion));
        OnPropertyChanged(nameof(SeleccionSinMascotas));
        OnPropertyChanged(nameof(TituloMascotas));
    }

    private void Ver(PropietarioFila? fila)
    {
        if (fila is not null)
        {
            Seleccionado = fila;
        }
    }

    private void NuevoPropietario() =>
        _navegacion.NavegarA<PropietarioEdicionViewModel>(vm => vm.Nuevo(AlTerminarEdicion));

    private void Editar(PropietarioFila? fila)
    {
        if (fila is not null)
        {
            _navegacion.NavegarA<PropietarioEdicionViewModel>(vm => vm.Cargar(fila.Propietario, AlTerminarEdicion));
        }
    }

    private void AlTerminarEdicion(bool seGuardo)
    {
        _navegacion.Volver();
        if (seGuardo)
        {
            Buscar();
        }
    }
}
