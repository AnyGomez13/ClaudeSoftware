using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>Pruebas de <see cref="ProcedimientoRepository"/> (RF-09, RF-10, RF-14).</summary>
public class ProcedimientoRepositoryTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private void Agregar(int mascotaId, int veterinarioId, DateTime fecha, string tipo, DateTime? proxima = null)
    {
        using var contexto = _bd.CrearContexto();
        new ProcedimientoRepository(contexto).Agregar(new Procedimiento
        {
            MascotaId = mascotaId,
            VeterinarioId = veterinarioId,
            Fecha = fecha,
            TipoProcedimiento = tipo,
            Descripcion = "Descripción",
            ProximaFechaRecomendada = proxima,
        });
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_ListarPorMascotaDevuelveSoloLaSuyaEnOrdenCronologicoConVeterinario()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        var misu = DatosDePrueba.CrearMascota(_bd, ana.Id, "Misu");
        var fabio = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        var william = DatosDePrueba.IdDeVeterinario(_bd, "William");
        Agregar(rocky.Id, william, new DateTime(2026, 8, 10), "Control");
        Agregar(rocky.Id, fabio, new DateTime(2026, 2, 1), "Consulta");
        Agregar(misu.Id, fabio, new DateTime(2026, 5, 5), "Cirugía");

        using var contexto = _bd.CrearContexto();
        var lista = new ProcedimientoRepository(contexto).ListarPorMascota(rocky.Id);

        Assert.Equal(["Consulta", "Control"], lista.Select(p => p.TipoProcedimiento).ToList());
        Assert.Equal(["Fabio", "William"], lista.Select(p => p.Veterinario!.NombreCompleto).ToList());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_ListarConProximaFechaTraeSoloLosQueTienenFechaOrdenadosConMascotaYPropietario()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);
        var fabio = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        Agregar(rocky.Id, fabio, new DateTime(2026, 1, 1), "Consulta");
        Agregar(rocky.Id, fabio, new DateTime(2026, 2, 1), "Desparasitación", new DateTime(2026, 12, 1));
        Agregar(rocky.Id, fabio, new DateTime(2026, 3, 1), "Desparasitación", new DateTime(2026, 11, 1));

        using var contexto = _bd.CrearContexto();
        var lista = new ProcedimientoRepository(contexto).ListarConProximaFecha();

        Assert.Equal(
            [new DateTime(2026, 11, 1), new DateTime(2026, 12, 1)],
            lista.Select(p => p.ProximaFechaRecomendada!.Value).ToList());
        Assert.All(lista, p => Assert.Equal("Ana Pérez", p.Mascota!.Propietario!.NombreCompleto));
    }
}
