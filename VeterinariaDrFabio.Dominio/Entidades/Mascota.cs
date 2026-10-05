namespace VeterinariaDrFabio.Dominio.Entidades;

/// <summary>
/// Paciente de la clínica; pertenece a un único propietario (RF-05, RF-07, RN-02, RN-03, RN-06).
/// La edad no se almacena: se calcula desde <see cref="FechaNacimiento"/> (RF-08, RN-05).
/// </summary>
public class Mascota
{
    public int Id { get; set; }

    public int PropietarioId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Especie { get; set; } = string.Empty;

    public string? Raza { get; set; }

    /// <summary>"Macho", "Hembra" o nulo.</summary>
    public string? Sexo { get; set; }

    public DateTime FechaNacimiento { get; set; }

    /// <summary>Verdadero cuando la fecha de nacimiento es estimada (SUP-04).</summary>
    public bool FechaNacimientoEstimada { get; set; }

    /// <summary>Peso actual en kilogramos (RN-06).</summary>
    public double Peso { get; set; }

    public string? ColorSenas { get; set; }

    /// <summary>Borrado lógico; las mascotas no se eliminan (SUP-08).</summary>
    public bool Activo { get; set; } = true;

    public Propietario? Propietario { get; set; }

    public ICollection<Procedimiento> Procedimientos { get; set; } = [];

    public ICollection<Vacunacion> Vacunaciones { get; set; } = [];

    public ICollection<Recordatorio> Recordatorios { get; set; } = [];
}
