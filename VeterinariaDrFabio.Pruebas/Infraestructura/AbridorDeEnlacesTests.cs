using VeterinariaDrFabio.App.Infraestructura;

namespace VeterinariaDrFabio.Pruebas.Infraestructura;

/// <summary>Pruebas de <see cref="AbridorDeEnlaces"/>: solo debe abrir direcciones web (RF-15, SUP-01).</summary>
public class AbridorDeEnlacesTests
{
    [Theory]
    [Trait("Req", "RF-15")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("calc.exe")]
    [InlineData("C:\\Windows\\System32\\calc.exe")]
    [InlineData("file:///C:/Windows/System32/calc.exe")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://ejemplo.com/archivo")]
    [InlineData("mailto:alguien@ejemplo.com")]
    public void RF15_RechazaCualquierCosaQueNoSeaUnaDireccionWeb(string direccion)
    {
        var abridor = new AbridorDeEnlaces();

        Assert.Throws<ArgumentException>(() => abridor.Abrir(direccion));
    }
}
