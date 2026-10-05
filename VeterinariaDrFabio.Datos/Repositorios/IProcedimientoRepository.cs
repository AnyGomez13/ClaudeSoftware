using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <summary>Acceso a datos de los procedimientos médicos (RF-09, RF-10, RF-14).</summary>
public interface IProcedimientoRepository
{
    /// <summary>Solo agrega: la historia clínica no se edita ni se borra (RN-08).</summary>
    void Agregar(Procedimiento procedimiento);

    /// <summary>Procedimientos de la mascota en orden cronológico ascendente, con su veterinario.</summary>
    List<Procedimiento> ListarPorMascota(int mascotaId);

    /// <summary>Procedimientos con próxima fecha recomendada, con mascota y propietario, ordenados por esa fecha.</summary>
    List<Procedimiento> ListarConProximaFecha();
}
