using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Dominio.Modelos;

/// <summary>
/// Carnet de vacunación generado a demanda y exportado a PDF. No se persiste (RF-12, RF-13, SUP-D05).
/// </summary>
public class CarnetDigital
{
    public Mascota Mascota { get; set; } = null!;

    public Propietario Propietario { get; set; } = null!;

    public List<Vacunacion> Vacunas { get; set; } = [];

    public DateTime FechaGeneracion { get; set; }
}
