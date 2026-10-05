namespace VeterinariaDrFabio.Dominio.Entidades;

/// <summary>
/// Aplicación de una vacuna a una mascota; base del carnet y de las alertas (RF-11, RF-12, RN-07).
/// </summary>
public class Vacunacion
{
    public int Id { get; set; }

    public int MascotaId { get; set; }

    /// <summary>Veterinario que aplicó la vacuna (R-03).</summary>
    public int VeterinarioId { get; set; }

    public string NombreVacuna { get; set; } = string.Empty;

    public DateTime FechaAplicacion { get; set; }

    /// <summary>Fecha del refuerzo; alimenta alertas y recordatorios (RF-14, RF-15).</summary>
    public DateTime? ProximaFecha { get; set; }

    public string? Lote { get; set; }

    public string? Observaciones { get; set; }

    public Mascota? Mascota { get; set; }

    public Veterinario? Veterinario { get; set; }
}
