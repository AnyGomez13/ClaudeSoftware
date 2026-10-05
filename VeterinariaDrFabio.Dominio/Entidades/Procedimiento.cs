namespace VeterinariaDrFabio.Dominio.Entidades;

/// <summary>
/// Registro clínico de una atención médica (RF-09, RF-10, RN-07, RN-08).
/// </summary>
public class Procedimiento
{
    public int Id { get; set; }

    public int MascotaId { get; set; }

    /// <summary>Veterinario que evaluó la atención (R-03).</summary>
    public int VeterinarioId { get; set; }

    public DateTime Fecha { get; set; }

    /// <summary>Consulta, cirugía, control, desparasitación… (texto libre, SUP-D11).</summary>
    public string TipoProcedimiento { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string? Tratamiento { get; set; }

    /// <summary>Peso en kilogramos al momento de la atención (SUP-05).</summary>
    public double? PesoEnElMomento { get; set; }

    /// <summary>Alimenta las alertas de próximas fechas (RF-14).</summary>
    public DateTime? ProximaFechaRecomendada { get; set; }

    public Mascota? Mascota { get; set; }

    public Veterinario? Veterinario { get; set; }
}
