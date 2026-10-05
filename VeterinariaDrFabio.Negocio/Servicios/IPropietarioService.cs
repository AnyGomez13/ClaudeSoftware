using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>Reglas de negocio de los propietarios (RF-02, RF-03, RF-04, RN-04, RN-14, CU-02, CU-03, CU-04).</summary>
public interface IPropietarioService
{
    /// <summary>
    /// Registra un propietario. Exige nombre completo y un celular colombiano de 10 dígitos que inicia en 3.
    /// </summary>
    Resultado Registrar(Propietario propietario);

    /// <summary>
    /// Actualiza un propietario existente conservando las reglas mínimas; también inactiva o reactiva (SUP-08).
    /// </summary>
    Resultado Editar(Propietario propietario);

    /// <summary>Busca por nombre o teléfono e incluye las mascotas; con texto vacío devuelve todos.</summary>
    List<Propietario> Buscar(string texto);

    Propietario? ObtenerPorId(int id);
}
