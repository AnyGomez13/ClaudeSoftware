using System.Text;
using QuestPDF.Fluent;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Dominio.Modelos;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Pruebas.Utilidades;

/// <summary>Pruebas de <see cref="GeneradorCarnetPdf"/> (RF-13, RN-09, RNF-06, SUP-D08).</summary>
public class GeneradorCarnetPdfTests : IDisposable
{
    private readonly string _directorio = Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas", Guid.NewGuid().ToString("N"));
    private readonly GeneradorCarnetPdf _generador = new();

    public void Dispose()
    {
        if (Directory.Exists(_directorio))
        {
            Directory.Delete(_directorio, recursive: true);
        }
    }

    private static CarnetDigital CarnetDeEjemplo(int vacunas = 3)
    {
        var fabio = new Veterinario { Id = 1, NombreCompleto = "Fabio" };
        var william = new Veterinario { Id = 2, NombreCompleto = "William" };
        var carnet = new CarnetDigital
        {
            Mascota = new Mascota
            {
                Id = 1,
                Nombre = "Rocky",
                Especie = "Perro",
                Raza = "Criollo",
                Sexo = "Macho",
                FechaNacimiento = new DateTime(2020, 3, 15),
                FechaNacimientoEstimada = true,
                Peso = 12.5,
            },
            Propietario = new Propietario { Id = 1, NombreCompleto = "Ana Pérez", Telefono = "3001234567" },
            FechaGeneracion = new DateTime(2026, 10, 5, 10, 30, 0),
        };
        for (var i = 0; i < vacunas; i++)
        {
            carnet.Vacunas.Add(new Vacunacion
            {
                Id = i + 1,
                NombreVacuna = i % 2 == 0 ? "Rabia" : "Parvovirus canino",
                FechaAplicacion = new DateTime(2026, 1, 10).AddMonths(i),
                ProximaFecha = i == 1 ? null : new DateTime(2027, 1, 10).AddMonths(i),
                Lote = i == 0 ? "L-2026-01" : null,
                Veterinario = i % 2 == 0 ? fabio : william,
            });
        }

        return carnet;
    }

    private static string PrimerasYUltimasLetras(string ruta, out string final)
    {
        var bytes = File.ReadAllBytes(ruta);
        final = Encoding.ASCII.GetString(bytes[^32..]);
        return Encoding.ASCII.GetString(bytes[..8]);
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_GeneraUnArchivoPdfValidoConElCarnet()
    {
        var ruta = Path.Combine(_directorio, "carnet.pdf");

        var resultado = _generador.Generar(CarnetDeEjemplo(), ruta);

        Assert.Equal(ruta, resultado);
        Assert.True(new FileInfo(ruta).Length > 1024);
        var inicio = PrimerasYUltimasLetras(ruta, out var final);
        Assert.StartsWith("%PDF-", inicio);
        Assert.Contains("%%EOF", final);
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_CreaLasCarpetasQueFaltenYReemplazaElArchivoExistente()
    {
        var ruta = Path.Combine(_directorio, "nueva", "subcarpeta", "carnet.pdf");

        _generador.Generar(CarnetDeEjemplo(1), ruta);
        var tamanoConUnaVacuna = new FileInfo(ruta).Length;
        _generador.Generar(CarnetDeEjemplo(25), ruta);

        Assert.True(File.Exists(ruta));
        Assert.True(new FileInfo(ruta).Length > tamanoConUnaVacuna);
    }

    [Fact]
    [Trait("Req", "RN-09")]
    public void RN09_UnCarnetSinVacunasOSinRutaSeRechaza()
    {
        Assert.Throws<ArgumentException>(() => _generador.Generar(CarnetDeEjemplo(0), Path.Combine(_directorio, "x.pdf")));
        Assert.Throws<ArgumentException>(() => _generador.Generar(CarnetDeEjemplo(), "  "));
        Assert.False(File.Exists(Path.Combine(_directorio, "x.pdf")));
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_GeneraUnaMuestraParaInspeccionVisual()
    {
        var muestra = Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas", "muestra-carnet.pdf");

        _generador.Generar(CarnetDeEjemplo(), muestra);

        // La misma maquetación se dibuja como imagen para poder revisar el diseño sin un visor de PDF.
        var paginas = GeneradorCarnetPdf.ConstruirDocumento(CarnetDeEjemplo()).GenerateImages().ToList();
        File.WriteAllBytes(Path.ChangeExtension(muestra, ".png"), paginas[0]);

        Assert.True(File.Exists(muestra));
        Assert.NotEmpty(paginas);
    }
}
