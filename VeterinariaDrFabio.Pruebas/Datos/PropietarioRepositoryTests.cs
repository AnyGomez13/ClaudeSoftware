using VeterinariaDrFabio.Datos.Repositorios;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>Pruebas de <see cref="PropietarioRepository"/> (RF-02, RF-03, RF-04, SUP-08).</summary>
public class PropietarioRepositoryTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_BuscarPorNombreEncuentraAlPropietarioConSusMascotas()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearPropietario(_bd, "Luis Gómez", "3109876543");
        DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        DatosDePrueba.CrearMascota(_bd, ana.Id, "Misu");

        using var contexto = _bd.CrearContexto();
        var resultado = new PropietarioRepository(contexto).Buscar("Pérez");

        var encontrado = Assert.Single(resultado);
        Assert.Equal("Ana Pérez", encontrado.NombreCompleto);
        Assert.Equal(["Misu", "Rocky"], encontrado.Mascotas.Select(m => m.Nombre).OrderBy(n => n).ToList());
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_BuscarPorTelefonoEncuentraAlPropietario()
    {
        DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearPropietario(_bd, "Luis Gómez", "3109876543");

        using var contexto = _bd.CrearContexto();
        var resultado = new PropietarioRepository(contexto).Buscar("3109876543");

        Assert.Equal("Luis Gómez", Assert.Single(resultado).NombreCompleto);
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_BuscarSinCoincidenciasDevuelveListaVaciaYTextoVacioDevuelveTodos()
    {
        DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearPropietario(_bd, "Luis Gómez", "3109876543");

        using var contexto = _bd.CrearContexto();
        var repositorio = new PropietarioRepository(contexto);

        Assert.Empty(repositorio.Buscar("Zzz"));
        Assert.Equal(2, repositorio.Buscar("  ").Count);
    }

    [Fact]
    [Trait("Req", "RF-04")]
    public void RF04_ActualizarGuardaElNuevoTelefono()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");

        using (var contexto = _bd.CrearContexto())
        {
            var repositorio = new PropietarioRepository(contexto);
            var propietario = repositorio.ObtenerPorId(ana.Id)!;
            propietario.Telefono = "3205550000";
            repositorio.Actualizar(propietario);
        }

        using var lectura = _bd.CrearContexto();
        Assert.Equal("3205550000", new PropietarioRepository(lectura).ObtenerPorId(ana.Id)!.Telefono);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_InactivarConservaAlPropietarioEnLaBase()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");

        using (var contexto = _bd.CrearContexto())
        {
            var repositorio = new PropietarioRepository(contexto);
            var propietario = repositorio.ObtenerPorId(ana.Id)!;
            propietario.Activo = false;
            repositorio.Actualizar(propietario);
        }

        using var lectura = _bd.CrearContexto();
        var inactivo = new PropietarioRepository(lectura).ObtenerPorId(ana.Id);
        Assert.NotNull(inactivo);
        Assert.False(inactivo.Activo);
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_ObtenerPorIdDevuelveNuloSiNoExiste()
    {
        using var contexto = _bd.CrearContexto();

        Assert.Null(new PropietarioRepository(contexto).ObtenerPorId(999));
    }
}
