using System.Globalization;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-06: formulario de mascota, nueva o en edición, con la edad calculada en vivo
/// (RF-05, RF-07, RF-08, RN-03, RN-06, SUP-04, SUP-08, RNF-01, CU-05, CU-07).
/// </summary>
public class MascotaEdicionViewModel : BaseViewModel
{
    public const string MensajeFormatoPeso = "Escriba el peso en kilogramos, mayor a cero (ejemplo: 12,5).";
    public const string MensajeFechaFutura = "La fecha de nacimiento no puede ser futura.";
    public const string SexoNoEspecificado = "No especificado";

    private readonly IMascotaService _mascotas;
    private readonly IPropietarioService _propietarios;
    private readonly IDialogService _dialogos;
    private Action<bool>? _alTerminar;
    private int _id;
    private bool _esNuevo = true;
    private bool _activo = true;
    private IReadOnlyList<PropietarioOpcion> _opcionesPropietario = [];
    private PropietarioOpcion? _propietarioSeleccionado;
    private string _nombre = string.Empty;
    private string _especie = string.Empty;
    private string _raza = string.Empty;
    private string _sexoSeleccionado = SexoNoEspecificado;
    private DateTime? _fechaNacimiento;
    private bool _fechaEstimada;
    private string _pesoTexto = string.Empty;
    private string _colorSenas = string.Empty;
    private string _mensajeError = string.Empty;

    public MascotaEdicionViewModel(IMascotaService mascotas, IPropietarioService propietarios, IDialogService dialogos)
    {
        _mascotas = mascotas;
        _propietarios = propietarios;
        _dialogos = dialogos;
        GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        CancelarCommand = new RelayCommand(Cancelar);
        CambiarEstadoCommand = new RelayCommand(CambiarEstado, () => !EsNuevo);
    }

    public RelayCommand GuardarCommand { get; }

    public RelayCommand CancelarCommand { get; }

    /// <summary>Inactiva o reactiva la mascota; nunca la elimina (SUP-08).</summary>
    public RelayCommand CambiarEstadoCommand { get; }

    /// <summary>Opciones del selector de sexo: el sexo es opcional.</summary>
    public IReadOnlyList<string> OpcionesSexo { get; } = [SexoNoEspecificado, "Macho", "Hembra"];

    /// <summary>Sugerencias de especie; el campo admite texto libre (SUP-D11).</summary>
    public IReadOnlyList<string> EspeciesSugeridas { get; } = ["Perro", "Gato", "Ave", "Conejo"];

    /// <summary>Una mascota no puede nacer después de hoy.</summary>
    public DateTime FechaMaxima => DateTime.Today;

    public string Titulo => EsNuevo ? "Nueva mascota" : "Editar mascota";

