using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="PropietariosViewModel"/>, la pantalla P-03 (RF-03, CU-03).</summary>
public class PropietariosViewModelTests : PantallaTestBase
{
    private PropietariosViewModel CrearPantalla() => Servicios.GetRequiredService<PropietariosViewModel>();

    private void SembrarDosPropietarios()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearPropietario(Bd, "Luis Gómez", "3109876543", activo: false);
        DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        DatosDePrueba.CrearMascota(Bd, ana.Id, "Misu");
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_AlAbrirListaTodosLosPropietariosConMascotasYEstado()
    {
        SembrarDosPropietarios();

        var pantalla = CrearPantalla();

        Assert.Equal(["Ana Pérez", "Luis Gómez"], pantalla.Propietarios.Select(f => f.NombreCompleto).ToList());
        Assert.Equal([2, 0], pantalla.Propietarios.Select(f => f.NumeroMascotas).ToList());
        Assert.Equal(["Activo", "Inactivo"], pantalla.Propietarios.Select(f => f.Estado).ToList());
        Assert.False(pantalla.SinResultados);
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_BuscarPorNombreOTelefonoFiltraMientrasSeEscribe()
    {
        SembrarDosPropietarios();
        var pantalla = CrearPantalla();

        pantalla.Texto = "Pérez";
        Assert.Equal(["Ana Pérez"], pantalla.Propietarios.Select(f => f.NombreCompleto).ToList());

        pantalla.Texto = "3109876543";
        Assert.Equal(["Luis Gómez"], pantalla.Propietarios.Select(f => f.NombreCompleto).ToList());

        pantalla.Texto = string.Empty;
        Assert.Equal(2, pantalla.Propietarios.Count);
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_SinCoincidenciasInformaQueNoSeEncontraronPropietarios()
    {
        SembrarDosPropietarios();
        var pantalla = CrearPantalla();

        pantalla.Texto = "Zzz";

        Assert.Empty(pantalla.Propietarios);
        Assert.True(pantalla.SinResultados);
        Assert.False(pantalla.HaySeleccion);
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_AlSeleccionarUnPropietarioSeListanSusMascotas()
    {
        SembrarDosPropietarios();
        var pantalla = CrearPantalla();
        Assert.False(pantalla.HaySeleccion);
        Assert.Empty(pantalla.Mascotas);

        pantalla.Seleccionado = pantalla.Propietarios[0];

        Assert.True(pantalla.HaySeleccion);
        Assert.Equal("Mascotas de Ana Pérez", pantalla.TituloMascotas);
        Assert.Equal(["Misu", "Rocky"], pantalla.Mascotas.Select(m => m.Nombre).ToList());
        Assert.All(pantalla.Mascotas, m => Assert.Equal("Perro", m.Especie));
        Assert.False(pantalla.SeleccionSinMascotas);
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_UnPropietarioSinMascotasLoIndica()
    {
        SembrarDosPropietarios();
        var pantalla = CrearPantalla();

        pantalla.VerCommand.Execute(pantalla.Propietarios[1]);

        Assert.Same(pantalla.Propietarios[1], pantalla.Seleccionado);
        Assert.Empty(pantalla.Mascotas);
        Assert.True(pantalla.SeleccionSinMascotas);
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_LaSeleccionSeConservaAlFiltrarSiElPropietarioSigueEnLaLista()
    {
        SembrarDosPropietarios();
        var pantalla = CrearPantalla();
        pantalla.Seleccionado = pantalla.Propietarios[0];

        pantalla.Texto = "Ana";
        Assert.Equal("Ana Pérez", pantalla.Seleccionado?.NombreCompleto);

        pantalla.Texto = "Luis";
        Assert.Null(pantalla.Seleccionado);
        Assert.Empty(pantalla.Mascotas);
    }

    [Fact]
    [Trait("Req", "RF-02")]
    public void RF02_NuevoPropietarioAbreElFormularioVacio()
    {
        var pantalla = CrearPantalla();
        Navegacion.NavegarASeccion<PropietariosViewModel>();

        pantalla.NuevoPropietarioCommand.Execute(null);

        var formulario = Assert.IsType<PropietarioEdicionViewModel>(Navegacion.ViewModelActual);
        Assert.True(formulario.EsNuevo);
        Assert.Equal(string.Empty, formulario.NombreCompleto);
    }

    [Fact]
    [Trait("Req", "RF-04")]
    public void RF04_EditarAbreElFormularioConLosDatosDelPropietario()
    {
        SembrarDosPropietarios();
        var pantalla = CrearPantalla();

        pantalla.EditarCommand.Execute(pantalla.Propietarios[0]);

        var formulario = Assert.IsType<PropietarioEdicionViewModel>(Navegacion.ViewModelActual);
        Assert.False(formulario.EsNuevo);
        Assert.Equal("Ana Pérez", formulario.NombreCompleto);
        Assert.Equal("3001234567", formulario.Telefono);
    }

    [Fact]
    [Trait("Req", "RF-02")]
    public void RF02_AlGuardarUnPropietarioNuevoSeVuelveALaListaYApareceAhi()
    {
        Navegacion.NavegarASeccion<PropietariosViewModel>();
        var lista = Assert.IsType<PropietariosViewModel>(Navegacion.ViewModelActual);

        lista.NuevoPropietarioCommand.Execute(null);
        var formulario = Assert.IsType<PropietarioEdicionViewModel>(Navegacion.ViewModelActual);
        formulario.NombreCompleto = "María Torres";
        formulario.Telefono = "3151112233";
        formulario.GuardarCommand.Execute(null);

        Assert.Same(lista, Navegacion.ViewModelActual);
        Assert.Equal(["María Torres"], lista.Propietarios.Select(f => f.NombreCompleto).ToList());
    }

    [Fact]
    [Trait("Req", "RF-04")]
    public void RF04_AlCancelarSeVuelveALaListaSinCambios()
    {
        SembrarDosPropietarios();
        Navegacion.NavegarASeccion<PropietariosViewModel>();
        var lista = Assert.IsType<PropietariosViewModel>(Navegacion.ViewModelActual);
        lista.EditarCommand.Execute(lista.Propietarios[0]);
        var formulario = Assert.IsType<PropietarioEdicionViewModel>(Navegacion.ViewModelActual);
        formulario.NombreCompleto = "Otro nombre";

        formulario.CancelarCommand.Execute(null);

        Assert.Same(lista, Navegacion.ViewModelActual);
        Assert.Equal("Ana Pérez", lista.Propietarios[0].NombreCompleto);
    }
}
