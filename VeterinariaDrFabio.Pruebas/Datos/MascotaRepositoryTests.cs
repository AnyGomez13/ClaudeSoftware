using VeterinariaDrFabio.Datos.Repositorios;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>Pruebas de <see cref="MascotaRepository"/> (RF-05, RF-06, RF-07, SUP-08).</summary>
public class MascotaRepositoryTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_BuscarPorNombreDeMascotaOPorNombreDelPropietario()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");
        var luis = DatosDePrueba.CrearPropietario(_bd, "Luis Gómez", "3109876543");
        DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        DatosDePrueba.CrearMascota(_bd, luis.Id, "Misu");

        using var contexto = _bd.CrearContexto();
        var repositorio = new MascotaRepository(contexto);

        Assert.Equal("Rocky", Assert.Single(repositorio.Buscar("Rock")).Nombre);
        var porPropietario = Assert.Single(repositorio.Buscar("Gómez"));
        Assert.Equal("Misu", porPropietario.Nombre);
        Assert.Equal("Luis Gómez", porPropietario.Propietario!.NombreCompleto);
        Assert.Empty(repositorio.Buscar("Zzz"));
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_ObtenerPorIdIncluyeAlPropietarioYGuardaLaFechaDeNacimiento()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);

        using var contexto = _bd.CrearContexto();
        var mascota = new MascotaRepository(contexto).ObtenerPorId(rocky.Id);

        Assert.NotNull(mascota);
        Assert.Equal("Ana Pérez", mascota.Propietario!.NombreCompleto);
        Assert.Equal(new DateTime(2020, 3, 15), mascota.FechaNacimiento);
        Assert.Equal(12.5, mascota.Peso);
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_ActualizarGuardaElNuevoPeso()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);

        using (var contexto = _bd.CrearContexto())
        {
            var repositorio = new MascotaRepository(contexto);
            var mascota = repositorio.ObtenerPorId(rocky.Id)!;
            mascota.Peso = 13.2;
            repositorio.Actualizar(mascota);
        }

        using var lectura = _bd.CrearContexto();
        Assert.Equal(13.2, new MascotaRepository(lectura).ObtenerPorId(rocky.Id)!.Peso);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_InactivarConservaALaMascotaEnLaBase()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);

        using (var contexto = _bd.CrearContexto())
        {
            var repositorio = new MascotaRepository(contexto);
            var mascota = repositorio.ObtenerPorId(rocky.Id)!;
            mascota.Activo = false;
            repositorio.Actualizar(mascota);
        }

        using var lectura = _bd.CrearContexto();
        Assert.False(new MascotaRepository(lectura).ObtenerPorId(rocky.Id)!.Activo);
    }
}
