using VeterinariaDrFabio.App.ViewModels;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="LoginViewModel"/> (RF-01, RNF-05, CU-01, P-01).</summary>
public class LoginViewModelTests
{
    private readonly AutenticacionFalsa _autenticacion = new();
    private readonly LoginViewModel _modelo;

    public LoginViewModelTests()
    {
        _modelo = new LoginViewModel(_autenticacion);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ConCredencialesValidasIniciaSesionYAvisaALaVista()
    {
        var avisos = 0;
        _modelo.SesionIniciada += () => avisos++;
        _modelo.Usuario = AutenticacionFalsa.UsuarioValido;
        _modelo.Contrasena = AutenticacionFalsa.ClaveValida;

        _modelo.IniciarSesionCommand.Execute(null);

        Assert.True(_autenticacion.SesionActiva);
        Assert.Equal(1, avisos);
        Assert.False(_modelo.TieneError);
        Assert.Equal(string.Empty, _modelo.Contrasena);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ConCredencialesInvalidasMuestraElErrorYVaciaLaContrasena()
    {
        var avisos = 0;
        _modelo.SesionIniciada += () => avisos++;
        _modelo.Usuario = AutenticacionFalsa.UsuarioValido;
        _modelo.Contrasena = "incorrecta";

        _modelo.IniciarSesionCommand.Execute(null);

        Assert.False(_autenticacion.SesionActiva);
        Assert.Equal(0, avisos);
        Assert.True(_modelo.TieneError);
        Assert.Equal("Credenciales inválidas", _modelo.MensajeError);
        Assert.Equal(string.Empty, _modelo.Contrasena);
        Assert.Equal(AutenticacionFalsa.UsuarioValido, _modelo.Usuario);
    }

    [Theory]
    [Trait("Req", "RF-01")]
    [InlineData("", "")]
    [InlineData("fabio", "")]
    [InlineData("", "clave")]
    [InlineData("   ", "clave")]
    public void RF01_SinUsuarioOSinContrasenaElBotonIngresarNoEstaHabilitado(string usuario, string contrasena)
    {
        _modelo.Usuario = usuario;
        _modelo.Contrasena = contrasena;

        Assert.False(_modelo.IniciarSesionCommand.CanExecute(null));
        _modelo.IniciarSesionCommand.Execute(null);
        Assert.Equal(0, _autenticacion.IntentosDeSesion);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ConUsuarioYContrasenaElBotonIngresarSeHabilita()
    {
        _modelo.Usuario = "fabio";
        _modelo.Contrasena = "x";

        Assert.True(_modelo.IniciarSesionCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_UnIntentoExitosoDespuesDeUnoFallidoLimpiaElError()
    {
        _modelo.Usuario = AutenticacionFalsa.UsuarioValido;
        _modelo.Contrasena = "incorrecta";
        _modelo.IniciarSesionCommand.Execute(null);
        Assert.True(_modelo.TieneError);

        _modelo.Contrasena = AutenticacionFalsa.ClaveValida;
        _modelo.IniciarSesionCommand.Execute(null);

        Assert.False(_modelo.TieneError);
        Assert.Equal(string.Empty, _modelo.MensajeError);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ReiniciarVaciaElFormulario()
    {
        _modelo.Usuario = "fabio";
        _modelo.Contrasena = "x";
        _modelo.IniciarSesionCommand.Execute(null);

        _modelo.Reiniciar();

        Assert.Equal(string.Empty, _modelo.Usuario);
        Assert.Equal(string.Empty, _modelo.Contrasena);
        Assert.False(_modelo.TieneError);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_LaVistaSeEntera_DeLosCambiosDeLasPropiedades()
    {
        var cambios = new List<string?>();
        _modelo.PropertyChanged += (_, e) => cambios.Add(e.PropertyName);

        _modelo.Usuario = "fabio";
        _modelo.Contrasena = "incorrecta";
        _modelo.IniciarSesionCommand.Execute(null);

        Assert.Contains(nameof(LoginViewModel.Usuario), cambios);
        Assert.Contains(nameof(LoginViewModel.MensajeError), cambios);
        Assert.Contains(nameof(LoginViewModel.TieneError), cambios);
    }
}
