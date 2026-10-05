using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="VacunacionService"/> (RF-11, RN-07, RN-08, RNF-08).</summary>
public class VacunacionServiceTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();
    private readonly int _mascotaId;
    private readonly int _fabioId;
    private readonly int _williamId;

    public VacunacionServiceTests()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        _mascotaId = DatosDePrueba.CrearMascota(_bd, ana.Id).Id;
        _fabioId = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        _williamId = DatosDePrueba.IdDeVeterinario(_bd, "William");
    }

    public void Dispose() => _bd.Dispose();

    private VacunacionService CrearServicio()
    {
        var contexto = _bd.CrearContexto();
        return new VacunacionService(
            new VacunacionRepository(contexto),
            new MascotaRepository(contexto),
            new VeterinarioRepository(contexto));
    }

    private int ContarVacunaciones() => _bd.Escalar<int>("SELECT COUNT(*) FROM Vacunacion;");

    private Vacunacion Nueva() => new()
    {
        MascotaId = _mascotaId,
        VeterinarioId = _fabioId,
        NombreVacuna = "Rabia",
        FechaAplicacion = new DateTime(2026, 10, 4),
    };

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_RegistrarGuardaLaVacunacionConSuVeterinarioYElRefuerzo()
    {
        var vacunacion = Nueva();
        vacunacion.VeterinarioId = _williamId;
        vacunacion.NombreVacuna = "  Parvovirus ";
        vacunacion.ProximaFecha = new DateTime(2027, 10, 4);
        vacunacion.Lote = " L-123 ";
        vacunacion.Observaciones = "";

        var resultado = CrearServicio().Registrar(vacunacion);

        Assert.True(resultado.Exito);
        using var lectura = _bd.CrearContexto();
        var guardada = lectura.Vacunaciones.Single();
        Assert.Equal(_williamId, guardada.VeterinarioId);
        Assert.Equal("Parvovirus", guardada.NombreVacuna);
        Assert.Equal(new DateTime(2027, 10, 4), guardada.ProximaFecha);
        Assert.Equal("L-123", guardada.Lote);
        Assert.Null(guardada.Observaciones);
    }

    [Theory]
    [Trait("Req", "RN-07")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void RN07_SinVeterinarioAsignadoNoSeGuarda(int veterinarioId)
    {
        var vacunacion = Nueva();
        vacunacion.VeterinarioId = veterinarioId;

        var resultado = CrearServicio().Registrar(vacunacion);

        Assert.False(resultado.Exito);
        Assert.Contains("veterinario", resultado.Mensaje);
        Assert.Equal(0, ContarVacunaciones());
    }

    [Fact]
    [Trait("Req", "RN-07")]
    public void RN07_UnVeterinarioInactivoNoPuedeSerAsignado()
    {
        _bd.Ejecutar("UPDATE Veterinario SET Activo = 0 WHERE NombreCompleto = 'William';");
        var vacunacion = Nueva();
        vacunacion.VeterinarioId = _williamId;

        Assert.False(CrearServicio().Registrar(vacunacion).Exito);
        Assert.Equal(0, ContarVacunaciones());
    }

    [Theory]
    [Trait("Req", "RNF-08")]
    [InlineData(0)]
    [InlineData(999)]
    public void RNF08_SinMascotaExistenteNoSeGuarda(int mascotaId)
    {
        var vacunacion = Nueva();
        vacunacion.MascotaId = mascotaId;

        Assert.False(CrearServicio().Registrar(vacunacion).Exito);
        Assert.Equal(0, ContarVacunaciones());
    }

    [Theory]
    [Trait("Req", "RF-11")]
    [InlineData("")]
    [InlineData("   ")]
    public void RF11_ElNombreDeLaVacunaEsObligatorio(string nombre)
    {
        var vacunacion = Nueva();
        vacunacion.NombreVacuna = nombre;

        Assert.False(CrearServicio().Registrar(vacunacion).Exito);
        Assert.Equal(0, ContarVacunaciones());
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_LaFechaDeAplicacionEsObligatoriaYNoPuedeSerFutura()
    {
        var sinFecha = Nueva();
        sinFecha.FechaAplicacion = default;
        var futura = Nueva();
        futura.FechaAplicacion = DateTime.Today.AddDays(1);

        Assert.False(CrearServicio().Registrar(sinFecha).Exito);
        Assert.False(CrearServicio().Registrar(futura).Exito);
        Assert.Equal(0, ContarVacunaciones());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_ElRefuerzoNoPuedeSerAnteriorALaAplicacion()
    {
        var vacunacion = Nueva();
        vacunacion.ProximaFecha = vacunacion.FechaAplicacion.AddDays(-1);

        Assert.False(CrearServicio().Registrar(vacunacion).Exito);
        Assert.Equal(0, ContarVacunaciones());
    }
}
