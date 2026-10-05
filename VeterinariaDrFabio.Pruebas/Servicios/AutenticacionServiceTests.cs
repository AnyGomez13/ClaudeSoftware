using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="AutenticacionService"/> (RF-01, RNF-05, RN-01, RN-13, SUP-07).</summary>
public class AutenticacionServiceTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private AutenticacionService CrearServicio()
    {
        var contexto = _bd.CrearContexto();
        return new AutenticacionService(new UsuarioRepository(contexto), new HasherContrasena());
    }

    private AutenticacionService ServicioConUsuario(string usuario = "fabio", string clave = "Clave-Segura-1")
    {
        var servicio = CrearServicio();
        Assert.True(servicio.CrearUsuarioInicial(usuario, clave).Exito);
        return servicio;
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_CredencialesValidasConcedenAcceso()
    {
        var servicio = ServicioConUsuario();

        Assert.True(servicio.IniciarSesion("fabio", "Clave-Segura-1"));
        Assert.True(servicio.SesionActiva);
        Assert.Equal("fabio", servicio.NombreUsuario);
    }

    [Theory]
    [Trait("Req", "RF-01")]
    [InlineData("fabio", "otra-clave")]
    [InlineData("fabio", "clave-segura-1")]
    [InlineData("nadie", "Clave-Segura-1")]
    [InlineData("", "Clave-Segura-1")]
    [InlineData("fabio", "")]
    [InlineData("   ", "   ")]
    public void RF01_CredencialesInvalidasNiegaElAcceso(string usuario, string clave)
    {
        var servicio = ServicioConUsuario();

        Assert.False(servicio.IniciarSesion(usuario, clave));
        Assert.False(servicio.SesionActiva);
        Assert.Null(servicio.NombreUsuario);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_UnUsuarioInactivoNoPuedeIniciarSesion()
    {
        var servicio = ServicioConUsuario();
        _bd.Ejecutar("UPDATE Usuario SET Activo = 0;");

        Assert.False(CrearServicio().IniciarSesion("fabio", "Clave-Segura-1"));
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_CerrarSesionLimpiaElEstado()
    {
        var servicio = ServicioConUsuario();
        servicio.IniciarSesion("fabio", "Clave-Segura-1");

        servicio.CerrarSesion();

        Assert.False(servicio.SesionActiva);
        Assert.Null(servicio.NombreUsuario);
    }

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_UnIntentoFallidoDejaSinSesionAUnaSesionAnterior()
    {
        var servicio = ServicioConUsuario();
        servicio.IniciarSesion("fabio", "Clave-Segura-1");

        Assert.False(servicio.IniciarSesion("fabio", "incorrecta"));
        Assert.False(servicio.SesionActiva);
    }

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_SinIniciarSesionNoHayAcceso()
    {
        var servicio = CrearServicio();

        Assert.False(servicio.SesionActiva);
        Assert.Null(servicio.NombreUsuario);
    }

    [Fact]
    [Trait("Req", "RNF-05")]
    public void RNF05_LaContrasenaSeGuardaSoloComoHashConSal()
    {
        ServicioConUsuario(clave: "Clave-Segura-1");

        var hash = _bd.Escalar<string>("SELECT ContrasenaHash FROM Usuario;");
        var sal = _bd.Escalar<string>("SELECT Salt FROM Usuario;");

        Assert.NotEqual("Clave-Segura-1", hash);
        Assert.DoesNotContain("Clave-Segura-1", hash);
        Assert.False(string.IsNullOrEmpty(sal));
        Assert.True(new HasherContrasena().Verificar("Clave-Segura-1", hash!, sal!));
    }

    [Fact]
    [Trait("Req", "SUP-07")]
    public void SUP07_SoloSePuedeCrearUnUsuario()
    {
        var servicio = ServicioConUsuario();

        var segundo = servicio.CrearUsuarioInicial("william", "Otra-Clave-2");

        Assert.False(segundo.Exito);
        Assert.NotEmpty(segundo.Mensaje);
        Assert.Equal(1, _bd.Escalar<int>("SELECT COUNT(*) FROM Usuario;"));
    }

    [Theory]
    [Trait("Req", "SUP-07")]
    [InlineData("", "clave")]
    [InlineData("   ", "clave")]
    [InlineData("fabio", "")]
    public void SUP07_NombreOContrasenaVaciosNoCreanElUsuario(string usuario, string clave)
    {
        var resultado = CrearServicio().CrearUsuarioInicial(usuario, clave);

        Assert.False(resultado.Exito);
        Assert.Equal(0, _bd.Escalar<int>("SELECT COUNT(*) FROM Usuario;"));
    }

    [Fact]
    [Trait("Req", "SUP-07")]
    public void SUP07_ElNombreDeUsuarioSeGuardaSinEspaciosAlrededor()
    {
        var servicio = ServicioConUsuario("  fabio  ");

        Assert.Equal("fabio", _bd.Escalar<string>("SELECT NombreUsuario FROM Usuario;"));
        Assert.True(servicio.IniciarSesion(" fabio ", "Clave-Segura-1"));
    }
}
