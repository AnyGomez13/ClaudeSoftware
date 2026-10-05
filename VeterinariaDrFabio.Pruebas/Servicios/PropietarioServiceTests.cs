using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="PropietarioService"/> (RF-02, RF-03, RF-04, RN-04, RN-14, SUP-08).</summary>
public class PropietarioServiceTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private PropietarioService CrearServicio() => new(new PropietarioRepository(_bd.CrearContexto()));

    private int ContarPropietarios() => _bd.Escalar<int>("SELECT COUNT(*) FROM Propietario;");

    [Fact]
    [Trait("Req", "RF-02")]
    public void RF02_RegistrarGuardaAlPropietarioConSoloNombreYTelefono()
    {
        var propietario = new Propietario { NombreCompleto = "  Ana Pérez  ", Telefono = " 3001234567 ", Documento = "  ", Correo = "", Direccion = null };

        var resultado = CrearServicio().Registrar(propietario);

        Assert.True(resultado.Exito);
        Assert.True(propietario.Id > 0);
        var guardado = CrearServicio().ObtenerPorId(propietario.Id)!;
        Assert.Equal("Ana Pérez", guardado.NombreCompleto);
        Assert.Equal("3001234567", guardado.Telefono);
        Assert.Null(guardado.Documento);
        Assert.Null(guardado.Correo);
        Assert.Null(guardado.Direccion);
        Assert.True(guardado.Activo);
    }

    [Theory]
    [Trait("Req", "RN-04")]
    [InlineData("", "3001234567")]
    [InlineData("   ", "3001234567")]
    [InlineData("Ana Pérez", "")]
    [InlineData("Ana Pérez", "   ")]
    public void RN04_SinNombreOSinTelefonoNoSeGuarda(string nombre, string telefono)
    {
        var resultado = CrearServicio().Registrar(new Propietario { NombreCompleto = nombre, Telefono = telefono });

        Assert.False(resultado.Exito);
        Assert.NotEmpty(resultado.Mensaje);
        Assert.Equal(0, ContarPropietarios());
    }

    [Theory]
    [Trait("Req", "RN-14")]
    [InlineData("2001234567")]
    [InlineData("300123456")]
    [InlineData("30012345678")]
    [InlineData("30012345ab")]
    [InlineData("573001234567")]
    [InlineData("+573001234567")]
    [InlineData("300 123 4567")]
    public void RN14_UnTelefonoQueNoEsCelularColombianoSeRechaza(string telefono)
    {
        var resultado = CrearServicio().Registrar(new Propietario { NombreCompleto = "Ana Pérez", Telefono = telefono });

        Assert.False(resultado.Exito);
        Assert.Contains("celular colombiano", resultado.Mensaje);
        Assert.Equal(0, ContarPropietarios());
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_BuscarPorNombreOPorTelefonoDevuelveAlPropietarioConSusMascotas()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearPropietario(_bd, "Luis Gómez", "3109876543");
        DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        var servicio = CrearServicio();

        var porNombre = Assert.Single(servicio.Buscar("Pérez"));
        var porTelefono = Assert.Single(servicio.Buscar("3001234567"));

        Assert.Equal(ana.Id, porNombre.Id);
        Assert.Equal(ana.Id, porTelefono.Id);
        Assert.Equal("Rocky", Assert.Single(porNombre.Mascotas).Nombre);
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_BuscarSinResultadosDevuelveListaVacia()
    {
        DatosDePrueba.CrearPropietario(_bd);

        Assert.Empty(CrearServicio().Buscar("Zzz"));
    }

    [Fact]
    [Trait("Req", "RF-04")]
    public void RF04_EditarActualizaElTelefono()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");

        var resultado = CrearServicio().Editar(new Propietario
        {
            Id = ana.Id,
            NombreCompleto = "Ana Pérez",
            Telefono = "3205550000",
            Activo = true,
        });

        Assert.True(resultado.Exito);
        Assert.Equal("3205550000", CrearServicio().ObtenerPorId(ana.Id)!.Telefono);
    }

    [Theory]
    [Trait("Req", "RF-04")]
    [InlineData("", "3205550000")]
    [InlineData("Ana Pérez", "123")]
    [InlineData("Ana Pérez", "")]
    public void RF04_EditarConDatosMinimosInvalidosNoGuardaYConservaLoAnterior(string nombre, string telefono)
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");

        var resultado = CrearServicio().Editar(new Propietario { Id = ana.Id, NombreCompleto = nombre, Telefono = telefono });

        Assert.False(resultado.Exito);
        var intacto = CrearServicio().ObtenerPorId(ana.Id)!;
        Assert.Equal("Ana Pérez", intacto.NombreCompleto);
        Assert.Equal("3001234567", intacto.Telefono);
    }

    [Fact]
    [Trait("Req", "RF-04")]
    public void RF04_EditarUnPropietarioInexistenteDevuelveError()
    {
        var resultado = CrearServicio().Editar(new Propietario { Id = 999, NombreCompleto = "Nadie", Telefono = "3001234567" });

        Assert.False(resultado.Exito);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_InactivarNoBorraAlPropietarioNiSusMascotas()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");

        var resultado = CrearServicio().Editar(new Propietario
        {
            Id = ana.Id,
            NombreCompleto = "Ana Pérez",
            Telefono = "3001234567",
            Activo = false,
        });

        Assert.True(resultado.Exito);
        var inactivo = Assert.Single(CrearServicio().Buscar("Ana"));
        Assert.False(inactivo.Activo);
        Assert.Single(inactivo.Mascotas);
    }
}
