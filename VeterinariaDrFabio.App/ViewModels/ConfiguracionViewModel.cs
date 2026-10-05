using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-13 (agregada a pedido del usuario): configuración de la cuenta. Permite cambiar la contraseña
/// de la única cuenta de acceso verificando antes la contraseña actual (RF-17, RNF-05, CU-14).
/// </summary>
public class ConfiguracionViewModel : BaseViewModel
{
    public const string MensajeConfirmacionDistinta = "La confirmación no coincide con la nueva contraseña.";
    public const string MensajeContrasenaActualizada = "La contraseña se actualizó correctamente.";

    private readonly IAutenticacionService _autenticacion;
    private string _contrasenaActual = string.Empty;
    private string _contrasenaNueva = string.Empty;
    private string _confirmacion = string.Empty;
    private string _mensajeError = string.Empty;
    private string _mensajeExito = string.Empty;

    public ConfiguracionViewModel(IAutenticacionService autenticacion)
    {
        _autenticacion = autenticacion;
        CambiarContrasenaCommand = new RelayCommand(CambiarContrasena, PuedeCambiar);
    }

    public RelayCommand CambiarContrasenaCommand { get; }

    /// <summary>Usuario con sesión iniciada, cuya contraseña se cambia.</summary>
    public string NombreUsuario => _autenticacion.NombreUsuario ?? string.Empty;

    public string ContrasenaActual
    {
        get => _contrasenaActual;
        set
        {
            if (SetProperty(ref _contrasenaActual, value))
            {
                CambiarContrasenaCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ContrasenaNueva
    {
        get => _contrasenaNueva;
        set
        {
            if (SetProperty(ref _contrasenaNueva, value))
            {
                CambiarContrasenaCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Confirmacion
    {
        get => _confirmacion;
        set
        {
            if (SetProperty(ref _confirmacion, value))
            {
                CambiarContrasenaCommand.RaiseCanExecuteChanged();
            }
        }
    }

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

    public string MensajeExito
    {
        get => _mensajeExito;
        private set
        {
            if (SetProperty(ref _mensajeExito, value))
            {
                OnPropertyChanged(nameof(TieneExito));
            }
        }
    }

    public bool TieneExito => MensajeExito.Length > 0;

    private bool PuedeCambiar() =>
        !string.IsNullOrEmpty(ContrasenaActual) && !string.IsNullOrEmpty(ContrasenaNueva) && !string.IsNullOrEmpty(Confirmacion);

    private void CambiarContrasena()
    {
        MensajeExito = string.Empty;

        if (ContrasenaNueva != Confirmacion)
        {
            MensajeError = MensajeConfirmacionDistinta;
            return;
        }

        var resultado = _autenticacion.CambiarContrasena(ContrasenaActual, ContrasenaNueva);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return;
        }

        MensajeError = string.Empty;
        ContrasenaActual = string.Empty;
        ContrasenaNueva = string.Empty;
        Confirmacion = string.Empty;
        MensajeExito = MensajeContrasenaActualizada;
    }
}
