using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="CarnetService"/> (RF-12, RF-13, RN-09, RNF-07).</summary>
public class CarnetServiceTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();
    private readonly string _directorio = Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        _bd.Dispose();
        if (Directory.Exists(_directorio))
        {
            Directory.Delete(_directorio, recursive: true);
        }
    }

    private CarnetService CrearServicio()
    {
        var contexto = _bd.CrearContexto();
        return new CarnetService(new MascotaRepository(contexto), new VacunacionRepository(contexto), new GeneradorCarnetPdf());
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_GenerarReuneMascotaPropietarioYTodasLasVacunasEnOrden()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez");
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        var fabio = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        var william = DatosDePrueba.IdDeVeterinario(_bd, "William");
        DatosDePrueba.CrearVacunacion(_bd, rocky.Id, william, "Parvovirus", new DateTime(2026, 6, 1), new DateTime(2027, 6, 1));
        DatosDePrueba.CrearVacunacion(_bd, rocky.Id, fabio, "Rabia", new DateTime(2026, 2, 1));
        var antes = DateTime.Now;

        var resultado = CrearServicio().Generar(rocky.Id);

        Assert.True(resultado.Exito);
        var carnet = resultado.Valor!;
        Assert.Equal("Rocky", carnet.Mascota.Nombre);
        Assert.Equal("Ana Pérez", carnet.Propietario.NombreCompleto);
        Assert.Equal(["Rabia", "Parvovirus"], carnet.Vacunas.Select(v => v.NombreVacuna).ToList());
        Assert.Equal(["Fabio", "William"], carnet.Vacunas.Select(v => v.Veterinario!.NombreCompleto).ToList());
        Assert.InRange(carnet.FechaGeneracion, antes, DateTime.Now);
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_UnaMascotaSinVacunasInformaYNoGeneraCarnet()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);

        var resultado = CrearServicio().Generar(rocky.Id);

        Assert.False(resultado.Exito);
        Assert.Null(resultado.Valor);
        Assert.Contains("no tiene vacunas", resultado.Mensaje);
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_UnaMascotaInexistenteDevuelveError()
    {
        Assert.False(CrearServicio().Generar(999).Exito);
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_ExportarPdfGuardaElArchivoEnLaRutaElegida()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);
        DatosDePrueba.CrearVacunacion(_bd, rocky.Id, DatosDePrueba.IdDeVeterinario(_bd, "Fabio"), "Rabia", new DateTime(2026, 2, 1));
        var servicio = CrearServicio();
        var carnet = servicio.Generar(rocky.Id).Valor!;
        var ruta = Path.Combine(_directorio, "carnet-rocky.pdf");

        var resultado = servicio.ExportarPdf(carnet, ruta);

        Assert.True(resultado.Exito);
        Assert.Equal(ruta, resultado.Valor);
        Assert.True(new FileInfo(ruta).Length > 1024);
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_ExportarPdfDevuelveErrorSiLaRutaNoSePuedeEscribir()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);
        DatosDePrueba.CrearVacunacion(_bd, rocky.Id, DatosDePrueba.IdDeVeterinario(_bd, "Fabio"), "Rabia", new DateTime(2026, 2, 1));
        var servicio = CrearServicio();
        var carnet = servicio.Generar(rocky.Id).Valor!;
        Directory.CreateDirectory(_directorio);
        var archivoComoCarpeta = Path.Combine(_directorio, "no-es-carpeta");
        File.WriteAllText(archivoComoCarpeta, "x");

        var conRutaInvalida = servicio.ExportarPdf(carnet, Path.Combine(archivoComoCarpeta, "carnet.pdf"));
        var sinRuta = servicio.ExportarPdf(carnet, "  ");

        Assert.False(conRutaInvalida.Exito);
        Assert.Contains("No se pudo guardar", conRutaInvalida.Mensaje);
        Assert.False(sinRuta.Exito);
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_NombreArchivoSugeridoUsaLaMascotaYLaFecha()
    {
        var carnet = new VeterinariaDrFabio.Dominio.Modelos.CarnetDigital
        {
            Mascota = new VeterinariaDrFabio.Dominio.Entidades.Mascota { Nombre = "Doña/Luna: 2" },
            FechaGeneracion = new DateTime(2026, 10, 5, 8, 0, 0),
        };

        var nombre = CrearServicio().NombreArchivoSugerido(carnet);

        Assert.Equal("Carnet_DoñaLuna_2_2026-10-05.pdf", nombre);
        Assert.DoesNotContain(Path.GetInvalidFileNameChars(), c => nombre.Contains(c));
    }
}
