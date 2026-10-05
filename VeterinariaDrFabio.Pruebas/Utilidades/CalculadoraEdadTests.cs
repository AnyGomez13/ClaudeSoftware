using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Pruebas.Utilidades;

/// <summary>Pruebas de <see cref="CalculadoraEdad"/> (RF-08, RN-05).</summary>
public class CalculadoraEdadTests
{
    private readonly CalculadoraEdad _calculadora = new();

    [Theory]
    [Trait("Req", "RF-08")]
    [InlineData("2024-10-05", "2026-10-05", 24)] // cumpleaños hoy
    [InlineData("2024-10-05", "2026-10-04", 23)] // un día antes del cumpleaños
    [InlineData("2026-10-01", "2026-10-05", 0)] // menor de un mes
    [InlineData("2026-09-05", "2026-10-05", 1)] // exactamente un mes
    [InlineData("2026-09-06", "2026-10-05", 0)] // un día menos de un mes
    [InlineData("2020-02-29", "2021-02-28", 12)] // nacido el 29 de febrero: cumple el último día de febrero
    [InlineData("2020-02-29", "2021-02-27", 11)]
    [InlineData("2020-02-29", "2024-02-29", 48)]
    [InlineData("2026-01-31", "2026-02-28", 1)] // 31 de enero: el mes se cumple el 28 de febrero
    [InlineData("2026-10-05", "2026-10-05", 0)] // nace hoy
    public void RF08_EnMesesCuentaLosMesesCumplidos(string nacimiento, string hoy, int esperado)
    {
        Assert.Equal(esperado, _calculadora.EnMeses(DateTime.Parse(nacimiento), DateTime.Parse(hoy)));
    }

    [Theory]
    [Trait("Req", "RF-08")]
    [InlineData("2026-10-01", "2026-10-05", "Menos de 1 mes")]
    [InlineData("2026-09-05", "2026-10-05", "1 mes")]
    [InlineData("2026-05-05", "2026-10-05", "5 meses")]
    [InlineData("2025-10-05", "2026-10-05", "1 año")]
    [InlineData("2025-09-05", "2026-10-05", "1 año 1 mes")]
    [InlineData("2024-10-05", "2026-10-05", "2 años")]
    [InlineData("2024-10-05", "2026-10-04", "1 año 11 meses")]
    [InlineData("2023-07-05", "2026-10-05", "3 años 3 meses")]
    public void RF08_LegibleExpresaLaEdadEnTexto(string nacimiento, string hoy, string esperado)
    {
        Assert.Equal(esperado, _calculadora.Legible(DateTime.Parse(nacimiento), DateTime.Parse(hoy)));
    }

    [Fact]
    [Trait("Req", "RN-05")]
    public void RN05_LaEdadSeCalculaConLaFechaActualSinGuardarse()
    {
        var haceUnAnio = DateTime.Today.AddYears(-1);

        Assert.Equal(12, _calculadora.EnMeses(haceUnAnio));
        Assert.Equal("1 año", _calculadora.Legible(haceUnAnio));
    }

    [Fact]
    [Trait("Req", "RF-08")]
    public void RF08_UnaFechaDeNacimientoFuturaSeRechaza()
    {
        var manana = DateTime.Today.AddDays(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculadora.EnMeses(manana));
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculadora.Legible(manana));
    }
}
