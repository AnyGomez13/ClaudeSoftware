using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>Pruebas de <see cref="RecordatorioRepository"/> (RF-14, RF-15, SUP-D06).</summary>
public class RecordatorioRepositoryTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private int CrearVacunacion(int mascotaId)
    {
        using var contexto = _bd.CrearContexto();
        var vacunacion = new Vacunacion
        {
            MascotaId = mascotaId,
            VeterinarioId = DatosDePrueba.IdDeVeterinario(_bd, "Fabio"),
            NombreVacuna = "Rabia",
            FechaAplicacion = new DateTime(2026, 10, 1),
            ProximaFecha = new DateTime(2027, 10, 1),
        };
        new VacunacionRepository(contexto).Agregar(vacunacion);
        return vacunacion.Id;
    }

    private static Recordatorio Nuevo(int mascotaId, int vacunacionId, DateTime fechaObjetivo) => new()
    {
        MascotaId = mascotaId,
        Tipo = "Vacunacion",
        FechaObjetivo = fechaObjetivo,
        Mensaje = "Mensaje",
        Enlace = "https://wa.me/573001234567",
        VacunacionId = vacunacionId,
    };

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_ListarPendientesExcluyeLosEnviadosYOrdenaPorFechaObjetivo()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);
        var vacunacionId = CrearVacunacion(rocky.Id);

        using (var contexto = _bd.CrearContexto())
        {
            var repositorio = new RecordatorioRepository(contexto);
            repositorio.Agregar(Nuevo(rocky.Id, vacunacionId, new DateTime(2027, 11, 1)));
            repositorio.Agregar(Nuevo(rocky.Id, vacunacionId, new DateTime(2027, 10, 1)));
            var enviado = Nuevo(rocky.Id, vacunacionId, new DateTime(2027, 9, 1));
            repositorio.Agregar(enviado);
            enviado.Estado = "Enviado";
            enviado.FechaEnvio = new DateTime(2026, 10, 5, 9, 0, 0);
            repositorio.Actualizar(enviado);
        }

        using var lectura = _bd.CrearContexto();
        var pendientes = new RecordatorioRepository(lectura).ListarPendientes();

        Assert.Equal(
            [new DateTime(2027, 10, 1), new DateTime(2027, 11, 1)],
            pendientes.Select(r => r.FechaObjetivo).ToList());
        Assert.All(pendientes, r => Assert.Equal("Ana Pérez", r.Mascota!.Propietario!.NombreCompleto));
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_ActualizarGuardaElEstadoEnviadoYLaFechaDeEnvio()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);
        var vacunacionId = CrearVacunacion(rocky.Id);
        var recordatorio = Nuevo(rocky.Id, vacunacionId, new DateTime(2027, 10, 1));

        using (var contexto = _bd.CrearContexto())
        {
            var repositorio = new RecordatorioRepository(contexto);
            repositorio.Agregar(recordatorio);
            recordatorio.Estado = "Enviado";
            recordatorio.FechaEnvio = new DateTime(2026, 10, 5, 9, 0, 0);
            repositorio.Actualizar(recordatorio);
        }

        using var lectura = _bd.CrearContexto();
        var guardado = lectura.Recordatorios.Single();
        Assert.Equal("Enviado", guardado.Estado);
        Assert.Equal(new DateTime(2026, 10, 5, 9, 0, 0), guardado.FechaEnvio);
        Assert.Empty(new RecordatorioRepository(lectura).ListarPendientes());
    }
}
