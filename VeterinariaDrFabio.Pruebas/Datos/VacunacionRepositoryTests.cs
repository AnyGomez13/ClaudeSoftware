using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>Pruebas de <see cref="VacunacionRepository"/> (RF-11, RF-12, RF-14).</summary>
public class VacunacionRepositoryTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private void Agregar(int mascotaId, int veterinarioId, DateTime fecha, string vacuna, DateTime? proxima = null)
    {
        using var contexto = _bd.CrearContexto();
        new VacunacionRepository(contexto).Agregar(new Vacunacion
        {
            MascotaId = mascotaId,
            VeterinarioId = veterinarioId,
            NombreVacuna = vacuna,
            FechaAplicacion = fecha,
            ProximaFecha = proxima,
        });
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_ListarPorMascotaDevuelveSoloLaSuyaEnOrdenCronologicoConVeterinario()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        var misu = DatosDePrueba.CrearMascota(_bd, ana.Id, "Misu");
        var fabio = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        var william = DatosDePrueba.IdDeVeterinario(_bd, "William");
        Agregar(rocky.Id, william, new DateTime(2026, 9, 1), "Parvovirus");
        Agregar(rocky.Id, fabio, new DateTime(2026, 3, 1), "Rabia");
        Agregar(misu.Id, fabio, new DateTime(2026, 4, 1), "Triple felina");

        using var contexto = _bd.CrearContexto();
        var lista = new VacunacionRepository(contexto).ListarPorMascota(rocky.Id);

        Assert.Equal(["Rabia", "Parvovirus"], lista.Select(v => v.NombreVacuna).ToList());
        Assert.Equal(["Fabio", "William"], lista.Select(v => v.Veterinario!.NombreCompleto).ToList());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_ListarConProximaFechaTraeSoloLosQueTienenRefuerzoOrdenadosConMascotaYPropietario()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);
        var fabio = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        Agregar(rocky.Id, fabio, new DateTime(2026, 1, 1), "Moquillo");
        Agregar(rocky.Id, fabio, new DateTime(2026, 2, 1), "Rabia", new DateTime(2027, 2, 1));
        Agregar(rocky.Id, fabio, new DateTime(2026, 3, 1), "Parvovirus", new DateTime(2026, 12, 1));

        using var contexto = _bd.CrearContexto();
        var lista = new VacunacionRepository(contexto).ListarConProximaFecha();

        Assert.Equal(["Parvovirus", "Rabia"], lista.Select(v => v.NombreVacuna).ToList());
        Assert.All(lista, v => Assert.Equal("Ana Pérez", v.Mascota!.Propietario!.NombreCompleto));
    }
}
