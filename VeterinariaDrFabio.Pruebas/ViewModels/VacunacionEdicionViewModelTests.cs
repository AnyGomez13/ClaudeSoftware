using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="VacunacionEdicionViewModel"/>, la pantalla P-09 (RF-11, RN-07, RN-08, SUP-10).</summary>
public class VacunacionEdicionViewModelTests : PantallaTestBase
{
    private readonly List<bool> _terminaciones = [];
    private int _mascotaId;

    private VacunacionEdicionViewModel Nueva()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        _mascotaId = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky").Id;
        var pantalla = Servicios.GetRequiredService<VacunacionEdicionViewModel>();
        pantalla.Nueva(_mascotaId, "Rocky", _terminaciones.Add);
        return pantalla;
    }

    private static void LlenarFormularioValido(VacunacionEdicionViewModel pantalla)
    {
        pantalla.VeterinarioSeleccionado = pantalla.Veterinarios.Single(v => v.Texto == "Fabio");
        pantalla.NombreVacuna = "Rabia";
        pantalla.FechaAplicacion = new DateTime(2026, 10, 4);
    }

    private Vacunacion Guardada() => Bd.CrearContexto().Vacunaciones.Single();

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_UnFormularioNuevoTraeLosVeterinariosActivosYLaFechaDeHoy()
    {
        var pantalla = Nueva();

        Assert.Equal("Registrar vacuna — Rocky", pantalla.Titulo);
        Assert.Equal(["Fabio", "William"], pantalla.Veterinarios.Select(v => v.Texto).ToList());
        Assert.Null(pantalla.VeterinarioSeleccionado);
        Assert.Equal(DateTime.Today, pantalla.FechaAplicacion);
        Assert.Contains("Rabia", pantalla.VacunasSugeridas);
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RN-07")]
    public void RN07_UnVeterinarioInactivoNoApareceEnElSelector()
    {
        Bd.Ejecutar("UPDATE Veterinario SET Activo = 0 WHERE NombreCompleto = 'Fabio';");

        var pantalla = Nueva();

        Assert.Equal(["William"], pantalla.Veterinarios.Select(v => v.Texto).ToList());
    }

    [Fact]
    [Trait("Req", "RN-07")]
    public void RN07_GuardarSoloSeHabilitaConVeterinarioVacunaYFecha()
    {
        var pantalla = Nueva();
        Assert.False(pantalla.GuardarCommand.CanExecute(null));

        pantalla.NombreVacuna = "Rabia";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));

        pantalla.VeterinarioSeleccionado = pantalla.Veterinarios[0];
        Assert.True(pantalla.GuardarCommand.CanExecute(null));

        pantalla.FechaAplicacion = null;
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
        pantalla.FechaAplicacion = DateTime.Today;
        pantalla.NombreVacuna = "   ";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_GuardarRegistraLaVacunaConSuVeterinarioYElRefuerzo()
    {
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);
        pantalla.ProximaFecha = new DateTime(2027, 10, 4);
        pantalla.Lote = "  L-2026-01 ";
        pantalla.Observaciones = "";

        pantalla.GuardarCommand.Execute(null);

        Assert.Equal([true], _terminaciones);
        var guardada = Guardada();
        Assert.Equal(_mascotaId, guardada.MascotaId);
        Assert.Equal(DatosDePrueba.IdDeVeterinario(Bd, "Fabio"), guardada.VeterinarioId);
        Assert.Equal("Rabia", guardada.NombreVacuna);
        Assert.Equal(new DateTime(2026, 10, 4), guardada.FechaAplicacion);
        Assert.Equal(new DateTime(2027, 10, 4), guardada.ProximaFecha);
        Assert.Equal("L-2026-01", guardada.Lote);
        Assert.Null(guardada.Observaciones);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_ElRefuerzoEsOpcional()
    {
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);

        pantalla.GuardarCommand.Execute(null);

        Assert.Equal([true], _terminaciones);
        Assert.Null(Guardada().ProximaFecha);
    }

    [Fact]
    [Trait("Req", "SUP-10")]
    public void SUP10_LaVacunaAdmiteTextoLibreAunqueNoEsteEnLasSugerencias()
    {
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);
        pantalla.NombreVacuna = "Bordetella";

        pantalla.GuardarCommand.Execute(null);

        Assert.Equal("Bordetella", Guardada().NombreVacuna);
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_UnRefuerzoAnteriorALaAplicacionLoRechazaElServicio()
    {
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);
        pantalla.ProximaFecha = new DateTime(2026, 10, 1);

        pantalla.GuardarCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Contains("anterior", pantalla.MensajeError);
        Assert.Empty(_terminaciones);
        Assert.Empty(Bd.CrearContexto().Vacunaciones);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_UnaFechaDeAplicacionFuturaLaRechazaElServicio()
    {
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);
        pantalla.FechaAplicacion = DateTime.Today.AddDays(3);

        pantalla.GuardarCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Contains("futura", pantalla.MensajeError);
        Assert.Empty(_terminaciones);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_CancelarNoGuardaNadaYAvisaQueSeCancelo()
    {
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);

        pantalla.CancelarCommand.Execute(null);

        Assert.Equal([false], _terminaciones);
        Assert.Empty(Bd.CrearContexto().Vacunaciones);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_AlReabrirElFormularioSeLimpianLosDatosAnteriores()
    {
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);
        pantalla.ProximaFecha = new DateTime(2026, 10, 1);
        pantalla.GuardarCommand.Execute(null);
        Assert.True(pantalla.TieneError);

        pantalla.Nueva(_mascotaId, "Rocky", _terminaciones.Add);

        Assert.Null(pantalla.VeterinarioSeleccionado);
        Assert.Equal(string.Empty, pantalla.NombreVacuna);
        Assert.Null(pantalla.ProximaFecha);
        Assert.Equal(DateTime.Today, pantalla.FechaAplicacion);
        Assert.False(pantalla.TieneError);
    }
}
