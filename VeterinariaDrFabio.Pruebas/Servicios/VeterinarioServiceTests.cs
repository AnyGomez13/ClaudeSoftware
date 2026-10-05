using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="VeterinarioService"/> (R-03, RF-16, SUP-06).</summary>
public class VeterinarioServiceTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private VeterinarioService CrearServicio() => new(new VeterinarioRepository(_bd.CrearContexto()));

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_ListarActivosDevuelveAFabioYWilliamPreRegistrados()
    {
        var nombres = CrearServicio().ListarActivos().Select(v => v.NombreCompleto).ToList();

        Assert.Equal(["Fabio", "William"], nombres);
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_RegistrarAgregaUnVeterinarioDisponibleParaSeleccionar()
    {
        var resultado = CrearServicio().Registrar(new Veterinario { NombreCompleto = "  Camila Rojas  ", RegistroProfesional = " MV-123 " });

        Assert.True(resultado.Exito);
        var camila = Assert.Single(CrearServicio().ListarActivos(), v => v.NombreCompleto == "Camila Rojas");
        Assert.Equal("MV-123", camila.RegistroProfesional);
    }

    [Theory]
    [Trait("Req", "RF-16")]
    [InlineData("")]
    [InlineData("   ")]
    public void RF16_RegistrarSinNombreNoGuarda(string nombre)
    {
        var resultado = CrearServicio().Registrar(new Veterinario { NombreCompleto = nombre });

        Assert.False(resultado.Exito);
        Assert.NotEmpty(resultado.Mensaje);
        Assert.Equal(2, _bd.Escalar<int>("SELECT COUNT(*) FROM Veterinario;"));
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_RegistroProfesionalEnBlancoSeGuardaComoNulo()
    {
        CrearServicio().Registrar(new Veterinario { NombreCompleto = "Camila Rojas", RegistroProfesional = "   " });

        Assert.Null(_bd.Escalar<string>("SELECT RegistroProfesional FROM Veterinario WHERE NombreCompleto = 'Camila Rojas';"));
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_EditarActualizaDatosEInactivaSinBorrar()
    {
        var servicio = CrearServicio();
        var william = servicio.ListarTodos().Single(v => v.NombreCompleto == "William");

        var resultado = servicio.Editar(new Veterinario
        {
            Id = william.Id,
            NombreCompleto = "William Torres",
            RegistroProfesional = "MV-999",
            Activo = false,
        });

        Assert.True(resultado.Exito);
        var lectura = CrearServicio();
        Assert.Equal(["Fabio"], lectura.ListarActivos().Select(v => v.NombreCompleto).ToList());
        var editado = lectura.ListarTodos().Single(v => v.Id == william.Id);
        Assert.Equal("William Torres", editado.NombreCompleto);
        Assert.Equal("MV-999", editado.RegistroProfesional);
        Assert.False(editado.Activo);
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_EditarRechazaNombreVacioYVeterinarioInexistente()
    {
        var servicio = CrearServicio();
        var fabio = servicio.ListarTodos().Single(v => v.NombreCompleto == "Fabio");

        Assert.False(servicio.Editar(new Veterinario { Id = fabio.Id, NombreCompleto = " " }).Exito);
        Assert.False(servicio.Editar(new Veterinario { Id = 999, NombreCompleto = "Nadie" }).Exito);
        Assert.Equal("Fabio", CrearServicio().ListarTodos().Single(v => v.Id == fabio.Id).NombreCompleto);
    }
}
