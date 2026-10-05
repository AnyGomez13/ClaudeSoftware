using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Dominio.Modelos;

/// <summary>
/// Agregado cronológico de procedimientos y vacunaciones de una mascota. No se persiste (RF-06, RF-10, SUP-D04).
/// </summary>
public class HistoriaClinica
{
    public Mascota Mascota { get; set; } = null!;

    /// <summary>Fecha del primer registro clínico.</summary>
    public DateTime FechaApertura { get; set; }

    public List<RegistroClinicoItem> Registros { get; set; } = [];
}
