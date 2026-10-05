using System.Windows;
using Microsoft.Extensions.DependencyInjection;

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

        Servicios = ConfigurarServicios();

        var ventanaPrincipal = Servicios.GetRequiredService<MainWindow>();
        ventanaPrincipal.Show();
    }

    private static IServiceProvider ConfigurarServicios()
    {
        var servicios = new ServiceCollection();

        // Los repositorios, servicios y ViewModels se registran en las fases siguientes.
        servicios.AddTransient<MainWindow>();

        return servicios.BuildServiceProvider();
    }
}
