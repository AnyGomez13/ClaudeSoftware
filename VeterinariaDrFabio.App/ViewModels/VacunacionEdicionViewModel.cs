using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-09: registro de una vacunación desde la ficha de la mascota. Con próxima fecha de refuerzo alimenta las
/// alertas y el carnet; el veterinario que la aplicó es obligatorio (RF-11, RN-07, RN-08, RNF-01, CU-09).
/// Al elegir una vacuna del listado propone la fecha del refuerzo según la vacuna, y esa fecha se puede cambiar.
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
    private bool _proximaFechaEscritaPorElUsuario;
    private IReadOnlyList<string> _vacunasSugeridas = [];
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

    /// <summary>Vacunas del listado para la especie de la mascota; el campo también admite texto libre (SUP-10, SUP-D11).</summary>
    public IReadOnlyList<string> VacunasSugeridas
    {
        get => _vacunasSugeridas;
        private set => SetProperty(ref _vacunasSugeridas, value);
    }

    /// <summary>Aviso bajo la fecha del refuerzo cuando la vacuna elegida está en el listado; vacío en caso contrario.</summary>
    public string SugerenciaRefuerzo =>
        CatalogoVacunas.Buscar(NombreVacuna) is { } vacuna
            ? $"Refuerzo habitual de esta vacuna: cada {vacuna.MesesRefuerzo} meses. Puede cambiar la fecha."
            : string.Empty;

    public bool HaySugerenciaRefuerzo => SugerenciaRefuerzo.Length > 0;

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
                OnPropertyChanged(nameof(SugerenciaRefuerzo));
                OnPropertyChanged(nameof(HaySugerenciaRefuerzo));
                ProponerRefuerzo();
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
                ProponerRefuerzo();
            }
        }
    }

    /// <summary>
    /// Fecha del próximo refuerzo; opcional. Si se informa, la vacuna aparece en las alertas (RF-14).
    /// Lo que escribe el usuario manda: desde entonces ya no se vuelve a proponer una fecha automática.
    /// </summary>
    public DateTime? ProximaFecha
    {
        get => _proximaFecha;
        set
        {
            if (SetProperty(ref _proximaFecha, value))
            {
                _proximaFechaEscritaPorElUsuario = true;
            }
        }
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
    /// <param name="especie">Especie de la mascota; con ella se ofrecen solo las vacunas que le corresponden.</param>
    public void Nueva(int mascotaId, string mascotaNombre, Action<bool>? alTerminar, string? especie = null)
    {
        _alTerminar = alTerminar;
        _mascotaId = mascotaId;
        _mascotaNombre = mascotaNombre;
        _proximaFechaEscritaPorElUsuario = false;
        OnPropertyChanged(nameof(Titulo));
        VacunasSugeridas = CatalogoVacunas.ParaEspecie(especie).Select(v => v.Nombre).ToList();

        Veterinarios = _veterinarios.ListarActivos().Select(v => new VeterinarioOpcion(v)).ToList();
        VeterinarioSeleccionado = null;
        NombreVacuna = string.Empty;
        FechaAplicacion = DateTime.Today;
        _proximaFecha = null;
        OnPropertyChanged(nameof(ProximaFecha));
        _proximaFechaEscritaPorElUsuario = false;
        Lote = string.Empty;
        Observaciones = string.Empty;
        MensajeError = string.Empty;
    }

    /// <summary>Propone la fecha del refuerzo según la vacuna y la fecha de aplicación, mientras el usuario no haya escrito la suya.</summary>
    private void ProponerRefuerzo()
    {
        if (_proximaFechaEscritaPorElUsuario)
        {
            return;
        }

        var propuesta = FechaAplicacion is { } aplicacion ? CatalogoVacunas.CalcularRefuerzo(NombreVacuna, aplicacion) : null;
        if (SetProperty(ref _proximaFecha, propuesta, nameof(ProximaFecha)))
        {
            // Una propuesta automática no cuenta como fecha escrita por el usuario.
            _proximaFechaEscritaPorElUsuario = false;
        }
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
