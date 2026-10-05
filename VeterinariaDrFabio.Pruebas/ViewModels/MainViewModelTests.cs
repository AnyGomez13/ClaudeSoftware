using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.App.ViewModels;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="MainViewModel"/>, el shell P-02 (RF-01, RN-13).</summary>
public class MainViewModelTests
{
    private readonly AutenticacionFalsa _autenticacion = new();
    private readonly MainViewModel _modelo;

    public MainViewModelTests()
    {
        var servicios = new ServiceCollection().BuildServiceProvider();
        var login = new LoginViewModel(_autenticacion);
        _modelo = new MainViewModel(_autenticacion, login, new NavigationService(servicios, _autenticacion));
    }

    private void IniciarSesion()
    {
        _modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
        _modelo.Login.Contrasena = AutenticacionFalsa.ClaveValida;
        _modelo.Login.IniciarSesionCommand.Execute(null);
    }

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_AlArrancarSoloSeMuestraElLoginYNoHayAccesoAlMenu()
    {
        Assert.False(_modelo.SesionActiva);
        Assert.True(_modelo.MostrarLogin);
        Assert.False(_modelo.NavegarSeccionCommand.CanExecute("Mascotas"));
        Assert.False(_modelo.CerrarSesionCommand.CanExecute(null));
        Assert.Equal(string.Empty, _modelo.SeccionActual);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ElMenuTieneLasCuatroSeccionesDelDiseno()
    {
        Assert.Equal(["Propietarios", "Mascotas", "Alertas", "Veterinarios"], _modelo.Secciones.Select(s => s.Titulo).ToList());
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_AlIniciarSesionSeMuestraElShellConLaPrimeraSeccionYElUsuario()
    {
        IniciarSesion();

        Assert.True(_modelo.SesionActiva);
        Assert.False(_modelo.MostrarLogin);
        Assert.Equal("Propietarios", _modelo.SeccionActual);
        Assert.Equal(AutenticacionFalsa.UsuarioValido, _modelo.NombreUsuario);
        Assert.Equal(["Propietarios"], _modelo.Secciones.Where(s => s.EsActual).Select(s => s.Titulo).ToList());
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ElMenuCambiaDeSeccion()
    {
        IniciarSesion();

        _modelo.NavegarSeccionCommand.Execute("Alertas");

        Assert.Equal("Alertas", _modelo.SeccionActual);
        Assert.Equal(["Alertas"], _modelo.Secciones.Where(s => s.EsActual).Select(s => s.Titulo).ToList());
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_UnaSeccionDesconocidaNoCambiaNada()
    {
        IniciarSesion();

        _modelo.NavegarSeccionCommand.Execute("Facturas");

        Assert.Equal("Propietarios", _modelo.SeccionActual);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_CerrarSesionVuelveAlLoginYLimpiaElEstado()
    {
        IniciarSesion();

        _modelo.CerrarSesionCommand.Execute(null);

        Assert.False(_autenticacion.SesionActiva);
        Assert.False(_modelo.SesionActiva);
        Assert.True(_modelo.MostrarLogin);
        Assert.Equal(string.Empty, _modelo.SeccionActual);
        Assert.Equal(string.Empty, _modelo.NombreUsuario);
        Assert.All(_modelo.Secciones, s => Assert.False(s.EsActual));
        Assert.Equal(string.Empty, _modelo.Login.Usuario);
        Assert.False(_modelo.NavegarSeccionCommand.CanExecute("Mascotas"));
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_UnLoginFallidoNoMuestraElShell()
    {
        _modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
        _modelo.Login.Contrasena = "incorrecta";
        _modelo.Login.IniciarSesionCommand.Execute(null);

        Assert.False(_modelo.SesionActiva);
        Assert.True(_modelo.MostrarLogin);
    }
}
