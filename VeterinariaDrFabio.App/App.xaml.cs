using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Datos.Contexto;

namespace VeterinariaDrFabio.App;

/// <summary>
/// Punto de arranque de la aplicación y contenedor de inyección de dependencias (SUP-D09).
/// </summary>
public partial class App : Application
{
    public IServiceProvider Servicios { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Sin esto WPF formatea fechas y números como en-US en las vistas (por ejemplo el DatePicker).
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        Servicios = ConfigurarServicios();
        InicializarBaseDatos();

        if (ComandoCrearUsuario.Solicitado(e.Args))
        {
            Shutdown(ComandoCrearUsuario.Ejecutar(e.Args, Servicios));
            return;
        }

        var ventanaPrincipal = Servicios.GetRequiredService<MainWindow>();
        ventanaPrincipal.Show();
    }

    private static IServiceProvider ConfigurarServicios()
    {
        var servicios = new ServiceCollection();
        ComposicionDeServicios.Registrar(servicios, RutaBaseDatos.CadenaConexion());
        return servicios.BuildServiceProvider();
    }

    /// <summary>Crea la base de datos local en el primer arranque o aplica las migraciones pendientes (RNF-03).</summary>
    private void InicializarBaseDatos()
    {
        using var contexto = Servicios.GetRequiredService<VeterinariaDbContext>();
        contexto.Database.Migrate();
    }
}
