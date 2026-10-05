using System.Globalization;
using System.Text;
using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Dominio.Modelos;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IAlertaService"/>
/// RF-14, RN-11, SUP-09, CU-11.
public class AlertaService : IAlertaService
{
    /// <summary>Días hacia adelante en que una fecha se considera próxima (supuesto A-08).</summary>
    public const int DiasDeAnticipacion = 30;

    private readonly IProcedimientoRepository _procedimientos;
    private readonly IVacunacionRepository _vacunaciones;
    private readonly IRecordatorioRepository _recordatorios;

    public AlertaService(
        IProcedimientoRepository procedimientos,
        IVacunacionRepository vacunaciones,
        IRecordatorioRepository recordatorios)
    {
        _procedimientos = procedimientos;
        _vacunaciones = vacunaciones;
        _recordatorios = recordatorios;
    }

    public List<AlertaProximaFecha> ProximasFechas()
    {
        var hoy = DateTime.Today;
        var limite = hoy.AddDays(DiasDeAnticipacion);

        var enviados = _recordatorios.ListarEnviados();
        var vacunasAvisadas = enviados.Where(r => r.VacunacionId is not null).Select(r => r.VacunacionId!.Value).ToHashSet();
        var procedimientosAvisados = enviados.Where(r => r.ProcedimientoId is not null).Select(r => r.ProcedimientoId!.Value).ToHashSet();

        var vacunasDeLaMascota = new Dictionary<int, List<Vacunacion>>();
        var procedimientosDeLaMascota = new Dictionary<int, List<Procedimiento>>();
        var alertas = new List<(AlertaProximaFecha Alerta, int Id)>();

        foreach (var vacunacion in _vacunaciones.ListarConProximaFecha())
        {
            var fecha = vacunacion.ProximaFecha!.Value.Date;
            if (fecha > limite || vacunasAvisadas.Contains(vacunacion.Id) || vacunacion.Mascota is not { Activo: true })
            {
                continue;
            }

            if (!vacunasDeLaMascota.TryGetValue(vacunacion.MascotaId, out var aplicadas))
            {
                aplicadas = _vacunaciones.ListarPorMascota(vacunacion.MascotaId);
                vacunasDeLaMascota[vacunacion.MascotaId] = aplicadas;
            }

            var nombre = Comparable(vacunacion.NombreVacuna);
            var reemplazada = aplicadas.Any(a =>
                a.Id != vacunacion.Id && Comparable(a.NombreVacuna) == nombre && a.FechaAplicacion.Date > vacunacion.FechaAplicacion.Date);
            if (reemplazada)
            {
                continue;
            }

            alertas.Add((Crear(vacunacion.Mascota, "Vacunacion", vacunacion.NombreVacuna, fecha, hoy, vacunacion.Id, null), vacunacion.Id));
        }

        foreach (var procedimiento in _procedimientos.ListarConProximaFecha())
        {
            var fecha = procedimiento.ProximaFechaRecomendada!.Value.Date;
            if (!EsDesparasitacion(procedimiento.TipoProcedimiento)
                || fecha > limite
                || procedimientosAvisados.Contains(procedimiento.Id)
                || procedimiento.Mascota is not { Activo: true })
            {
                continue;
            }

            if (!procedimientosDeLaMascota.TryGetValue(procedimiento.MascotaId, out var realizados))
            {
                realizados = _procedimientos.ListarPorMascota(procedimiento.MascotaId);
                procedimientosDeLaMascota[procedimiento.MascotaId] = realizados;
            }

            var reemplazado = realizados.Any(r =>
                r.Id != procedimiento.Id && EsDesparasitacion(r.TipoProcedimiento) && r.Fecha.Date > procedimiento.Fecha.Date);
            if (reemplazado)
            {
                continue;
            }

            alertas.Add((Crear(procedimiento.Mascota, "Desparasitacion", "Desparasitación", fecha, hoy, null, procedimiento.Id), procedimiento.Id));
        }

        return alertas
            .OrderBy(a => a.Alerta.FechaObjetivo)
            .ThenBy(a => a.Alerta.Tipo)
            .ThenBy(a => a.Id)
            .Select(a => a.Alerta)
            .ToList();
    }

    private static AlertaProximaFecha Crear(
        Mascota mascota, string tipo, string detalle, DateTime fecha, DateTime hoy, int? vacunacionId, int? procedimientoId) => new()
        {
            MascotaId = mascota.Id,
            MascotaNombre = mascota.Nombre,
            PropietarioNombre = mascota.Propietario?.NombreCompleto ?? string.Empty,
            PropietarioTelefono = mascota.Propietario?.Telefono ?? string.Empty,
            Tipo = tipo,
            Detalle = detalle,
            FechaObjetivo = fecha,
            Vencida = fecha < hoy,
            VacunacionId = vacunacionId,
            ProcedimientoId = procedimientoId,
        };

    /// <summary>Un procedimiento genera alerta solo si su tipo es "desparasitación", sin importar tildes ni mayúsculas (A-09).</summary>
    private static bool EsDesparasitacion(string tipoProcedimiento) => Comparable(tipoProcedimiento) == "desparasitacion";

    private static string Comparable(string texto)
    {
        var descompuesto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sinTildes = descompuesto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(sinTildes.ToArray()).ToLowerInvariant();
    }
}
