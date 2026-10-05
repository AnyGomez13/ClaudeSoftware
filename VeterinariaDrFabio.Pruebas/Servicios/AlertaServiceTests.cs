using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="AlertaService"/> (RF-14, RN-11, SUP-09).</summary>
public class AlertaServiceTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();
    private readonly int _fabioId;
    private readonly Mascota _rocky;
    private static readonly DateTime Hoy = DateTime.Today;

    public AlertaServiceTests()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");
        _rocky = DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        _fabioId = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
    }

    public void Dispose() => _bd.Dispose();

    private AlertaService CrearServicio()
    {
        var contexto = _bd.CrearContexto();
        return new AlertaService(
            new ProcedimientoRepository(contexto), new VacunacionRepository(contexto), new RecordatorioRepository(contexto));
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_ListaLasProximasEnLosSiguientes30DiasYLasVencidasOrdenadasPorFecha()
    {
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Parvovirus", Hoy.AddDays(-200), Hoy.AddDays(30));
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Rabia", Hoy.AddDays(-300), Hoy.AddDays(-5));
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Moquillo", Hoy.AddDays(-100), Hoy.AddDays(10));

        var alertas = CrearServicio().ProximasFechas();

        Assert.Equal(["Rabia", "Moquillo", "Parvovirus"], alertas.Select(a => a.Detalle).ToList());
        Assert.Equal([true, false, false], alertas.Select(a => a.Vencida).ToList());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_NoListaLasFechasLejanasNiLosRegistrosSinProximaFecha()
    {
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Rabia", Hoy.AddDays(-30), Hoy.AddDays(31));
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Moquillo", Hoy.AddDays(-30));

        Assert.Empty(CrearServicio().ProximasFechas());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_LaAlertaTraeMascotaPropietarioTipoYFecha()
    {
        var vacunacion = DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Rabia", Hoy.AddDays(-300), Hoy.AddDays(7));

        var alerta = Assert.Single(CrearServicio().ProximasFechas());

        Assert.Equal(_rocky.Id, alerta.MascotaId);
        Assert.Equal("Rocky", alerta.MascotaNombre);
        Assert.Equal("Ana Pérez", alerta.PropietarioNombre);
        Assert.Equal("3001234567", alerta.PropietarioTelefono);
        Assert.Equal("Vacunacion", alerta.Tipo);
        Assert.Equal(Hoy.AddDays(7), alerta.FechaObjetivo);
        Assert.Equal(vacunacion.Id, alerta.VacunacionId);
        Assert.Null(alerta.ProcedimientoId);
    }

    [Theory]
    [Trait("Req", "SUP-09")]
    [InlineData("Desparasitación")]
    [InlineData("desparasitacion")]
    [InlineData("  DESPARASITACIÓN ")]
    public void SUP09_SoloLosProcedimientosDeDesparasitacionGeneranAlerta(string tipo)
    {
        var desparasitacion = DatosDePrueba.CrearProcedimiento(_bd, _rocky.Id, _fabioId, tipo, Hoy.AddDays(-80), Hoy.AddDays(10));
        DatosDePrueba.CrearProcedimiento(_bd, _rocky.Id, _fabioId, "Control", Hoy.AddDays(-20), Hoy.AddDays(5));
        DatosDePrueba.CrearProcedimiento(_bd, _rocky.Id, _fabioId, "Cirugía", Hoy.AddDays(-20), Hoy.AddDays(6));

        var alerta = Assert.Single(CrearServicio().ProximasFechas());

        Assert.Equal("Desparasitacion", alerta.Tipo);
        Assert.Equal(desparasitacion.Id, alerta.ProcedimientoId);
        Assert.Null(alerta.VacunacionId);
        Assert.Equal(Hoy.AddDays(10), alerta.FechaObjetivo);
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_UnaMascotaInactivaNoGeneraAlertas()
    {
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Rabia", Hoy.AddDays(-300), Hoy.AddDays(7));
        _bd.Ejecutar("UPDATE Mascota SET Activo = 0;");

        Assert.Empty(CrearServicio().ProximasFechas());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_UnaAplicacionPosteriorDeLaMismaVacunaDejaSinEfectoLaAlertaAnterior()
    {
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Rabia", Hoy.AddDays(-400), Hoy.AddDays(-35));
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "RABIA", Hoy.AddDays(-30), Hoy.AddDays(335));
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Moquillo", Hoy.AddDays(-400), Hoy.AddDays(-20));

        var alertas = CrearServicio().ProximasFechas();

        Assert.Equal(["Moquillo"], alertas.Select(a => a.Detalle).ToList());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_UnaDesparasitacionPosteriorDejaSinEfectoLaAnterior()
    {
        DatosDePrueba.CrearProcedimiento(_bd, _rocky.Id, _fabioId, "Desparasitación", Hoy.AddDays(-120), Hoy.AddDays(-30));
        DatosDePrueba.CrearProcedimiento(_bd, _rocky.Id, _fabioId, "Desparasitación", Hoy.AddDays(-25), Hoy.AddDays(65));

        Assert.Empty(CrearServicio().ProximasFechas());
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_UnaAlertaConRecordatorioEnviadoYaNoSeMuestra()
    {
        var rabia = DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Rabia", Hoy.AddDays(-300), Hoy.AddDays(7));
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Moquillo", Hoy.AddDays(-300), Hoy.AddDays(8));
        using (var contexto = _bd.CrearContexto())
        {
            new RecordatorioRepository(contexto).Agregar(new Recordatorio
            {
                MascotaId = _rocky.Id,
                Tipo = "Vacunacion",
                FechaObjetivo = Hoy.AddDays(7),
                Mensaje = "m",
                Enlace = "e",
                Estado = "Enviado",
                FechaEnvio = DateTime.Now,
                VacunacionId = rabia.Id,
            });
        }

        var alertas = CrearServicio().ProximasFechas();

        Assert.Equal(["Moquillo"], alertas.Select(a => a.Detalle).ToList());
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_UnaAlertaConRecordatorioSoloPendienteSigueVisible()
    {
        var rabia = DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Rabia", Hoy.AddDays(-300), Hoy.AddDays(7));
        using (var contexto = _bd.CrearContexto())
        {
            new RecordatorioRepository(contexto).Agregar(new Recordatorio
            {
                MascotaId = _rocky.Id,
                Tipo = "Vacunacion",
                FechaObjetivo = Hoy.AddDays(7),
                Mensaje = "m",
                Enlace = "e",
                VacunacionId = rabia.Id,
            });
        }

        Assert.Single(CrearServicio().ProximasFechas());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_SinRegistrosLaListaEstaVacia()
    {
        Assert.Empty(CrearServicio().ProximasFechas());
    }
}
