using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-09: registro de una vacunación desde la ficha de la mascota. Con próxima fecha de refuerzo alimenta las
/// alertas y el carnet; el veterinario que la aplicó es obligatorio (RF-11, RN-07, RN-08, RNF-01, CU-09).
/// </summary>
public class VacunacionEdicionViewModel : BaseViewModel
{
    private readonly IVacunacionService _vacunaciones;
    private readonly IVeterinarioService _veterinarios;
    private Action<bool>? _alTerminar;
    private int _mascotaId;
    private string _mascotaNombre = string.Empty;
    private IReadOnlyList<VeterinarioOpcion> _opcionesVeterinario = [];
    private VeterinarioOpcion? _veterinarioSeleccionado;
    private string _nombreVacuna = string.Empty;
    private DateTime? _fechaAplicacion;
    private DateTime? _proximaFecha;
    private string _lote = string.Empty;
    private string _observaciones = string.Empty;
    private string _mensajeError = string.Empty;

    public VacunacionEdicionViewModel(IVacunacionService vacunaciones, IVeterinarioService veterinarios)
    {
        _vacunaciones = vacunaciones;
        _veterinarios = veterinarios;
        GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        CancelarCommand = new RelayCommand(Cancelar);
    }

    public RelayCommand GuardarCommand { get; }

    public RelayCommand CancelarCommand { get; }

    /// <summary>Sugerencias de vacuna; el campo admite texto libre y no hay catálogo cerrado (SUP-10, SUP-D11).</summary>
    public IReadOnlyList<string> VacunasSugeridas { get; } = ["Rabia", "Parvovirus", "Moquillo", "Triple canina", "Triple felina"];

    /// <summary>Una vacunación registra un acto ya realizado: no se admiten fechas futuras.</summary>
    public DateTime FechaMaxima => DateTime.Today;

    public string Titulo => $"Registrar vacuna — {_mascotaNombre}";

    public IReadOnlyList<VeterinarioOpcion> Veterinarios
    {
        get => _opcionesVeterinario;
        private set => SetProperty(ref _opcionesVeterinario, value);
    }

    public VeterinarioOpcion? VeterinarioSeleccionado
    {
        get => _veterinarioSeleccionado;
        set
        {
            if (SetProperty(ref _veterinarioSeleccionado, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string NombreVacuna
    {
        get => _nombreVacuna;
        set
        {
            if (SetProperty(ref _nombreVacuna, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public DateTime? FechaAplicacion
    {
        get => _fechaAplicacion;
        set
        {
            if (SetProperty(ref _fechaAplicacion, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Fecha del próximo refuerzo; opcional. Si se informa, la vacuna aparece en las alertas (RF-14).</summary>
    public DateTime? ProximaFecha
    {
        get => _proximaFecha;
        set => SetProperty(ref _proximaFecha, value);
    }

    /// <summary>Lote o laboratorio de la vacuna; opcional.</summary>
    public string Lote
    {
        get => _lote;
        set => SetProperty(ref _lote, value);
    }

    public string Observaciones
    {
        get => _observaciones;
        set => SetProperty(ref _observaciones, value);
    }

    /// <summary>Error devuelto por las reglas de negocio al guardar.</summary>
    public string MensajeError
    {
        get => _mensajeError;
        private set
        {
            if (SetProperty(ref _mensajeError, value))
            {
                OnPropertyChanged(nameof(TieneError));
            }
        }
    }

    public bool TieneError => MensajeError.Length > 0;

    /// <summary>Prepara el formulario vacío para registrar una vacunación de la mascota; la fecha parte en hoy.</summary>
    /// <param name="alTerminar">Se invoca con verdadero si se guardó y con falso si se canceló.</param>
    public void Nueva(int mascotaId, string mascotaNombre, Action<bool>? alTerminar)
    {
        _alTerminar = alTerminar;
        _mascotaId = mascotaId;
        _mascotaNombre = mascotaNombre;
        OnPropertyChanged(nameof(Titulo));

        Veterinarios = _veterinarios.ListarActivos().Select(v => new VeterinarioOpcion(v)).ToList();
        VeterinarioSeleccionado = null;
        NombreVacuna = string.Empty;
        FechaAplicacion = DateTime.Today;
        ProximaFecha = null;
        Lote = string.Empty;
        Observaciones = string.Empty;
        MensajeError = string.Empty;
    }

    private bool PuedeGuardar() =>
        VeterinarioSeleccionado is not null
        && FechaAplicacion is not null
        && !string.IsNullOrWhiteSpace(NombreVacuna);

    private void Guardar()
    {
        var vacunacion = new Vacunacion
        {
            MascotaId = _mascotaId,
            VeterinarioId = VeterinarioSeleccionado?.Id ?? 0,
            NombreVacuna = NombreVacuna,
            FechaAplicacion = FechaAplicacion ?? default,
            ProximaFecha = ProximaFecha,
            Lote = Lote,
            Observaciones = Observaciones,
        };

        var resultado = _vacunaciones.Registrar(vacunacion);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return;
        }

        MensajeError = string.Empty;
        _alTerminar?.Invoke(true);
    }

    private void Cancelar() => _alTerminar?.Invoke(false);
}
