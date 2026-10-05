using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VeterinariaDrFabio.App.ViewModels;

namespace VeterinariaDrFabio.App.Vistas;

/// <summary>
/// Vista P-01. <see cref="PasswordBox.Password"/> no admite binding, por eso el código de la vista solo
/// sincroniza la contraseña con el ViewModel; no contiene lógica.
/// </summary>
public partial class LoginView : UserControl
{
    private bool _sincronizando;

    public LoginView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void CampoContrasena_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_sincronizando || DataContext is not LoginViewModel modelo)
        {
            return;
        }

        _sincronizando = true;
        modelo.Contrasena = CampoContrasena.Password;
        _sincronizando = false;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is LoginViewModel anterior)
        {
            anterior.PropertyChanged -= OnModeloCambio;
        }

        if (e.NewValue is LoginViewModel nuevo)
        {
            nuevo.PropertyChanged += OnModeloCambio;
        }
    }

    /// <summary>Cuando el ViewModel vacía la contraseña (intento fallido o cierre de sesión) se vacía también el campo.</summary>
    private void OnModeloCambio(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoginViewModel.Contrasena)
            && sender is LoginViewModel modelo
            && modelo.Contrasena.Length == 0
            && !_sincronizando)
        {
            _sincronizando = true;
            CampoContrasena.Clear();
            _sincronizando = false;
        }
    }
}
