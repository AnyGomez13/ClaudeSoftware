using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Negocio.Utilidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="MascotasViewModel"/>, la pantalla P-05 (RF-06, RF-08, CU-06).</summary>
public class MascotasViewModelTests : PantallaTestBase
{
    private MascotasViewModel CrearPantalla() => Servicios.GetRequiredService<MascotasViewModel>();

    private void SembrarMascotas()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var luis = DatosDePrueba.CrearPropietario(Bd, "Luis Gómez", "3109876543");
        DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        DatosDePrueba.CrearMascota(Bd, ana.Id, "Misu");
        var firu = DatosDePrueba.CrearMascota(Bd, luis.Id, "Firu");
        Bd.Ejecutar($"UPDATE Mascota SET Activo = 0 WHERE Id = {firu.Id};");
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_AlAbrirListaLasMascotasConEspeciePropietarioEdadYEstado()
    {
        SembrarMascotas();

        var pantalla = CrearPantalla();

        Assert.Equal(["Firu", "Misu", "Rocky"], pantalla.Mascotas.Select(f => f.Nombre).ToList());
        Assert.Equal(["Luis Gómez", "Ana Pérez", "Ana Pérez"], pantalla.Mascotas.Select(f => f.Propietario).ToList());
        Assert.All(pantalla.Mascotas, f => Assert.Equal("Perro", f.Especie));
        Assert.Equal(["Inactiva", "Activa", "Activa"], pantalla.Mascotas.Select(f => f.Estado).ToList());
        Assert.False(pantalla.SinResultados);
    }

    [Fact]
    [Trait("Req", "RF-08")]
    public void RF08_LaEdadDeCadaFilaSeCalculaConLaFechaActual()
    {
        SembrarMascotas();

        var pantalla = CrearPantalla();

        var esperada = new CalculadoraEdad().Legible(new DateTime(2020, 3, 15));
        Assert.All(pantalla.Mascotas, f => Assert.Equal(esperada, f.Edad));
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_BuscarPorNombreDeLaMascotaOPorNombreDelPropietario()
    {
        SembrarMascotas();
        var pantalla = CrearPantalla();

        pantalla.Texto = "Rock";
        Assert.Equal(["Rocky"], pantalla.Mascotas.Select(f => f.Nombre).ToList());

        pantalla.Texto = "Gómez";
        Assert.Equal(["Firu"], pantalla.Mascotas.Select(f => f.Nombre).ToList());

        pantalla.Texto = "Pérez";
        Assert.Equal(["Misu", "Rocky"], pantalla.Mascotas.Select(f => f.Nombre).ToList());

        pantalla.Texto = string.Empty;
        Assert.Equal(3, pantalla.Mascotas.Count);
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_SinCoincidenciasInformaQueNoSeEncontraronMascotas()
    {
        SembrarMascotas();
        var pantalla = CrearPantalla();

        pantalla.Texto = "Zzz";

        Assert.Empty(pantalla.Mascotas);
        Assert.True(pantalla.SinResultados);
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_NuevaMascotaAbreElFormularioVacio()
    {
        SembrarMascotas();
        Navegacion.NavegarASeccion<MascotasViewModel>();
        var lista = Assert.IsType<MascotasViewModel>(Navegacion.ViewModelActual);

        lista.NuevaMascotaCommand.Execute(null);

        var formulario = Assert.IsType<MascotaEdicionViewModel>(Navegacion.ViewModelActual);
        Assert.True(formulario.EsNuevo);
        Assert.Equal(string.Empty, formulario.Nombre);
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_EditarAbreElFormularioConLosDatosDeLaMascota()
    {
        SembrarMascotas();
        Navegacion.NavegarASeccion<MascotasViewModel>();
        var lista = Assert.IsType<MascotasViewModel>(Navegacion.ViewModelActual);

        lista.EditarCommand.Execute(lista.Mascotas.Single(f => f.Nombre == "Rocky"));

        var formulario = Assert.IsType<MascotaEdicionViewModel>(Navegacion.ViewModelActual);
        Assert.False(formulario.EsNuevo);
        Assert.Equal("Rocky", formulario.Nombre);
        Assert.Equal("Ana Pérez — 3001234567", formulario.PropietarioSeleccionado?.Texto);
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_AbrirFichaMuestraLaFichaDeLaMascotaSeleccionada()
    {
        SembrarMascotas();
        Navegacion.NavegarASeccion<MascotasViewModel>();
        var lista = Assert.IsType<MascotasViewModel>(Navegacion.ViewModelActual);

        lista.AbrirFichaCommand.Execute(lista.Mascotas.Single(f => f.Nombre == "Misu"));

        var ficha = Assert.IsType<MascotaDetalleViewModel>(Navegacion.ViewModelActual);
        Assert.True(ficha.Encontrada);
        Assert.Equal("Misu", ficha.Nombre);
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_AlGuardarUnaMascotaNuevaSeVuelveALaListaYApareceAhi()
    {
        DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        Navegacion.NavegarASeccion<MascotasViewModel>();
        var lista = Assert.IsType<MascotasViewModel>(Navegacion.ViewModelActual);
        Assert.True(lista.SinResultados);

        lista.NuevaMascotaCommand.Execute(null);
        var formulario = Assert.IsType<MascotaEdicionViewModel>(Navegacion.ViewModelActual);
        formulario.PropietarioSeleccionado = formulario.Propietarios[0];
        formulario.Nombre = "Luna";
        formulario.Especie = "Gato";
        formulario.FechaNacimiento = DateTime.Today.AddYears(-1);
        formulario.PesoTexto = "4,2";
        formulario.GuardarCommand.Execute(null);

        Assert.Same(lista, Navegacion.ViewModelActual);
        Assert.Equal(["Luna"], lista.Mascotas.Select(f => f.Nombre).ToList());
        Assert.Equal("1 año", lista.Mascotas[0].Edad);
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_AlCancelarLaEdicionSeVuelveALaListaSinCambios()
    {
        SembrarMascotas();
        Navegacion.NavegarASeccion<MascotasViewModel>();
        var lista = Assert.IsType<MascotasViewModel>(Navegacion.ViewModelActual);
        lista.EditarCommand.Execute(lista.Mascotas[0]);
        var formulario = Assert.IsType<MascotaEdicionViewModel>(Navegacion.ViewModelActual);
        formulario.Nombre = "Cambiado";

        formulario.CancelarCommand.Execute(null);

        Assert.Same(lista, Navegacion.ViewModelActual);
        Assert.DoesNotContain(lista.Mascotas, f => f.Nombre == "Cambiado");
    }
}
