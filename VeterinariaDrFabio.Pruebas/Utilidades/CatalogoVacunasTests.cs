using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Pruebas.Utilidades;

/// <summary>Pruebas de <see cref="CatalogoVacunas"/> y <see cref="TextoComparable"/> (RF-11, SUP-10).</summary>
public class CatalogoVacunasTests
{
    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_ElListadoTieneVacunasSinRepetirYConIntervaloPositivo()
    {
        var nombres = CatalogoVacunas.Todas.Select(v => TextoComparable.Normalizar(v.Nombre)).ToList();

        Assert.NotEmpty(nombres);
        Assert.Equal(nombres.Count, nombres.Distinct().Count());
        Assert.All(CatalogoVacunas.Todas, v => Assert.InRange(v.MesesRefuerzo, 1, 36));
        Assert.All(CatalogoVacunas.Todas, v => Assert.NotEmpty(v.Especies));
    }

    [Theory]
    [Trait("Req", "RF-11")]
    [InlineData("Rabia", 12)]
    [InlineData("rabia", 12)]
    [InlineData("  RABIA ", 12)]
    [InlineData("Séxtuple canina", 12)]
    [InlineData("sextuple canina", 12)]
    [InlineData("Bordetella", 6)]
    public void RF11_BuscarEncuentraLaVacunaSinImportarTildesNiMayusculas(string nombre, int mesesEsperados)
    {
        var vacuna = CatalogoVacunas.Buscar(nombre);

        Assert.NotNull(vacuna);
        Assert.Equal(mesesEsperados, vacuna.MesesRefuerzo);
    }

    [Theory]
    [Trait("Req", "SUP-10")]
    [InlineData("Giardia")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SUP10_UnaVacunaFueraDelListadoNoTieneSugerencia(string? nombre)
    {
        Assert.Null(CatalogoVacunas.Buscar(nombre));
        Assert.Null(CatalogoVacunas.CalcularRefuerzo(nombre, new DateTime(2026, 10, 5)));
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_ElRefuerzoSeCalculaSumandoLosMesesDeLaVacunaALaFechaDeAplicacion()
    {
        Assert.Equal(new DateTime(2027, 10, 5), CatalogoVacunas.CalcularRefuerzo("Rabia", new DateTime(2026, 10, 5)));
        Assert.Equal(new DateTime(2027, 4, 5), CatalogoVacunas.CalcularRefuerzo("Bordetella", new DateTime(2026, 10, 5)));
        Assert.Equal(new DateTime(2027, 4, 5), CatalogoVacunas.CalcularRefuerzo("Bordetella", new DateTime(2026, 10, 5, 15, 30, 0)));
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_ElRefuerzoRespetaElFinDeMesYLosAniosBisiestos()
    {
        Assert.Equal(new DateTime(2027, 2, 28), CatalogoVacunas.CalcularRefuerzo("Bordetella", new DateTime(2026, 8, 31)));
        Assert.Equal(new DateTime(2025, 2, 28), CatalogoVacunas.CalcularRefuerzo("Rabia", new DateTime(2024, 2, 29)));
    }

    [Theory]
    [Trait("Req", "RF-11")]
    [InlineData("Perro", "Rabia", true)]
    [InlineData("Perro", "Parvovirus", true)]
    [InlineData("Perro", "Triple felina", false)]
    [InlineData("gato", "Triple felina", true)]
    [InlineData("Gato", "Rabia", true)]
    [InlineData("Gato", "Parvovirus", false)]
    public void RF11_LaListaSeFiltraSegunLaEspecieDeLaMascota(string especie, string vacuna, bool debeEstar)
    {
        var nombres = CatalogoVacunas.ParaEspecie(especie).Select(v => v.Nombre).ToList();

        Assert.Equal(debeEstar, nombres.Contains(vacuna));
    }

    [Theory]
    [Trait("Req", "RF-11")]
    [InlineData("Ave")]
    [InlineData("")]
    [InlineData(null)]
    public void RF11_ConUnaEspecieSinListadoSeOfrecenTodasLasVacunas(string? especie)
    {
        Assert.Equal(CatalogoVacunas.Todas.Count, CatalogoVacunas.ParaEspecie(especie).Count);
    }

    [Theory]
    [Trait("Req", "RF-11")]
    [InlineData("Desparasitación", "desparasitacion")]
    [InlineData("  ÁÉÍÓÚ Ñandú ", "aeiou nandu")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void RF11_NormalizarQuitaTildesMayusculasYEspaciosDeLosExtremos(string? texto, string esperado)
    {
        Assert.Equal(esperado, TextoComparable.Normalizar(texto));
    }
}
