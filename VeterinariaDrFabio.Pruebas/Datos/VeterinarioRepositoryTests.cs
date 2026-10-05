using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>Pruebas de <see cref="VeterinarioRepository"/> (RF-16, SUP-06).</summary>
public class VeterinarioRepositoryTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_ListarActivosDevuelveAFabioYWilliam()
    {
        using var contexto = _bd.CrearContexto();

        var nombres = new VeterinarioRepository(contexto).ListarActivos().Select(v => v.NombreCompleto).ToList();

        Assert.Equal(["Fabio", "William"], nombres);
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_AgregarYActualizarSeReflejanEnLosListados()
    {
        using (var contexto = _bd.CrearContexto())
        {
            new VeterinarioRepository(contexto).Agregar(new Veterinario { NombreCompleto = "Camila Rojas" });
        }

        var idWilliam = DatosDePrueba.IdDeVeterinario(_bd, "William");
        using (var contexto = _bd.CrearContexto())
        {
            var repositorio = new VeterinarioRepository(contexto);
            var william = repositorio.ObtenerPorId(idWilliam)!;
            william.Activo = false;
            repositorio.Actualizar(william);
        }

        using var lectura = _bd.CrearContexto();
        var lecturaRepositorio = new VeterinarioRepository(lectura);
        Assert.Equal(["Camila Rojas", "Fabio"], lecturaRepositorio.ListarActivos().Select(v => v.NombreCompleto).ToList());
        Assert.Equal(3, lecturaRepositorio.ListarTodos().Count);
    }
}
