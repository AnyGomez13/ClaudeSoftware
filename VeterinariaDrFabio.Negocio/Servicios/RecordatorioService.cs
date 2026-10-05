using System.Globalization;
using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Dominio.Modelos;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IRecordatorioService"/>
/// RF-15, RN-10, RN-14, RN-15, RNF-04, RNF-06, SUP-01, SUP-09, SUP-D06, CU-12.
public class RecordatorioService : IRecordatorioService
{
    private readonly IRecordatorioRepository _recordatorios;
    private readonly GeneradorEnlaceWhatsApp _generadorEnlace;
    private readonly IConectividad _conectividad;

    public RecordatorioService(
        IRecordatorioRepository recordatorios,
        GeneradorEnlaceWhatsApp generadorEnlace,
        IConectividad conectividad)
    {
        _recordatorios = recordatorios;
        _generadorEnlace = generadorEnlace;
        _conectividad = conectividad;
    }

    public Resultado<Recordatorio> Preparar(AlertaProximaFecha alerta)
    {
        var mensaje = ArmarMensaje(alerta);

        string enlace;
        try
        {
            enlace = _generadorEnlace.Construir(alerta.PropietarioTelefono, mensaje);
        }
        catch (ArgumentException)
        {
            return Resultado<Recordatorio>.Error(
                "El teléfono del propietario no es un celular colombiano válido (10 dígitos que inicia en 3).");
        }

        if (!_conectividad.HayInternet())
        {
            return Resultado<Recordatorio>.Error(
                "No hay conexión a internet. Intente enviar el recordatorio cuando haya conexión; el resto del sistema sigue funcionando.");
        }

        var pendiente = _recordatorios.ListarPendientes().FirstOrDefault(r =>
            r.VacunacionId == alerta.VacunacionId && r.ProcedimientoId == alerta.ProcedimientoId);
        if (pendiente is not null)
        {
            pendiente.Mensaje = mensaje;
            pendiente.Enlace = enlace;
            pendiente.FechaObjetivo = alerta.FechaObjetivo;
            _recordatorios.Actualizar(pendiente);
            return Resultado<Recordatorio>.Ok(pendiente);
        }

        var recordatorio = new Recordatorio
        {
            MascotaId = alerta.MascotaId,
            Tipo = alerta.Tipo,
            FechaObjetivo = alerta.FechaObjetivo,
            Mensaje = mensaje,
            Enlace = enlace,
            Estado = "Pendiente",
            VacunacionId = alerta.VacunacionId,
            ProcedimientoId = alerta.ProcedimientoId,
        };
        _recordatorios.Agregar(recordatorio);
        return Resultado<Recordatorio>.Ok(recordatorio);
    }

    public Resultado MarcarEnviado(Recordatorio recordatorio)
    {
        if (recordatorio.Id <= 0)
        {
            return Resultado.Error("El recordatorio no se ha preparado todavía.");
        }

        if (recordatorio.Estado == "Enviado")
        {
            return Resultado.Ok();
        }

        recordatorio.Estado = "Enviado";
        recordatorio.FechaEnvio = DateTime.Now;
        _recordatorios.Actualizar(recordatorio);
        return Resultado.Ok();
    }

    /// <summary>Texto fijo del recordatorio con propietario, mascota, tipo y fecha (supuesto A-10).</summary>
    private static string ArmarMensaje(AlertaProximaFecha alerta)
    {
        var fecha = alerta.FechaObjetivo.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var motivo = alerta.Tipo == "Vacunacion"
            ? $"el refuerzo de la vacuna {alerta.Detalle}"
            : "la desparasitación";
        var verbo = alerta.Vencida ? "le correspondía" : "le corresponde";

        return $"Hola {alerta.PropietarioNombre}, le saludamos de la Clínica Veterinaria Dr. Fabio. " +
               $"Le recordamos que a {alerta.MascotaNombre} {verbo} {motivo} el {fecha}. Lo esperamos en la clínica.";
    }
}
