using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>
/// Pruebas del listado de vacunas y del refuerzo propuesto en <see cref="VacunacionEdicionViewModel"/> (P-09):
/// la fecha se propone según la vacuna y el usuario puede cambiarla (RF-11, SUP-10).
/// </summary>
public class VacunacionRefuerzoSugeridoTests : PantallaTestBase
{
    private int _mascotaId;

    private VacunacionEdicionViewModel Nueva(string especie = "Perro")
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        _mascotaId = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky").Id;
        var pantalla = Servicios.GetRequiredService<VacunacionEdicionViewModel>();
        pantalla.Nueva(_mascotaId, "Rocky", null, especie);
        return pantalla;
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_ElListadoSeFiltraSegunLaEspecieDeLaMascota()
    {
        var perro = Nueva("Perro");
        Assert.Contains("Parvovirus", perro.VacunasSugeridas);
        Assert.Contains("Rabia", perro.VacunasSugeridas);
        Assert.DoesNotContain("Triple felina", perro.VacunasSugeridas);

        var gato = Nueva("Gato");
        Assert.Contains("Triple felina", gato.VacunasSugeridas);
        Assert.Contains("Rabia", gato.VacunasSugeridas);
        Assert.DoesNotContain("Parvovirus", gato.VacunasSugeridas);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_AlElegirUnaVacunaDelListadoSeProponeElRefuerzoSegunLaVacuna()
    {
        var pantalla = Nueva();
        pantalla.FechaAplicacion = new DateTime(2026, 10, 5);
        Assert.Null(pantalla.ProximaFecha);

        pantalla.NombreVacuna = "Rabia";
        Assert.Equal(new DateTime(2027, 10, 5), pantalla.ProximaFecha);

        pantalla.NombreVacuna = "Bordetella";
        Assert.Equal(new DateTime(2027, 4, 5), pantalla.ProximaFecha);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_AlCambiarLaFechaDeAplicacionElRefuerzoPropuestoLaSigue()
    {
        var pantalla = Nueva();
        pantalla.NombreVacuna = "Rabia";
        pantalla.FechaAplicacion = new DateTime(2026, 1, 10);
        Assert.Equal(new DateTime(2027, 1, 10), pantalla.ProximaFecha);

        pantalla.FechaAplicacion = new DateTime(2026, 3, 20);

        Assert.Equal(new DateTime(2027, 3, 20), pantalla.ProximaFecha);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_LaFechaDeHoyEsLaAplicacionPorDefectoYElRefuerzoSeCalculaDesdeEsa()
    {
        var pantalla = Nueva();

        pantalla.NombreVacuna = "Parvovirus";

        Assert.Equal(DateTime.Today.AddMonths(12), pantalla.ProximaFecha);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_ElUsuarioPuedeCambiarLaFechaPropuestaYYaNoSeLeSobrescribe()
    {
        var pantalla = Nueva();
        pantalla.FechaAplicacion = new DateTime(2026, 10, 5);
        pantalla.NombreVacuna = "Rabia";

        pantalla.ProximaFecha = new DateTime(2027, 1, 15);
        pantalla.NombreVacuna = "Bordetella";
        pantalla.FechaAplicacion = new DateTime(2026, 11, 1);

        Assert.Equal(new DateTime(2027, 1, 15), pantalla.ProximaFecha);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_UsuarioQueBorraLaFechaPropuestaLaDejaVacia()
    {
        var pantalla = Nueva();
        pantalla.FechaAplicacion = new DateTime(2026, 10, 5);
        pantalla.NombreVacuna = "Rabia";
        Assert.NotNull(pantalla.ProximaFecha);

        pantalla.ProximaFecha = null;
        pantalla.NombreVacuna = "Parvovirus";

        Assert.Null(pantalla.ProximaFecha);
    }

    [Fact]
    [Trait("Req", "SUP-10")]
    public void SUP10_UnaVacunaFueraDelListadoNoProponeFechaYLaQuitaSiLaHabiaPropuesto()
    {
        var pantalla = Nueva();
        pantalla.FechaAplicacion = new DateTime(2026, 10, 5);
        pantalla.NombreVacuna = "Rabia";
        Assert.NotNull(pantalla.ProximaFecha);

        pantalla.NombreVacuna = "Giardia";

        Assert.Null(pantalla.ProximaFecha);
        Assert.False(pantalla.HaySugerenciaRefuerzo);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_SinFechaDeAplicacionNoSePropone()
    {
        var pantalla = Nueva();
        pantalla.NombreVacuna = "Rabia";
        Assert.NotNull(pantalla.ProximaFecha);

        pantalla.FechaAplicacion = null;

        Assert.Null(pantalla.ProximaFecha);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_ElAvisoDiceCadaCuantosMesesSeRefuerza()
    {
        var pantalla = Nueva();

        pantalla.NombreVacuna = "Bordetella";
        Assert.True(pantalla.HaySugerenciaRefuerzo);
        Assert.Contains("cada 6 meses", pantalla.SugerenciaRefuerzo);

        pantalla.NombreVacuna = "rabia";
        Assert.Contains("cada 12 meses", pantalla.SugerenciaRefuerzo);

        pantalla.NombreVacuna = "";
        Assert.False(pantalla.HaySugerenciaRefuerzo);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_LaFechaPropuestaSeGuardaComoRefuerzoDeLaVacunacion()
    {
        var pantalla = Nueva();
        pantalla.VeterinarioSeleccionado = pantalla.Veterinarios[0];
        pantalla.FechaAplicacion = new DateTime(2026, 10, 5);
        pantalla.NombreVacuna = "Rabia";

        pantalla.GuardarCommand.Execute(null);

        Assert.Equal(new DateTime(2027, 10, 5), Bd.CrearContexto().Vacunaciones.Single().ProximaFecha);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_AlAbrirUnFormularioNuevoSeOlvidaLaFechaEscritaAntes()
    {
        var pantalla = Nueva();
        pantalla.FechaAplicacion = new DateTime(2026, 10, 5);
        pantalla.NombreVacuna = "Rabia";
        pantalla.ProximaFecha = new DateTime(2027, 1, 15);

        pantalla.Nueva(_mascotaId, "Rocky", null, "Perro");
        pantalla.NombreVacuna = "Rabia";

        Assert.Equal(DateTime.Today.AddMonths(12), pantalla.ProximaFecha);
    }
}
