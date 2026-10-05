using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <summary>Acceso a datos de la cuenta de acceso (RF-01, RNF-05).</summary>
public interface IUsuarioRepository
{
    /// <summary>Devuelve el usuario con ese nombre exacto o nulo si no existe.</summary>
    Usuario? ObtenerPorNombre(string nombreUsuario);

    /// <summary>Indica si ya existe alguna cuenta; el sistema tiene un único usuario (R-03).</summary>
    bool HayUsuarios();

    /// <summary>Guarda los cambios de una cuenta existente, por ejemplo la nueva contraseña (CU-14).</summary>
    void Actualizar(Usuario usuario);

    /// <summary>Registra un usuario; se usa en la puesta en marcha (SUP-07).</summary>
    void Agregar(Usuario usuario);
}
