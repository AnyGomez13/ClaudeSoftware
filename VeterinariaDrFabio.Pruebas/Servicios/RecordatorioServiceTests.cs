using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Dominio.Modelos;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="RecordatorioService"/> (RF-15, RN-10, RN-14, RN-15, RNF-04, RNF-06, SUP-D06).</summary>
public class RecordatorioServiceTests : IDisposable
{
    private static readonly DateTime Hoy = DateTime.Today;
    private readonly BaseDatosTemporal _bd = new();
    private readonly int _fabioId;
    private readonly Mascota _rocky;

    public RecordatorioServiceTests()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");
        _rocky = DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        _fabioId = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
    }

    public void Dispose() => _bd.Dispose();

    private sealed class ConectividadFalsa(bool hayInternet) : IConectividad
    {
        public bool HayInternet() => hayInternet;
    }

    private RecordatorioService CrearServicio(bool hayInternet = true) =>
        new(new RecordatorioRepository(_bd.CrearContexto()), new GeneradorEnlaceWhatsApp(), new ConectividadFalsa(hayInternet));

    private AlertaService CrearAlertas()
    {
        var contexto = _bd.CrearContexto();
        return new AlertaService(
            new ProcedimientoRepository(contexto), new VacunacionRepository(contexto), new RecordatorioRepository(contexto));
    }

    private AlertaProximaFecha AlertaDeVacuna(DateTime? proxima = null)
    {
        DatosDePrueba.CrearVacunacion(_bd, _rocky.Id, _fabioId, "Rabia", Hoy.AddDays(-300), proxima ?? Hoy.AddDays(7));
        return Assert.Single(CrearAlertas().ProximasFechas());
    }

    private int ContarRecordatorios() => _bd.Escalar<int>("SELECT COUNT(*) FROM Recordatorio;");

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_PrepararGuardaUnRecordatorioPendienteConEnlaceWaMeYMensaje()
    {
        var alerta = AlertaDeVacuna(Hoy.AddDays(7));

        var resultado = CrearServicio().Preparar(alerta);

        Assert.True(resultado.Exito);
        var recordatorio = resultado.Valor!;
        Assert.True(recordatorio.Id > 0);
        Assert.Equal("Pendiente", recordatorio.Estado);
        Assert.Equal("Vacunacion", recordatorio.Tipo);
        Assert.Equal(alerta.VacunacionId, recordatorio.VacunacionId);
        Assert.Null(recordatorio.ProcedimientoId);
        Assert.StartsWith("https://wa.me/573001234567?text=", recordatorio.Enlace);
        Assert.Contains("Ana Pérez", recordatorio.Mensaje);
        Assert.Contains("Rocky", recordatorio.Mensaje);
        Assert.Contains("vacuna Rabia", recordatorio.Mensaje);
        Assert.Contains(alerta.FechaObjetivo.ToString("dd/MM/yyyy"), recordatorio.Mensaje);
        Assert.Equal(1, ContarRecordatorios());
    }

    [Fact]
    [Trait("Req", "RN-10")]
    public void RN10_ElEnlaceCodificaElMensajeYNoUsaLaApiDePago()
    {
        var recordatorio = CrearServicio().Preparar(AlertaDeVacuna()).Valor!;

        var texto = recordatorio.Enlace["https://wa.me/573001234567?text=".Length..];

        Assert.Equal(recordatorio.Mensaje, Uri.UnescapeDataString(texto));
        Assert.StartsWith("https://wa.me/", recordatorio.Enlace);
        Assert.DoesNotContain("api.whatsapp.com", recordatorio.Enlace);
    }

    [Fact]
    [Trait("Req", "SUP-09")]
    public void SUP09_UnaDesparasitacionSeGuardaConOrigenEnElProcedimiento()
    {
        var procedimiento = DatosDePrueba.CrearProcedimiento(_bd, _rocky.Id, _fabioId, "Desparasitación", Hoy.AddDays(-80), Hoy.AddDays(5));
        var alerta = Assert.Single(CrearAlertas().ProximasFechas());

        var recordatorio = CrearServicio().Preparar(alerta).Valor!;

        Assert.Equal("Desparasitacion", recordatorio.Tipo);
        Assert.Equal(procedimiento.Id, recordatorio.ProcedimientoId);
        Assert.Null(recordatorio.VacunacionId);
        Assert.Contains("la desparasitación", recordatorio.Mensaje);
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_UnaAlertaVencidaUsaElTiempoPasadoEnElMensaje()
    {
        var alerta = AlertaDeVacuna(Hoy.AddDays(-5));

        var recordatorio = CrearServicio().Preparar(alerta).Valor!;

        Assert.True(alerta.Vencida);
        Assert.Contains("le correspondía", recordatorio.Mensaje);
    }

    [Theory]
    [Trait("Req", "RN-14")]
    [InlineData("2001234567")]
    [InlineData("300123456")]
    [InlineData("")]
    [InlineData("+573001234567")]
    public void RN14_ConUnTelefonoInvalidoNoSeGuardaNada(string telefono)
    {
        var alerta = AlertaDeVacuna();
        alerta.PropietarioTelefono = telefono;

        var resultado = CrearServicio().Preparar(alerta);

        Assert.False(resultado.Exito);
        Assert.Contains("celular colombiano", resultado.Mensaje);
        Assert.Equal(0, ContarRecordatorios());
    }

    [Fact]
    [Trait("Req", "RNF-04")]
    public void RNF04_SinInternetSePosponeElEnvioYNoSeGuardaNada()
    {
        var alerta = AlertaDeVacuna();

        var resultado = CrearServicio(hayInternet: false).Preparar(alerta);

        Assert.False(resultado.Exito);
        Assert.Contains("conexión a internet", resultado.Mensaje);
        Assert.Equal(0, ContarRecordatorios());
        Assert.Single(CrearAlertas().ProximasFechas());
    }

    [Fact]
    [Trait("Req", "SUP-D06")]
    public void SUPD06_PrepararDosVecesLaMismaAlertaReutilizaElRecordatorioPendiente()
    {
        var alerta = AlertaDeVacuna();

        var primero = CrearServicio().Preparar(alerta).Valor!;
        var segundo = CrearServicio().Preparar(alerta).Valor!;

        Assert.Equal(primero.Id, segundo.Id);
        Assert.Equal(1, ContarRecordatorios());
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_MarcarEnviadoGuardaEstadoYFechaYLaAlertaDejaDeMostrarse()
    {
        var alerta = AlertaDeVacuna();
        var servicio = CrearServicio();
        var recordatorio = servicio.Preparar(alerta).Valor!;
        var antes = DateTime.Now.AddSeconds(-1);

        var resultado = servicio.MarcarEnviado(recordatorio);

        Assert.True(resultado.Exito);
        using var lectura = _bd.CrearContexto();
        var guardado = lectura.Recordatorios.Single();
        Assert.Equal("Enviado", guardado.Estado);
        Assert.InRange(guardado.FechaEnvio!.Value, antes, DateTime.Now.AddSeconds(1));
        Assert.Empty(new RecordatorioRepository(lectura).ListarPendientes());
        Assert.Empty(CrearAlertas().ProximasFechas());
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_MarcarEnviadoUnRecordatorioYaEnviadoOUnoSinPrepararNoCambiaNada()
    {
        var servicio = CrearServicio();
        var recordatorio = servicio.Preparar(AlertaDeVacuna()).Valor!;
        servicio.MarcarEnviado(recordatorio);
        var fechaEnvio = recordatorio.FechaEnvio;

        Assert.True(servicio.MarcarEnviado(recordatorio).Exito);
        Assert.Equal(fechaEnvio, recordatorio.FechaEnvio);
        Assert.False(servicio.MarcarEnviado(new Recordatorio()).Exito);
    }
}
