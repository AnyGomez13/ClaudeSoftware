using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="ConfiguracionViewModel"/>, la pantalla P-13 (RF-17, RNF-05, CU-14).</summary>
public class ConfiguracionViewModelTests : PantallaTestBase
{
    private ConfiguracionViewModel CrearPantalla() => Servicios.GetRequiredService<ConfiguracionViewModel>();

    private static void LlenarFormulario(ConfiguracionViewModel pantalla, string actual = "actual", string nueva = "nueva", string confirmacion = "nueva")
    {
        pantalla.ContrasenaActual = actual;
        pantalla.ContrasenaNueva = nueva;
        pantalla.Confirmacion = confirmacion;
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_MuestraElUsuarioConLaSesionIniciada()
    {
        Assert.Equal(AutenticacionFalsa.UsuarioValido, CrearPantalla().NombreUsuario);
    }

    [Theory]
    [Trait("Req", "RF-17")]
    [InlineData("", "", "")]
    [InlineData("actual", "", "")]
    [InlineData("actual", "nueva", "")]
    [InlineData("", "nueva", "nueva")]
    [InlineData("actual", "", "nueva")]
    public void RF17_ElBotonSoloSeHabilitaConLosTresCamposEscritos(string actual, string nueva, string confirmacion)
    {
        var pantalla = CrearPantalla();

        LlenarFormulario(pantalla, actual, nueva, confirmacion);

        Assert.False(pantalla.CambiarContrasenaCommand.CanExecute(null));
        pantalla.CambiarContrasenaCommand.Execute(null);
        Assert.Empty(Autenticacion.CambiosDeContrasena);
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_ConLosTresCamposEscritosSeCambiaLaContrasenaYSeLimpiaElFormulario()
    {
        var pantalla = CrearPantalla();
        LlenarFormulario(pantalla);
        Assert.True(pantalla.CambiarContrasenaCommand.CanExecute(null));

        pantalla.CambiarContrasenaCommand.Execute(null);

        Assert.Equal([("actual", "nueva")], Autenticacion.CambiosDeContrasena);
        Assert.True(pantalla.TieneExito);
        Assert.Equal(ConfiguracionViewModel.MensajeContrasenaActualizada, pantalla.MensajeExito);
        Assert.False(pantalla.TieneError);
        Assert.Equal(string.Empty, pantalla.ContrasenaActual);
        Assert.Equal(string.Empty, pantalla.ContrasenaNueva);
        Assert.Equal(string.Empty, pantalla.Confirmacion);
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_SiLaConfirmacionNoCoincideNoSeLlamaAlServicio()
    {
        var pantalla = CrearPantalla();
        LlenarFormulario(pantalla, confirmacion: "otra");

        pantalla.CambiarContrasenaCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Equal(ConfiguracionViewModel.MensajeConfirmacionDistinta, pantalla.MensajeError);
        Assert.Empty(Autenticacion.CambiosDeContrasena);
        Assert.False(pantalla.TieneExito);
        Assert.Equal("nueva", pantalla.ContrasenaNueva);
    }

    [Fact]
    [Trait("Req", "RNF-05")]
    public void RNF05_UnErrorDelServicioSeMuestraYNoSeLimpianLosCampos()
    {
        Autenticacion.ResultadoCambioContrasena = Resultado.Error("La contraseña actual no es correcta.");
        var pantalla = CrearPantalla();
        LlenarFormulario(pantalla);

        pantalla.CambiarContrasenaCommand.Execute(null);

        Assert.Equal("La contraseña actual no es correcta.", pantalla.MensajeError);
        Assert.False(pantalla.TieneExito);
        Assert.Equal("actual", pantalla.ContrasenaActual);
        Assert.Equal("nueva", pantalla.ContrasenaNueva);
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_UnNuevoIntentoLimpiaElMensajeDeExitoAnterior()
    {
        var pantalla = CrearPantalla();
        LlenarFormulario(pantalla);
        pantalla.CambiarContrasenaCommand.Execute(null);
        Assert.True(pantalla.TieneExito);

        LlenarFormulario(pantalla, confirmacion: "otra");
        pantalla.CambiarContrasenaCommand.Execute(null);

        Assert.False(pantalla.TieneExito);
        Assert.True(pantalla.TieneError);
    }

    [Fact]
    [Trait("Req", "RF-17")]
    public void RF17_UnErrorAnteriorSeLimpiaAlCambiarConExito()
    {
        var pantalla = CrearPantalla();
        LlenarFormulario(pantalla, confirmacion: "otra");
        pantalla.CambiarContrasenaCommand.Execute(null);
        Assert.True(pantalla.TieneError);

        pantalla.Confirmacion = "nueva";
        pantalla.CambiarContrasenaCommand.Execute(null);

        Assert.False(pantalla.TieneError);
        Assert.True(pantalla.TieneExito);
    }
}
