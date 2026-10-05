namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>Inicio y cierre de sesión contra la base de datos (RF-01, RNF-05, RN-01, RN-13, CU-01).</summary>
public interface IAutenticacionService
{
    /// <summary>Hay una sesión iniciada; sin ella no se accede a ninguna función (RN-13).</summary>
    bool SesionActiva { get; }

    /// <summary>Nombre del usuario con sesión iniciada o nulo.</summary>
    string? NombreUsuario { get; }

    /// <summary>
    /// Valida usuario y contraseña contra la base. Devuelve falso si el usuario no existe, está inactivo
    /// o la contraseña no coincide, y en ese caso no queda sesión activa.
    /// </summary>
    bool IniciarSesion(string nombreUsuario, string clave);

    void CerrarSesion();

    /// <summary>
    /// Cambia la contraseña de la cuenta con sesión iniciada (CU-14). Exige la contraseña actual correcta y una nueva
    /// que no esté vacía y sea distinta de la actual; la nueva se guarda solo como hash con sal. La sesión sigue activa.
    /// </summary>
    Resultado CambiarContrasena(string contrasenaActual, string contrasenaNueva);

    /// <summary>
    /// Crea la única cuenta del sistema en la puesta en marcha (SUP-07). Falla si ya existe un usuario
    /// (R-03) o si el nombre o la contraseña están vacíos. La contraseña se guarda solo como hash con sal.
    /// </summary>
    Resultado CrearUsuarioInicial(string nombreUsuario, string clave);
}
