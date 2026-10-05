using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IAutenticacionService"/>
/// RF-01, RF-17, RNF-05, RN-01, RN-13, CU-01, CU-14.
public class AutenticacionService : IAutenticacionService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly HasherContrasena _hasher;

    public AutenticacionService(IUsuarioRepository usuarios, HasherContrasena hasher)
    {
        _usuarios = usuarios;
        _hasher = hasher;
    }

    public bool SesionActiva { get; private set; }

    public string? NombreUsuario { get; private set; }

    public bool IniciarSesion(string nombreUsuario, string clave)
    {
        CerrarSesion();

        var nombre = nombreUsuario?.Trim();
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(clave))
        {
            return false;
        }

        var usuario = _usuarios.ObtenerPorNombre(nombre);
        if (usuario is null || !usuario.Activo || !_hasher.Verificar(clave, usuario.ContrasenaHash, usuario.Salt))
        {
            return false;
        }

        SesionActiva = true;
        NombreUsuario = usuario.NombreUsuario;
        return true;
    }

    public void CerrarSesion()
    {
        SesionActiva = false;
        NombreUsuario = null;
    }

    public Resultado CambiarContrasena(string contrasenaActual, string contrasenaNueva)
    {
        if (!SesionActiva || NombreUsuario is null)
        {
            return Resultado.Error("Debe iniciar sesión para cambiar la contraseña.");
        }

        if (string.IsNullOrEmpty(contrasenaActual))
        {
            return Resultado.Error("Escriba la contraseña actual.");
        }

        if (string.IsNullOrEmpty(contrasenaNueva))
        {
            return Resultado.Error("Escriba la contraseña nueva.");
        }

        var usuario = _usuarios.ObtenerPorNombre(NombreUsuario);
        if (usuario is null || !usuario.Activo || !_hasher.Verificar(contrasenaActual, usuario.ContrasenaHash, usuario.Salt))
        {
            return Resultado.Error("La contraseña actual no es correcta.");
        }

        if (contrasenaNueva == contrasenaActual)
        {
            return Resultado.Error("La contraseña nueva debe ser distinta de la actual.");
        }

        usuario.ContrasenaHash = _hasher.Hashear(contrasenaNueva, out var salt);
        usuario.Salt = salt;
        _usuarios.Actualizar(usuario);
        return Resultado.Ok();
    }

    public Resultado CrearUsuarioInicial(string nombreUsuario, string clave)
    {
        var nombre = nombreUsuario?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            return Resultado.Error("El nombre de usuario es obligatorio.");
        }

        if (string.IsNullOrEmpty(clave))
        {
            return Resultado.Error("La contraseña es obligatoria.");
        }

        if (_usuarios.HayUsuarios())
        {
            return Resultado.Error("Ya existe un usuario; el sistema tiene una única cuenta de acceso.");
        }

        var hash = _hasher.Hashear(clave, out var salt);
        _usuarios.Agregar(new Usuario { NombreUsuario = nombre, ContrasenaHash = hash, Salt = salt });
        return Resultado.Ok();
    }
}
