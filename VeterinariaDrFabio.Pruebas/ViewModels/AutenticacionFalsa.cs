using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Autenticación de prueba: acepta una sola pareja de credenciales y recuerda cuántas veces se llamó.</summary>
public sealed class AutenticacionFalsa : IAutenticacionService
{
    public const string UsuarioValido = "fabio";
    public const string ClaveValida = "Clave-Segura-1";

    public bool SesionActiva { get; private set; }

    public string? NombreUsuario { get; private set; }

    public int IntentosDeSesion { get; private set; }

    public string? UltimoUsuarioRecibido { get; private set; }

    public bool IniciarSesion(string nombreUsuario, string clave)
    {
        IntentosDeSesion++;
        UltimoUsuarioRecibido = nombreUsuario;
        SesionActiva = nombreUsuario == UsuarioValido && clave == ClaveValida;
        NombreUsuario = SesionActiva ? nombreUsuario : null;
        return SesionActiva;
    }

    public void CerrarSesion()
    {
        SesionActiva = false;
        NombreUsuario = null;
    }

    public Resultado CrearUsuarioInicial(string nombreUsuario, string clave) => Resultado.Ok();
}
