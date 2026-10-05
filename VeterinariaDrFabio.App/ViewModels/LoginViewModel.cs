using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Pantalla P-01: inicio de sesión (RF-01, RNF-05, RN-01, CU-01).</summary>
public class LoginViewModel : BaseViewModel
{
    public const string MensajeCredencialesInvalidas = "Credenciales inválidas";

    private readonly IAutenticacionService _autenticacion;
    private string _usuario = string.Empty;
    private string _contrasena = string.Empty;
    private string _mensajeError = string.Empty;

    public LoginViewModel(IAutenticacionService autenticacion)
    {
        _autenticacion = autenticacion;
        IniciarSesionCommand = new RelayCommand(IniciarSesion, PuedeIniciarSesion);
    }

    /// <summary>Se dispara cuando las credenciales son válidas y la sesión queda iniciada.</summary>
    public event Action? SesionIniciada;

    public RelayCommand IniciarSesionCommand { get; }

    public string Usuario
    {
        get => _usuario;
        set
        {
            if (SetProperty(ref _usuario, value))
            {
                IniciarSesionCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Contrasena
    {
        get => _contrasena;
        set
        {
            if (SetProperty(ref _contrasena, value))
            {
                IniciarSesionCommand.RaiseCanExecuteChanged();
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

    public bool TieneError => !string.IsNullOrEmpty(MensajeError);

    /// <summary>Deja el formulario vacío, por ejemplo al cerrar sesión.</summary>
    public void Reiniciar()
    {
        Usuario = string.Empty;
        Contrasena = string.Empty;
        MensajeError = string.Empty;
    }

    private bool PuedeIniciarSesion() =>
        !string.IsNullOrWhiteSpace(Usuario) && !string.IsNullOrEmpty(Contrasena);

    private void IniciarSesion()
    {
        if (_autenticacion.IniciarSesion(Usuario, Contrasena))
        {
            Reiniciar();
            SesionIniciada?.Invoke();
            return;
        }

        MensajeError = MensajeCredencialesInvalidas;
        Contrasena = string.Empty;
    }
}
