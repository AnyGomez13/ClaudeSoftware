using System.Collections.ObjectModel;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-05: búsqueda de mascotas por su nombre o el del propietario, con la edad calculada
/// (RF-06, RF-08, RNF-01, CU-06).
/// </summary>
public class MascotasViewModel : BaseViewModel
{
    private readonly IMascotaService _mascotas;
    private readonly INavigationService _navegacion;
    private string _texto = string.Empty;

    public MascotasViewModel(IMascotaService mascotas, INavigationService navegacion)
    {
        _mascotas = mascotas;
        _navegacion = navegacion;
        NuevaMascotaCommand = new RelayCommand(NuevaMascota);
        AbrirFichaCommand = new RelayCommand(parametro => AbrirFicha(parametro as MascotaFila));
        EditarCommand = new RelayCommand(parametro => Editar(parametro as MascotaFila));
        Buscar();
    }

    public ObservableCollection<MascotaFila> Mascotas { get; } = [];

    public RelayCommand NuevaMascotaCommand { get; }

    public RelayCommand AbrirFichaCommand { get; }

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

    public bool SinResultados => Mascotas.Count == 0;

    /// <summary>Recarga la lista con el texto de búsqueda actual.</summary>
    public void Buscar()
    {
        Mascotas.Clear();
        foreach (var mascota in _mascotas.Buscar(Texto))
        {
            Mascotas.Add(new MascotaFila(mascota, EdadLegible(mascota)));
        }

        OnPropertyChanged(nameof(SinResultados));
    }

    private string EdadLegible(Mascota mascota)
    {
        try
        {
            return _mascotas.CalcularEdadLegible(mascota);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Un dato con fecha de nacimiento futura no debe impedir mostrar la lista.
            return "—";
        }
    }

    private void NuevaMascota() =>
        _navegacion.NavegarA<MascotaEdicionViewModel>(vm => vm.Nueva(AlTerminarEdicion));

    private void Editar(MascotaFila? fila)
    {
        if (fila is not null)
        {
            _navegacion.NavegarA<MascotaEdicionViewModel>(vm => vm.Cargar(fila.Mascota, AlTerminarEdicion));
        }
    }

    private void AbrirFicha(MascotaFila? fila)
    {
        if (fila is not null)
        {
            _navegacion.NavegarA<MascotaDetalleViewModel>(vm => vm.Cargar(fila.Id));
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
