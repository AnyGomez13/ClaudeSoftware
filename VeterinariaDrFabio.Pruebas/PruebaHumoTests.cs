using Microsoft.Extensions.DependencyInjection;

namespace VeterinariaDrFabio.Pruebas;

/// <summary>
/// Comprueba que la solución compila y que las referencias entre proyectos funcionan (RNF-02).
/// </summary>
public class PruebaHumoTests
{
    [Fact]
    [Trait("Req", "RNF-02")]
    public void RNF02_LaSolucionCompilaYElContenedorDiResuelveServicios()
    {
        var servicios = new ServiceCollection();
        servicios.AddSingleton<string>("ok");

        using var proveedor = servicios.BuildServiceProvider();

        Assert.Equal("ok", proveedor.GetRequiredService<string>());
    }
}
