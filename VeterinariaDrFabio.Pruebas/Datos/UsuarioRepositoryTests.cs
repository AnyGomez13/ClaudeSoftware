using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>Pruebas de <see cref="UsuarioRepository"/> (RF-01, RN-01).</summary>
public class UsuarioRepositoryTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ObtenerPorNombreDevuelveElUsuarioGuardado()
    {
        using (var contexto = _bd.CrearContexto())
        {
            new UsuarioRepository(contexto).Agregar(new Usuario { NombreUsuario = "fabio", ContrasenaHash = "h", Salt = "s" });
        }

        using var lectura = _bd.CrearContexto();
        var usuario = new UsuarioRepository(lectura).ObtenerPorNombre("fabio");

        Assert.NotNull(usuario);
        Assert.Equal("h", usuario.ContrasenaHash);
        Assert.True(usuario.Activo);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ObtenerPorNombreDevuelveNuloSiNoExiste()
    {
        using var contexto = _bd.CrearContexto();

        Assert.Null(new UsuarioRepository(contexto).ObtenerPorNombre("nadie"));
    }
}
