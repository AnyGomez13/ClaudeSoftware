using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-08: registro de un procedimiento médico desde la ficha de la mascota. El veterinario que lo evaluó
/// es obligatorio y el registro no se edita ni se borra después (RF-09, RN-07, RN-08, RNF-01, CU-08).
/// </summary>
public class ProcedimientoEdicionViewModel : BaseViewModel
{
    private readonly IProcedimientoService _procedimientos;
    private readonly IVeterinarioService _veterinarios;
    private Action<bool>? _alTerminar;
    private int _mascotaId;
    private string _mascotaNombre = string.Empty;
    private IReadOnlyList<VeterinarioOpcion> _opcionesVeterinario = [];
    private VeterinarioOpcion? _veterinarioSeleccionado;
    private DateTime? _fecha;
    private string _tipoProcedimiento = string.Empty;
    private string _descripcion = string.Empty;
    private string _tratamiento = string.Empty;
    private string _pesoTexto = string.Empty;
    private DateTime? _proximaFecha;
    private string _mensajeError = string.Empty;

    public ProcedimientoEdicionViewModel(IProcedimientoService procedimientos, IVeterinarioService veterinarios)
    {
        _procedimientos = procedimientos;
        _veterinarios = veterinarios;
        GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        CancelarCommand = new RelayCommand(Cancelar);
    }

    public RelayCommand GuardarCommand { get; }

    public RelayCommand CancelarCommand { get; }

    /// <summary>Sugerencias de tipo; el campo admite texto libre (SUP-D11). "Desparasitación" alimenta las alertas (A-09).</summary>
    public IReadOnlyList<string> TiposSugeridos { get; } = ["Consulta", "Cirugía", "Control", "Desparasitación"];

    /// <summary>Un procedimiento registra un acto ya realizado: no se admiten fechas futuras.</summary>
    public DateTime FechaMaxima => DateTime.Today;

    public string Titulo => $"Nuevo procedimiento — {_mascotaNombre}";

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

    public DateTime? Fecha
    {
        get => _fecha;
        set
        {
            if (SetProperty(ref _fecha, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TipoProcedimiento
    {
        get => _tipoProcedimiento;
        set
        {
            if (SetProperty(ref _tipoProcedimiento, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Descripcion
    {
        get => _descripcion;
        set
        {
            if (SetProperty(ref _descripcion, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Tratamiento
    {
        get => _tratamiento;
        set => SetProperty(ref _tratamiento, value);
    }

    /// <summary>Peso en kilogramos al momento de la atención; es opcional y admite coma o punto decimal (SUP-05).</summary>
    public string PesoTexto
    {
        get => _pesoTexto;
        set
        {
            if (SetProperty(ref _pesoTexto, value))
            {
                OnPropertyChanged(nameof(ErrorPeso));
                OnPropertyChanged(nameof(TieneErrorPeso));
            }
        }
    }

    public string ErrorPeso =>
        string.IsNullOrWhiteSpace(PesoTexto) || MascotaEdicionViewModel.TryLeerPeso(PesoTexto, out var peso) && peso > 0
            ? string.Empty
            : MascotaEdicionViewModel.MensajeFormatoPeso;

    public bool TieneErrorPeso => ErrorPeso.Length > 0;

    public DateTime? ProximaFecha
    {
        get => _proximaFecha;
        set => SetProperty(ref _proximaFecha, value);
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

    /// <summary>Prepara el formulario vacío para registrar un procedimiento de la mascota; la fecha parte en hoy.</summary>
    /// <param name="alTerminar">Se invoca con verdadero si se guardó y con falso si se canceló.</param>
    public void Nuevo(int mascotaId, string mascotaNombre, Action<bool>? alTerminar)
    {
        _alTerminar = alTerminar;
        _mascotaId = mascotaId;
        _mascotaNombre = mascotaNombre;
        OnPropertyChanged(nameof(Titulo));

        Veterinarios = _veterinarios.ListarActivos().Select(v => new VeterinarioOpcion(v)).ToList();
        VeterinarioSeleccionado = null;
        Fecha = DateTime.Today;
        TipoProcedimiento = string.Empty;
        Descripcion = string.Empty;
        Tratamiento = string.Empty;
        PesoTexto = string.Empty;
        ProximaFecha = null;
        MensajeError = string.Empty;
    }

    private bool PuedeGuardar() =>
        VeterinarioSeleccionado is not null
        && Fecha is not null
        && !string.IsNullOrWhiteSpace(TipoProcedimiento)
        && !string.IsNullOrWhiteSpace(Descripcion);

    private void Guardar()
    {
        var procedimiento = new Procedimiento
        {
            MascotaId = _mascotaId,
            VeterinarioId = VeterinarioSeleccionado?.Id ?? 0,
            Fecha = Fecha ?? default,
            TipoProcedimiento = TipoProcedimiento,
            Descripcion = Descripcion,
            Tratamiento = Tratamiento,
            PesoEnElMomento = LeerPesoOpcional(),
            ProximaFechaRecomendada = ProximaFecha,
        };

        var resultado = _procedimientos.Registrar(procedimiento);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return;
        }

        MensajeError = string.Empty;
        _alTerminar?.Invoke(true);
    }

    private void Cancelar() => _alTerminar?.Invoke(false);

    /// <summary>Vacío significa sin peso; un texto inválido se envía como 0 para que el servicio lo rechace.</summary>
    private double? LeerPesoOpcional()
    {
        if (string.IsNullOrWhiteSpace(PesoTexto))
        {
            return null;
        }

        return MascotaEdicionViewModel.TryLeerPeso(PesoTexto, out var peso) ? peso : 0;
    }
}