    public bool EsNuevo
    {
        get => _esNuevo;
        private set
        {
            if (SetProperty(ref _esNuevo, value))
            {
                OnPropertyChanged(nameof(Titulo));
                OnPropertyChanged(nameof(EsEdicion));
                CambiarEstadoCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool EsEdicion => !EsNuevo;

    public bool Activo
    {
        get => _activo;
        private set
        {
            if (SetProperty(ref _activo, value))
            {
                OnPropertyChanged(nameof(TextoCambiarEstado));
            }
        }
    }

    public string TextoCambiarEstado => Activo ? "Inactivar" : "Reactivar";

    public IReadOnlyList<PropietarioOpcion> Propietarios
    {
        get => _opcionesPropietario;
        private set
        {
            if (SetProperty(ref _opcionesPropietario, value))
            {
                OnPropertyChanged(nameof(NoHayPropietarios));
            }
        }
    }

    /// <summary>Sin propietarios no se puede registrar una mascota: hay que crear uno primero (RN-03).</summary>
    public bool NoHayPropietarios => Propietarios.Count == 0;

    public PropietarioOpcion? PropietarioSeleccionado
    {
        get => _propietarioSeleccionado;
        set
        {
            if (SetProperty(ref _propietarioSeleccionado, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Nombre
    {
        get => _nombre;
        set
        {
            if (SetProperty(ref _nombre, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Especie
    {
        get => _especie;
        set
        {
            if (SetProperty(ref _especie, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Raza
    {
        get => _raza;
        set => SetProperty(ref _raza, value);
    }

    public string SexoSeleccionado
    {
        get => _sexoSeleccionado;
        set => SetProperty(ref _sexoSeleccionado, value ?? SexoNoEspecificado);
    }

    public DateTime? FechaNacimiento
    {
        get => _fechaNacimiento;
        set
        {
            if (SetProperty(ref _fechaNacimiento, value))
            {
                OnPropertyChanged(nameof(EdadCalculada));
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Marca que la fecha de nacimiento es una estimación (SUP-04).</summary>
    public bool FechaEstimada
    {
        get => _fechaEstimada;
        set => SetProperty(ref _fechaEstimada, value);
    }

    /// <summary>Edad calculada en vivo desde la fecha de nacimiento escrita (RF-08).</summary>
    public string EdadCalculada
    {
        get
        {
            if (FechaNacimiento is not { } fecha)
            {
                return "—";
            }

            return fecha.Date > DateTime.Today
                ? MensajeFechaFutura
                : _mascotas.CalcularEdadLegible(new Mascota { FechaNacimiento = fecha });
        }
    }

    /// <summary>Peso en kilogramos tal como se escribe; admite coma o punto decimal.</summary>
    public string PesoTexto
    {
        get => _pesoTexto;
        set
        {
            if (SetProperty(ref _pesoTexto, value))
            {
                OnPropertyChanged(nameof(ErrorPeso));
                OnPropertyChanged(nameof(TieneErrorPeso));
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Aviso en vivo bajo el campo de peso: vacío mientras no se escribe nada o el valor es válido.</summary>
    public string ErrorPeso =>
        string.IsNullOrWhiteSpace(PesoTexto) || TryLeerPeso(PesoTexto, out var peso) && peso > 0
            ? string.Empty
            : MensajeFormatoPeso;

    public bool TieneErrorPeso => ErrorPeso.Length > 0;

    public string ColorSenas
    {
        get => _colorSenas;
        set => SetProperty(ref _colorSenas, value);
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

    /// <summary>Prepara el formulario vacío para registrar una mascota.</summary>
    /// <param name="alTerminar">Se invoca con verdadero si se guardó o cambió el estado, y con falso si se canceló.</param>
    public void Nueva(Action<bool>? alTerminar) => Preparar(new Mascota(), alTerminar, esNuevo: true);

    /// <summary>Prepara el formulario con los datos de una mascota existente.</summary>
    public void Cargar(Mascota mascota, Action<bool>? alTerminar) => Preparar(mascota, alTerminar, esNuevo: false);

    /// <summary>Lee un peso escrito con coma o punto decimal; solo acepta números finitos.</summary>
    public static bool TryLeerPeso(string? texto, out double peso)
    {
        peso = 0;
        var normalizado = texto?.Trim().Replace(',', '.');
        return !string.IsNullOrEmpty(normalizado)
            && double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out peso)
            && double.IsFinite(peso);
    }

    private void Preparar(Mascota mascota, Action<bool>? alTerminar, bool esNuevo)
    {
        _alTerminar = alTerminar;
        _id = mascota.Id;
        EsNuevo = esNuevo;
        Activo = mascota.Activo;

        // Se ofrecen los propietarios activos y, al editar, también el actual aunque esté inactivo.
        Propietarios = _propietarios.Buscar(string.Empty)
            .Where(p => p.Activo || p.Id == mascota.PropietarioId)
            .Select(p => new PropietarioOpcion(p))
            .ToList();
        PropietarioSeleccionado = Propietarios.FirstOrDefault(p => p.Id == mascota.PropietarioId);

        Nombre = mascota.Nombre;
        Especie = mascota.Especie;
        Raza = mascota.Raza ?? string.Empty;
        SexoSeleccionado = mascota.Sexo ?? SexoNoEspecificado;
        FechaNacimiento = esNuevo ? null : mascota.FechaNacimiento;
        FechaEstimada = mascota.FechaNacimientoEstimada;
        PesoTexto = esNuevo ? string.Empty : mascota.Peso.ToString("0.##", CultureInfo.GetCultureInfo("es-CO"));
        ColorSenas = mascota.ColorSenas ?? string.Empty;
        MensajeError = string.Empty;
    }

    private bool PuedeGuardar() =>
        PropietarioSeleccionado is not null
        && !string.IsNullOrWhiteSpace(Nombre)
        && !string.IsNullOrWhiteSpace(Especie)
        && FechaNacimiento is not null
        && !string.IsNullOrWhiteSpace(PesoTexto);

    private void Guardar()
    {
        var mascota = ConstruirMascota(Activo);
        var resultado = EsNuevo ? _mascotas.Registrar(mascota) : _mascotas.Editar(mascota);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return;
        }

        MensajeError = string.Empty;
        _alTerminar?.Invoke(true);
    }

    private void Cancelar() => _alTerminar?.Invoke(false);

    private void CambiarEstado()
    {
        if (Activo && !_dialogos.Confirmar(
                "Inactivar mascota",
                $"¿Inactivar a {Nombre.Trim()}? No se elimina: su historia clínica se conserva."))
        {
            return;
        }

        var resultado = _mascotas.Editar(ConstruirMascota(!Activo));
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return;
        }

        MensajeError = string.Empty;
        _alTerminar?.Invoke(true);
    }

    private Mascota ConstruirMascota(bool activo) => new()
    {
        Id = _id,
        PropietarioId = PropietarioSeleccionado?.Id ?? 0,
        Nombre = Nombre,
        Especie = Especie,
        Raza = Raza,
        Sexo = SexoSeleccionado == SexoNoEspecificado ? null : SexoSeleccionado,
        FechaNacimiento = FechaNacimiento ?? default,
        FechaNacimientoEstimada = FechaEstimada,
        Peso = TryLeerPeso(PesoTexto, out var peso) ? peso : 0,
        ColorSenas = ColorSenas,
        Activo = activo,
    };
}
