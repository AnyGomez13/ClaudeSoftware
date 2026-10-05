namespace VeterinariaDrFabio.Dominio.Entidades;

/// <summary>
/// Aviso preparado para enviar al propietario por WhatsApp (RF-14, RF-15, SUP-01, SUP-09, SUP-D06).
/// Tiene exactamente un origen: una vacunación o un procedimiento de desparasitación.
/// </summary>
public class Recordatorio
{
    public int Id { get; set; }

    public int MascotaId { get; set; }

    /// <summary>"Vacunacion" o "Desparasitacion".</summary>
    public string Tipo { get; set; } = string.Empty;

    public DateTime FechaObjetivo { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Enlace wa.me de click-to-chat (SUP-01).</summary>
    public string Enlace { get; set; } = string.Empty;

    /// <summary>"Pendiente" o "Enviado".</summary>
    public string Estado { get; set; } = "Pendiente";

    public DateTime? FechaEnvio { get; set; }

    public int? VacunacionId { get; set; }

    public int? ProcedimientoId { get; set; }

    public Mascota? Mascota { get; set; }

    public Vacunacion? Vacunacion { get; set; }

    public Procedimiento? Procedimiento { get; set; }
}
