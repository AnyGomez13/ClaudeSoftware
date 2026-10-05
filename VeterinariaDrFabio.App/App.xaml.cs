using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;

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

        // Cada resolución recibe su propio contexto, de modo que un error al guardar no contamina a los demás.
        servicios.AddDbContext<VeterinariaDbContext>(
            opciones => opciones.UseSqlite(RutaBaseDatos.CadenaConexion()),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        servicios.AddTransient<IUsuarioRepository, UsuarioRepository>();
        servicios.AddTransient<IVeterinarioRepository, VeterinarioRepository>();

        servicios.AddTransient<HasherContrasena>();

        // La sesión vive mientras la aplicación esté abierta, por eso el servicio es único.
        servicios.AddSingleton<IAutenticacionService, AutenticacionService>();
        servicios.AddTransient<IVeterinarioService, VeterinarioService>();

        // Los demás repositorios, servicios y ViewModels se registran en las fases siguientes.
        servicios.AddTransient<MainWindow>();

        return servicios.BuildServiceProvider();
    }

    /// <summary>Crea la base de datos local en el primer arranque o aplica las migraciones pendientes (RNF-03).</summary>
    private void InicializarBaseDatos()
    {
        using var contexto = Servicios.GetRequiredService<VeterinariaDbContext>();
        contexto.Database.Migrate();
    }
}
