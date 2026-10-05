using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>
/// Base de las pruebas de pantallas: usa el mismo registro de dependencias que la aplicación real,
/// con una base SQLite temporal, una sesión ya iniciada y diálogos de prueba.
/// </summary>
public abstract class PantallaTestBase : IDisposable
{
    /// <param name="iniciarSesion">Falso para probar el estado previo al login.</param>
    protected PantallaTestBase(bool iniciarSesion = true)
    {
        if (iniciarSesion)
        {
            Autenticacion.IniciarSesion(AutenticacionFalsa.UsuarioValido, AutenticacionFalsa.ClaveValida);
        }

        Servicios = ComposicionDePrueba.Crear(Bd, Autenticacion, Dialogos);
    }

    protected BaseDatosTemporal Bd { get; } = new();

    protected AutenticacionFalsa Autenticacion { get; } = new();

    protected DialogoFalso Dialogos { get; } = new();

    protected ServiceProvider Servicios { get; }

    protected INavigationService Navegacion => Servicios.GetRequiredService<INavigationService>();

    public void Dispose()
    {
        Servicios.Dispose();
        Bd.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>Arma el contenedor de dependencias de la aplicación sobre una base de prueba.</summary>
internal static class ComposicionDePrueba
{
    public static ServiceProvider Crear(BaseDatosTemporal bd, IAutenticacionService autenticacion, IDialogService dialogos)
    {
        var servicios = new ServiceCollection();
        ComposicionDeServicios.Registrar(servicios, bd.CadenaConexion);

        // El último registro gana: la sesión y los diálogos se sustituyen por los de prueba.
        servicios.AddSingleton(autenticacion);
        servicios.AddSingleton(dialogos);
        return servicios.BuildServiceProvider();
    }
}
