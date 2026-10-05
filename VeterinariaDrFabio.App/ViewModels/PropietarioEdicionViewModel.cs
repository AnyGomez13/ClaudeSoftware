using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-04: formulario de propietario, nuevo o en edición, con validación en vivo del celular
/// (RF-02, RF-04, RN-04, RN-14, SUP-08, RNF-01, CU-02, CU-04).
/// </summary>
public class PropietarioEdicionViewModel : BaseViewModel
{
    public const string MensajeFormatoTelefono = "Escriba un celular colombiano: 10 dígitos que inicia en 3 (ejemplo: 3001234567).";

    private readonly IPropietarioService _propietarios;
    private readonly IDialogService _dialogos;
    private Action<bool>? _alTerminar;
    private int _id;
    private string _nombreCompleto = string.Empty;
    private string _telefono = string.Empty;
    private string _documento = string.Empty;
    private string _correo = string.Empty;
    private string _direccion = string.Empty;
    private bool _activo = true;
    private bool _esNuevo = true;
    private string _mensajeError = string.Empty;

    public PropietarioEdicionViewModel(IPropietarioService propietarios, IDialogService dialogos)
    {
        _propietarios = propietarios;
        _dialogos = dialogos;
        GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        CancelarCommand = new RelayCommand(Cancelar);
        CambiarEstadoCommand = new RelayCommand(CambiarEstado, () => !EsNuevo);
    }

    public RelayCommand GuardarCommand { get; }

    public RelayCommand CancelarCommand { get; }

    /// <summary>Inactiva o reactiva al propietario; nunca lo elimina (SUP-08).</summary>
    public RelayCommand CambiarEstadoCommand { get; }

    public string Titulo => EsNuevo ? "Nuevo propietario" : "Editar propietario";

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

    /// <summary>Verdadero en modo edición, cuando se ofrece Inactivar o Reactivar.</summary>
    public bool EsEdicion => !EsNuevo;

    public string TextoCambiarEstado => Activo ? "Inactivar" : "Reactivar";

    public string NombreCompleto
    {
        get => _nombreCompleto;
        set
        {
            if (SetProperty(ref _nombreCompleto, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Telefono
    {
        get => _telefono;
        set
        {
            if (SetProperty(ref _telefono, value))
            {
                OnPropertyChanged(nameof(ErrorTelefono));
                OnPropertyChanged(nameof(TieneErrorTelefono));
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Documento
    {
        get => _documento;
        set => SetProperty(ref _documento, value);
    }

    public string Correo
    {
        get => _correo;
        set => SetProperty(ref _correo, value);
    }

    public string Direccion
    {
        get => _direccion;
        set => SetProperty(ref _direccion, value);
    }

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

    /// <summary>Aviso en vivo bajo el campo de teléfono: vacío mientras no se escribe nada o el formato es válido.</summary>
    public string ErrorTelefono =>
        string.IsNullOrWhiteSpace(Telefono) || PropietarioService.EsCelularColombiano(Telefono.Trim())
            ? string.Empty
            : MensajeFormatoTelefono;

    public bool TieneErrorTelefono => ErrorTelefono.Length > 0;

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

    /// <summary>Prepara el formulario vacío para registrar un propietario.</summary>
    /// <param name="alTerminar">Se invoca con verdadero si se guardó o cambió el estado, y con falso si se canceló.</param>
    public void Nuevo(Action<bool>? alTerminar)
    {
        Cargar(new Propietario(), alTerminar, esNuevo: true);
    }

    /// <summary>Prepara el formulario con los datos de un propietario existente.</summary>
    public void Cargar(Propietario propietario, Action<bool>? alTerminar) => Cargar(propietario, alTerminar, esNuevo: false);

    private void Cargar(Propietario propietario, Action<bool>? alTerminar, bool esNuevo)
    {
        _alTerminar = alTerminar;
        _id = propietario.Id;
        EsNuevo = esNuevo;
        NombreCompleto = propietario.NombreCompleto;
        Telefono = propietario.Telefono;
        Documento = propietario.Documento ?? string.Empty;
        Correo = propietario.Correo ?? string.Empty;
        Direccion = propietario.Direccion ?? string.Empty;
        Activo = propietario.Activo;
        MensajeError = string.Empty;
    }

    private bool PuedeGuardar() =>
        !string.IsNullOrWhiteSpace(NombreCompleto) && !string.IsNullOrWhiteSpace(Telefono);

    private void Guardar()
    {
        var propietario = ConstruirPropietario(Activo);
        var resultado = EsNuevo ? _propietarios.Registrar(propietario) : _propietarios.Editar(propietario);
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
                "Inactivar propietario",
                $"¿Inactivar a {NombreCompleto.Trim()}? No se elimina: sus datos y los de sus mascotas se conservan."))
        {
            return;
        }

        var resultado = _propietarios.Editar(ConstruirPropietario(!Activo));
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return;
        }

        MensajeError = string.Empty;
        _alTerminar?.Invoke(true);
    }

    private Propietario ConstruirPropietario(bool activo) => new()
    {
        Id = _id,
        NombreCompleto = NombreCompleto,
        Telefono = Telefono,
        Documento = Documento,
        Correo = Correo,
        Direccion = Direccion,
        Activo = activo,
    };
}
