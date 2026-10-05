namespace VeterinariaDrFabio.Dominio.Modelos;

/// <summary>
/// Fila de la pantalla de alertas: una vacunación o desparasitación próxima o vencida, con los datos
/// de la mascota y el propietario para preparar el recordatorio. No se persiste (RF-14, RF-15, RN-11, CU-11).
/// </summary>
public class AlertaProximaFecha
{
    public int MascotaId { get; set; }

    public string MascotaNombre { get; set; } = string.Empty;

    public string PropietarioNombre { get; set; } = string.Empty;

    /// <summary>Celular del propietario, 10 dígitos sin prefijo (SUP-D07).</summary>
    public string PropietarioTelefono { get; set; } = string.Empty;

    /// <summary>"Vacunacion" o "Desparasitacion", igual que <c>Recordatorio.Tipo</c>.</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Nombre de la vacuna o "Desparasitación".</summary>
    public string Detalle { get; set; } = string.Empty;

    public DateTime FechaObjetivo { get; set; }

    /// <summary>Verdadero si la fecha objetivo ya pasó (se resalta como error en P-11).</summary>
    public bool Vencida { get; set; }

    /// <summary>Vacunación que origina la alerta; nulo si el origen es un procedimiento.</summary>
    public int? VacunacionId { get; set; }

    /// <summary>Procedimiento de desparasitación que origina la alerta; nulo si el origen es una vacunación.</summary>
    public int? ProcedimientoId { get; set; }
}
