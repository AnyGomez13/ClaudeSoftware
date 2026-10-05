using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas del cambio de contraseña de <see cref="AutenticacionService"/> (RF-17, RNF-05, CU-14).</summary>
public class CambioDeContrasenaTests : IDisposable
{
    private const string ClaveInicial = "Clave-Inicial-1";
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private AutenticacionService CrearServicio()
    {
        var contexto = _bd.CrearContexto();
        return new AutenticacionService(new UsuarioRepository(contexto), new HasherContrasena());
    }

    private AutenticacionService ServicioConSesion()
    {
        var servicio = CrearServicio();
        Assert.True(servicio.CrearUsuarioInicial("villasancarlos", ClaveInicial).Exito);
        Assert.True(servicio.IniciarSesion("villasancarlos", ClaveInicial));
        return servicio;
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_ConLaContrasenaActualCorrectaSeCambiaYLaNuevaPermiteEntrar()
    {
        var servicio = ServicioConSesion();

        var resultado = servicio.CambiarContrasena(ClaveInicial, "Clave-Nueva-2");

        Assert.True(resultado.Exito);
        var otraSesion = CrearServicio();
        Assert.False(otraSesion.IniciarSesion("villasancarlos", ClaveInicial));
        Assert.True(otraSesion.IniciarSesion("villasancarlos", "Clave-Nueva-2"));
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_LaSesionSigueActivaTrasCambiarLaContrasena()
    {
        var servicio = ServicioConSesion();

        servicio.CambiarContrasena(ClaveInicial, "Clave-Nueva-2");

        Assert.True(servicio.SesionActiva);
        Assert.Equal("villasancarlos", servicio.NombreUsuario);
    }

    [Fact]
    [Trait("Req", "RNF-05")]
    public void RNF05_LaNuevaContrasenaSeGuardaSoloComoHashConSalNueva()
    {
        var servicio = ServicioConSesion();
        var hashAntes = _bd.Escalar<string>("SELECT ContrasenaHash FROM Usuario;");
        var salAntes = _bd.Escalar<string>("SELECT Salt FROM Usuario;");

        servicio.CambiarContrasena(ClaveInicial, "Clave-Nueva-2");

        var hash = _bd.Escalar<string>("SELECT ContrasenaHash FROM Usuario;");
        var sal = _bd.Escalar<string>("SELECT Salt FROM Usuario;");
        Assert.NotEqual(hashAntes, hash);
        Assert.NotEqual(salAntes, sal);
        Assert.DoesNotContain("Clave-Nueva-2", hash);
        Assert.True(new HasherContrasena().Verificar("Clave-Nueva-2", hash!, sal!));
        Assert.Equal(1, _bd.Escalar<int>("SELECT COUNT(*) FROM Usuario;"));
    }

    [Theory]
    [Trait("Req", "RNF-05")]
    [InlineData("incorrecta")]
    [InlineData("clave-inicial-1")]
    public void RNF05_ConUnaContrasenaActualIncorrectaNoSeCambiaNada(string actual)
    {
        var servicio = ServicioConSesion();
        var hashAntes = _bd.Escalar<string>("SELECT ContrasenaHash FROM Usuario;");

        var resultado = servicio.CambiarContrasena(actual, "Clave-Nueva-2");

        Assert.False(resultado.Exito);
        Assert.Contains("actual no es correcta", resultado.Mensaje);
        Assert.Equal(hashAntes, _bd.Escalar<string>("SELECT ContrasenaHash FROM Usuario;"));
        Assert.True(CrearServicio().IniciarSesion("villasancarlos", ClaveInicial));
    }

    [Theory]
    [Trait("Req", "RF-17")]
    [InlineData("", "Clave-Nueva-2")]
    [InlineData(ClaveInicial, "")]
    public void RF17_ConCamposVaciosNoSeCambiaNada(string actual, string nueva)
    {
        var servicio = ServicioConSesion();

        var resultado = servicio.CambiarContrasena(actual, nueva);

        Assert.False(resultado.Exito);
        Assert.NotEmpty(resultado.Mensaje);
        Assert.True(CrearServicio().IniciarSesion("villasancarlos", ClaveInicial));
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_LaContrasenaNuevaDebeSerDistintaDeLaActual()
    {
        var servicio = ServicioConSesion();

        var resultado = servicio.CambiarContrasena(ClaveInicial, ClaveInicial);

        Assert.False(resultado.Exito);
        Assert.Contains("distinta", resultado.Mensaje);
    }

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_SinSesionNoSePuedeCambiarLaContrasena()
    {
        var servicio = CrearServicio();
        servicio.CrearUsuarioInicial("villasancarlos", ClaveInicial);

        var resultado = servicio.CambiarContrasena(ClaveInicial, "Clave-Nueva-2");

        Assert.False(resultado.Exito);
        Assert.Contains("iniciar sesión", resultado.Mensaje);
        Assert.True(CrearServicio().IniciarSesion("villasancarlos", ClaveInicial));
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_SePuedeCambiarVariasVecesSeguidas()
    {
        var servicio = ServicioConSesion();

        Assert.True(servicio.CambiarContrasena(ClaveInicial, "Segunda-2").Exito);
        Assert.True(servicio.CambiarContrasena("Segunda-2", "Tercera-3").Exito);
        Assert.False(servicio.CambiarContrasena("Segunda-2", "Cuarta-4").Exito);

        Assert.True(CrearServicio().IniciarSesion("villasancarlos", "Tercera-3"));
    }
}
