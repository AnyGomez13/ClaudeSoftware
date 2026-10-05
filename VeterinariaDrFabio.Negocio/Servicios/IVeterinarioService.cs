using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>Gestión de los veterinarios que se atribuyen a procedimientos y vacunaciones (R-03, RF-16, CU-13).</summary>
public interface IVeterinarioService
{
    /// <summary>Veterinarios activos, para los selectores de procedimiento y vacunación.</summary>
    List<Veterinario> ListarActivos();

    /// <summary>Todos los veterinarios con su estado, para la pantalla P-12.</summary>
    List<Veterinario> ListarTodos();

    /// <summary>Registra un veterinario nuevo; el nombre completo es obligatorio.</summary>
    Resultado Registrar(Veterinario veterinario);

    /// <summary>Actualiza nombre, registro profesional y estado de un veterinario existente.</summary>
    Resultado Editar(Veterinario veterinario);
}
