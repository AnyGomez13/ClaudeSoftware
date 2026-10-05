namespace VeterinariaDrFabio.Dominio.Entidades;

/// <summary>
/// Profesional que atiende; su nombre queda en cada procedimiento y vacunación (R-03, RF-16, RN-07).
/// </summary>
public class Veterinario
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string? RegistroProfesional { get; set; }

    public bool Activo { get; set; } = true;
}
