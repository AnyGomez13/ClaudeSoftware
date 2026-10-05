namespace VeterinariaDrFabio.Dominio.Modelos;

/// <summary>
/// Línea de la historia clínica: un procedimiento o una vacunación (RF-10).
/// </summary>
public class RegistroClinicoItem
{
    public DateTime Fecha { get; set; }

    /// <summary>"Procedimiento" o "Vacunacion".</summary>
    public string Origen { get; set; } = string.Empty;

    public string Detalle { get; set; } = string.Empty;

    /// <summary>Nombre del veterinario que realizó el registro (RN-07).</summary>
    public string Veterinario { get; set; } = string.Empty;
}
