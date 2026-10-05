using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>
/// Registro de vacunaciones. Solo registra: la historia clínica no se edita ni se borra (RF-11, RN-07, RN-08, CU-09).
/// </summary>
public interface IVacunacionService
{
    /// <summary>
    /// Registra una vacunación. Exige una mascota existente, un veterinario activo, nombre de la vacuna y fecha
    /// de aplicación no futura. La próxima fecha de refuerzo, si se informa, no puede ser anterior a la aplicación.
    /// </summary>
    Resultado Registrar(Vacunacion vacunacion);
}
