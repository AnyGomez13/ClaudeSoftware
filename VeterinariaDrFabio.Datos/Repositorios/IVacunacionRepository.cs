using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <summary>Acceso a datos de las vacunaciones (RF-11, RF-12, RF-14).</summary>
public interface IVacunacionRepository
{
    /// <summary>Solo agrega: la historia clínica no se edita ni se borra (RN-08).</summary>
    void Agregar(Vacunacion vacunacion);

    /// <summary>Vacunaciones de la mascota en orden cronológico ascendente, con su veterinario.</summary>
    List<Vacunacion> ListarPorMascota(int mascotaId);

    /// <summary>Vacunaciones con próxima fecha de refuerzo, con mascota y propietario, ordenadas por esa fecha.</summary>
    List<Vacunacion> ListarConProximaFecha();
}
