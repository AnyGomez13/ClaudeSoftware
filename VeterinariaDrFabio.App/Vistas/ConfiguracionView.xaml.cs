using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VeterinariaDrFabio.App.ViewModels;

namespace VeterinariaDrFabio.App.Vistas;

/// <summary>
/// Vista P-13. <see cref="PasswordBox.Password"/> no admite binding, por eso el código de la vista solo
/// sincroniza las tres contraseñas con el ViewModel; no contiene lógica.
/// </summary>
public partial class ConfiguracionView : UserControl
{
    private bool _sincronizando;

    public ConfiguracionView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void CampoActual_PasswordChanged(object sender, RoutedEventArgs e) =>
        Sincronizar(modelo => modelo.ContrasenaActual = CampoActual.Password);

    private void CampoNueva_PasswordChanged(object sender, RoutedEventArgs e) =>
        Sincronizar(modelo => modelo.ContrasenaNueva = CampoNueva.Password);

    private void CampoConfirmacion_PasswordChanged(object sender, RoutedEventArgs e) =>
        Sincronizar(modelo => modelo.Confirmacion = CampoConfirmacion.Password);

    private void Sincronizar(Action<ConfiguracionViewModel> asignar)
    {
        if (_sincronizando || DataContext is not ConfiguracionViewModel modelo)
        {
            return;
        }

        _sincronizando = true;
        asignar(modelo);
        _sincronizando = false;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ConfiguracionViewModel anterior)
        {
            anterior.PropertyChanged -= OnModeloCambio;
        }

        if (e.NewValue is ConfiguracionViewModel nuevo)
        {
            nuevo.PropertyChanged += OnModeloCambio;
        }
    }

    /// <summary>Cuando el ViewModel vacía una contraseña (por ejemplo tras cambiarla) se vacía también el campo.</summary>
    private void OnModeloCambio(object? sender, PropertyChangedEventArgs e)
    {
        if (_sincronizando || sender is not ConfiguracionViewModel modelo)
        {
            return;
        }

        _sincronizando = true;
        if (e.PropertyName == nameof(ConfiguracionViewModel.ContrasenaActual) && modelo.ContrasenaActual.Length == 0)
        {
            CampoActual.Clear();
        }
        else if (e.PropertyName == nameof(ConfiguracionViewModel.ContrasenaNueva) && modelo.ContrasenaNueva.Length == 0)
        {
            CampoNueva.Clear();
        }
        else if (e.PropertyName == nameof(ConfiguracionViewModel.Confirmacion) && modelo.Confirmacion.Length == 0)
        {
            CampoConfirmacion.Clear();
        }

        _sincronizando = false;
    }
}
