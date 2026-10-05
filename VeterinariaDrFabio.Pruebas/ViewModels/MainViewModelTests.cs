using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="MainViewModel"/>, el shell P-02 (RF-01, RN-13).</summary>
public class MainViewModelTests : PantallaTestBase
{
    private readonly MainViewModel _modelo;

    public MainViewModelTests()
        : base(iniciarSesion: false)
    {
        _modelo = Servicios.GetRequiredService<MainViewModel>();
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
        Assert.Null(_modelo.Navegacion.ViewModelActual);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ElMenuTieneLasCuatroSeccionesDelDiseno()
    {
        Assert.Equal(["Propietarios", "Mascotas", "Alertas", "Veterinarios"], _modelo.Secciones.Select(s => s.Titulo).ToList());
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_AlIniciarSesionSeMuestraElShellConPropietariosYElUsuario()
    {
        IniciarSesion();

        Assert.True(_modelo.SesionActiva);
        Assert.False(_modelo.MostrarLogin);
        Assert.Equal("Propietarios", _modelo.SeccionActual);
        Assert.Equal(AutenticacionFalsa.UsuarioValido, _modelo.NombreUsuario);
        Assert.Equal(["Propietarios"], _modelo.Secciones.Where(s => s.EsActual).Select(s => s.Titulo).ToList());
        Assert.IsType<PropietariosViewModel>(_modelo.Navegacion.ViewModelActual);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ElMenuAbreLaPantallaDeCadaSeccionImplementada()
    {
        IniciarSesion();

        _modelo.NavegarSeccionCommand.Execute("Veterinarios");
        Assert.Equal("Veterinarios", _modelo.SeccionActual);
        Assert.IsType<VeterinariosViewModel>(_modelo.Navegacion.ViewModelActual);
        Assert.Equal(["Veterinarios"], _modelo.Secciones.Where(s => s.EsActual).Select(s => s.Titulo).ToList());

        _modelo.NavegarSeccionCommand.Execute("Mascotas");
        Assert.Equal("Mascotas", _modelo.SeccionActual);
        Assert.IsType<MascotasViewModel>(_modelo.Navegacion.ViewModelActual);

        _modelo.NavegarSeccionCommand.Execute("Propietarios");
        Assert.IsType<PropietariosViewModel>(_modelo.Navegacion.ViewModelActual);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_UnaSeccionSinPantallaTodaviaDejaElAreaDeContenidoVacia()
    {
        IniciarSesion();

        _modelo.NavegarSeccionCommand.Execute("Alertas");

        Assert.Equal("Alertas", _modelo.SeccionActual);
        Assert.Null(_modelo.Navegacion.ViewModelActual);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_UnaSeccionDesconocidaNoCambiaNada()
    {
        IniciarSesion();

        _modelo.NavegarSeccionCommand.Execute("Facturas");

        Assert.Equal("Propietarios", _modelo.SeccionActual);
        Assert.IsType<PropietariosViewModel>(_modelo.Navegacion.ViewModelActual);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_CerrarSesionVuelveAlLoginYLimpiaElEstado()
    {
        IniciarSesion();

        _modelo.CerrarSesionCommand.Execute(null);

        Assert.False(Autenticacion.SesionActiva);
        Assert.False(_modelo.SesionActiva);
        Assert.True(_modelo.MostrarLogin);
        Assert.Equal(string.Empty, _modelo.SeccionActual);
        Assert.Equal(string.Empty, _modelo.NombreUsuario);
        Assert.All(_modelo.Secciones, s => Assert.False(s.EsActual));
        Assert.Equal(string.Empty, _modelo.Login.Usuario);
        Assert.Null(_modelo.Navegacion.ViewModelActual);
        Assert.False(_modelo.NavegarSeccionCommand.CanExecute("Mascotas"));
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_UnLoginFallidoNoMuestraElShellNiNingunaPantalla()
    {
        _modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
        _modelo.Login.Contrasena = "incorrecta";
        _modelo.Login.IniciarSesionCommand.Execute(null);

        Assert.False(_modelo.SesionActiva);
        Assert.True(_modelo.MostrarLogin);
        Assert.Null(_modelo.Navegacion.ViewModelActual);
    }
}
