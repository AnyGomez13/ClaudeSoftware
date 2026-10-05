using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>Reglas de negocio de las mascotas (RF-05, RF-07, RF-08, RN-02, RN-03, RN-06, CU-05, CU-07).</summary>
public interface IMascotaService
{
    /// <summary>
    /// Registra una mascota. Exige un propietario existente, nombre, especie, fecha de nacimiento
    /// (real o estimada, no futura) y peso mayor a cero en kilogramos.
    /// </summary>
    Resultado Registrar(Mascota mascota);

    /// <summary>Actualiza una mascota existente con las mismas reglas; también la inactiva o reactiva (SUP-08).</summary>
    Resultado Editar(Mascota mascota);

    /// <summary>Busca por nombre de la mascota o del propietario; con texto vacío devuelve todas.</summary>
    List<Mascota> Buscar(string texto);

    /// <summary>Devuelve la mascota con su propietario o nulo si no existe.</summary>
    Mascota? ObtenerPorId(int id);

    /// <summary>Edad en meses cumplidos, calculada con la fecha actual y nunca guardada (RF-08).</summary>
    int CalcularEdadMeses(Mascota mascota);

    /// <summary>Edad en texto, por ejemplo "2 años 3 meses" (RF-08).</summary>
    string CalcularEdadLegible(Mascota mascota);
}
