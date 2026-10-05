using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>
/// Registro de procedimientos médicos. Solo registra: la historia clínica no se edita ni se borra (RF-09, RN-07, RN-08, CU-08).
/// </summary>
public interface IProcedimientoService
{
    /// <summary>
    /// Registra un procedimiento. Exige una mascota existente, un veterinario activo, fecha no futura,
    /// tipo y descripción. El peso, si se informa, debe ser mayor a cero.
    /// </summary>
    Resultado Registrar(Procedimiento procedimiento);
}
