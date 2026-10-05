using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Pruebas.Utilidades;

/// <summary>Pruebas de <see cref="GeneradorEnlaceWhatsApp"/> (RF-15, RN-10, RN-14, SUP-01, SUP-D07).</summary>
public class GeneradorEnlaceWhatsAppTests
{
    private readonly GeneradorEnlaceWhatsApp _generador = new();

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_ConstruyeElEnlaceWaMeConPrefijo57()
    {
        var enlace = _generador.Construir("3001234567", "Hola");

        Assert.Equal("https://wa.me/573001234567?text=Hola", enlace);
    }

    [Fact]
    [Trait("Req", "RN-10")]
    public void RN10_CodificaEspaciosTildesSaltosDeLineaYSimbolosDelMensaje()
    {
        const string mensaje = "Hola Ana, la vacuna de Rocky vence el 05/10/2026.\nSaludos & gracias";

        var enlace = _generador.Construir("3001234567", mensaje);
        var texto = enlace["https://wa.me/573001234567?text=".Length..];

        Assert.Equal(mensaje, Uri.UnescapeDataString(texto));
        Assert.DoesNotContain(' ', texto);
        Assert.DoesNotContain('\n', texto);
        Assert.DoesNotContain('&', texto);
        Assert.Contains("%0A", texto);
    }

    [Fact]
    [Trait("Req", "RN-10")]
    public void RN10_ConservaLasTildesComoUtf8Codificado()
    {
        var enlace = _generador.Construir("3001234567", "Desparasitación");

        Assert.EndsWith("Desparasitaci%C3%B3n", enlace);
    }

    [Theory]
    [Trait("Req", "RN-14")]
    [InlineData("2001234567")]
    [InlineData("300123456")]
    [InlineData("30012345678")]
    [InlineData("30012345ab")]
    [InlineData("573001234567")]
    [InlineData("+573001234567")]
    [InlineData("3001234567\n")]
    [InlineData("")]
    [InlineData(null)]
    public void RN14_RechazaTelefonosQueNoSonCelularColombiano(string? telefono)
    {
        Assert.Throws<ArgumentException>(() => _generador.Construir(telefono!, "Hola"));
    }

    [Theory]
    [Trait("Req", "RF-15")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RF15_RechazaUnMensajeVacio(string? mensaje)
    {
        Assert.Throws<ArgumentException>(() => _generador.Construir("3001234567", mensaje!));
    }
}
