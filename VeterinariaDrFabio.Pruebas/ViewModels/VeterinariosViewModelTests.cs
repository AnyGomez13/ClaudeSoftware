using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="VeterinariosViewModel"/>, la pantalla P-12 (R-03, RF-16, SUP-06, CU-13).</summary>
public class VeterinariosViewModelTests : PantallaTestBase
{
    private VeterinariosViewModel CrearPantalla() => Servicios.GetRequiredService<VeterinariosViewModel>();

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_ListaAFabioYWilliamPreRegistradosConSuEstado()
    {
        var pantalla = CrearPantalla();

        Assert.Equal(["Fabio", "William"], pantalla.Veterinarios.Select(f => f.NombreCompleto).ToList());
        Assert.Equal(["Activo", "Activo"], pantalla.Veterinarios.Select(f => f.Estado).ToList());
        Assert.Equal(["—", "—"], pantalla.Veterinarios.Select(f => f.RegistroProfesional).ToList());
        Assert.False(pantalla.EnEdicion);
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_NuevoAbreElPanelVacioYGuardarSoloSeHabilitaConNombre()
    {
        var pantalla = CrearPantalla();

        pantalla.NuevoCommand.Execute(null);

        Assert.True(pantalla.EnEdicion);
        Assert.Equal("Nuevo veterinario", pantalla.TituloFormulario);
        Assert.False(pantalla.PuedeCambiarEstado);
        Assert.False(pantalla.GuardarCommand.CanExecute(null));

        pantalla.NombreCompleto = "   ";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));

        pantalla.NombreCompleto = "Camila Rojas";
        Assert.True(pantalla.GuardarCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_GuardarUnVeterinarioNuevoLoAgregaALaListaYCierraElPanel()
    {
        var pantalla = CrearPantalla();
        pantalla.NuevoCommand.Execute(null);
        pantalla.NombreCompleto = "Camila Rojas";
        pantalla.RegistroProfesional = "MV-123";

        pantalla.GuardarCommand.Execute(null);

        Assert.False(pantalla.EnEdicion);
        Assert.Equal(["Camila Rojas", "Fabio", "William"], pantalla.Veterinarios.Select(f => f.NombreCompleto).ToList());
        Assert.Equal("MV-123", pantalla.Veterinarios.Single(f => f.NombreCompleto == "Camila Rojas").RegistroProfesional);
        Assert.Equal(3, Bd.Escalar<int>("SELECT COUNT(*) FROM Veterinario;"));
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_EditarCargaLosDatosDelVeterinarioYPermiteCambiarElEstado()
    {
        var pantalla = CrearPantalla();
        var william = pantalla.Veterinarios.Single(f => f.NombreCompleto == "William");

        pantalla.EditarCommand.Execute(william);

        Assert.True(pantalla.EnEdicion);
        Assert.Equal("Editar veterinario", pantalla.TituloFormulario);
        Assert.True(pantalla.PuedeCambiarEstado);
        Assert.Equal("William", pantalla.NombreCompleto);
        Assert.True(pantalla.Activo);

        pantalla.RegistroProfesional = "MV-999";
        pantalla.Activo = false;
        pantalla.GuardarCommand.Execute(null);

        var editado = pantalla.Veterinarios.Single(f => f.Id == william.Id);
        Assert.Equal("MV-999", editado.RegistroProfesional);
        Assert.Equal("Inactivo", editado.Estado);
        Assert.Equal(2, Bd.Escalar<int>("SELECT COUNT(*) FROM Veterinario;"));
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_CancelarCierraElPanelSinGuardar()
    {
        var pantalla = CrearPantalla();
        pantalla.NuevoCommand.Execute(null);
        pantalla.NombreCompleto = "No se guarda";

        pantalla.CancelarCommand.Execute(null);

        Assert.False(pantalla.EnEdicion);
        Assert.Equal(2, pantalla.Veterinarios.Count);
        Assert.Equal(2, Bd.Escalar<int>("SELECT COUNT(*) FROM Veterinario;"));
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_UnVeterinarioNuevoNaceActivoYQuedaDisponibleParaSeleccionar()
    {
        var pantalla = CrearPantalla();
        pantalla.NuevoCommand.Execute(null);
        pantalla.NombreCompleto = "Camila Rojas";
        pantalla.GuardarCommand.Execute(null);

        var veterinarios = Servicios.GetRequiredService<VeterinariaDrFabio.Negocio.Servicios.IVeterinarioService>();

        Assert.Contains(veterinarios.ListarActivos(), v => v.NombreCompleto == "Camila Rojas");
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_AlAbrirUnPanelNuevoDespuesDeEditarSeLimpianLosDatos()
    {
        var pantalla = CrearPantalla();
        pantalla.EditarCommand.Execute(pantalla.Veterinarios[0]);
        Assert.Equal("Fabio", pantalla.NombreCompleto);

        pantalla.NuevoCommand.Execute(null);

        Assert.Equal(string.Empty, pantalla.NombreCompleto);
        Assert.True(pantalla.Activo);
        Assert.False(pantalla.PuedeCambiarEstado);
    }
}
