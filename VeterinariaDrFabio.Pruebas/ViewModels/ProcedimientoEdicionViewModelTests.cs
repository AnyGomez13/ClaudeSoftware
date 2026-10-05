using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="ProcedimientoEdicionViewModel"/>, la pantalla P-08 (RF-09, RN-07, RN-08, SUP-05).</summary>
public class ProcedimientoEdicionViewModelTests : PantallaTestBase
{
    private readonly List<bool> _terminaciones = [];
    private int _mascotaId;

    private ProcedimientoEdicionViewModel Nuevo()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        _mascotaId = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky").Id;
        var pantalla = Servicios.GetRequiredService<ProcedimientoEdicionViewModel>();
        pantalla.Nuevo(_mascotaId, "Rocky", _terminaciones.Add);
        return pantalla;
    }

    private static void LlenarFormularioValido(ProcedimientoEdicionViewModel pantalla)
    {
        pantalla.VeterinarioSeleccionado = pantalla.Veterinarios.Single(v => v.Texto == "William");
        pantalla.Fecha = new DateTime(2026, 10, 4);
        pantalla.TipoProcedimiento = "Consulta";
        pantalla.Descripcion = "Control general sin novedades";
    }

    private Procedimiento Guardado() => Bd.CrearContexto().Procedimientos.Single();

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_UnFormularioNuevoTraeLosVeterinariosActivosYLaFechaDeHoy()
    {
        var pantalla = Nuevo();

        Assert.Equal("Nuevo procedimiento — Rocky", pantalla.Titulo);
        Assert.Equal(["Fabio", "William"], pantalla.Veterinarios.Select(v => v.Texto).ToList());
        Assert.Null(pantalla.VeterinarioSeleccionado);
        Assert.Equal(DateTime.Today, pantalla.Fecha);
        Assert.Contains("Desparasitación", pantalla.TiposSugeridos);
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RN-07")]
    public void RN07_UnVeterinarioInactivoNoApareceEnElSelector()
    {
        Bd.Ejecutar("UPDATE Veterinario SET Activo = 0 WHERE NombreCompleto = 'William';");

        var pantalla = Nuevo();

        Assert.Equal(["Fabio"], pantalla.Veterinarios.Select(v => v.Texto).ToList());
    }

    [Fact]
    [Trait("Req", "RN-07")]
    public void RN07_GuardarSoloSeHabilitaConVeterinarioFechaTipoYDescripcion()
    {
        var pantalla = Nuevo();
        Assert.False(pantalla.GuardarCommand.CanExecute(null));

        pantalla.TipoProcedimiento = "Consulta";
        pantalla.Descripcion = "Control";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));

        pantalla.VeterinarioSeleccionado = pantalla.Veterinarios[0];
        Assert.True(pantalla.GuardarCommand.CanExecute(null));

        pantalla.Fecha = null;
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
        pantalla.Fecha = DateTime.Today;
        pantalla.Descripcion = "   ";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
        pantalla.Descripcion = "Control";
        pantalla.TipoProcedimiento = "";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_GuardarRegistraElProcedimientoConSuVeterinarioYAvisaQueTermino()
    {
        var pantalla = Nuevo();
        LlenarFormularioValido(pantalla);
        pantalla.Tratamiento = "  ";
        pantalla.PesoTexto = "12,8";
        pantalla.ProximaFecha = new DateTime(2027, 1, 4);

        pantalla.GuardarCommand.Execute(null);

        Assert.Equal([true], _terminaciones);
        var guardado = Guardado();
        Assert.Equal(_mascotaId, guardado.MascotaId);
        Assert.Equal(DatosDePrueba.IdDeVeterinario(Bd, "William"), guardado.VeterinarioId);
        Assert.Equal(new DateTime(2026, 10, 4), guardado.Fecha);
        Assert.Equal("Consulta", guardado.TipoProcedimiento);
        Assert.Equal("Control general sin novedades", guardado.Descripcion);
        Assert.Null(guardado.Tratamiento);
        Assert.Equal(12.8, guardado.PesoEnElMomento);
        Assert.Equal(new DateTime(2027, 1, 4), guardado.ProximaFechaRecomendada);
    }

    [Fact]
    [Trait("Req", "SUP-05")]
    public void SUP05_ElPesoEsOpcional()
    {
        var pantalla = Nuevo();
        LlenarFormularioValido(pantalla);

        pantalla.GuardarCommand.Execute(null);

        Assert.Equal([true], _terminaciones);
        Assert.Null(Guardado().PesoEnElMomento);
        Assert.Null(Guardado().ProximaFechaRecomendada);
    }

    [Theory]
    [Trait("Req", "RN-06")]
    [InlineData("abc", true)]
    [InlineData("0", true)]
    [InlineData("-2", true)]
    [InlineData("12.5", false)]
    [InlineData("", false)]
    public void RN06_ElPesoInvalidoSeAvisaEnVivo(string texto, bool debeMostrarError)
    {
        var pantalla = Nuevo();

        pantalla.PesoTexto = texto;

        Assert.Equal(debeMostrarError, pantalla.TieneErrorPeso);
    }

    [Fact]
    [Trait("Req", "RN-06")]
    public void RN06_UnPesoInvalidoLoRechazaElServicioYElFormularioNoSeCierra()
    {
        var pantalla = Nuevo();
        LlenarFormularioValido(pantalla);
        pantalla.PesoTexto = "abc";

        pantalla.GuardarCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Contains("kilogramos", pantalla.MensajeError);
        Assert.Empty(_terminaciones);
        Assert.Empty(Bd.CrearContexto().Procedimientos);
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_UnaProximaFechaAnteriorALaDelProcedimientoLaRechazaElServicio()
    {
        var pantalla = Nuevo();
        LlenarFormularioValido(pantalla);
        pantalla.ProximaFecha = new DateTime(2026, 10, 1);

        pantalla.GuardarCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Contains("anterior", pantalla.MensajeError);
        Assert.Empty(_terminaciones);
        Assert.Empty(Bd.CrearContexto().Procedimientos);
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_UnaFechaFuturaLaRechazaElServicio()
    {
        var pantalla = Nuevo();
        LlenarFormularioValido(pantalla);
        pantalla.Fecha = DateTime.Today.AddDays(3);

        pantalla.GuardarCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Contains("futura", pantalla.MensajeError);
        Assert.Empty(_terminaciones);
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_CancelarNoGuardaNadaYAvisaQueSeCancelo()
    {
        var pantalla = Nuevo();
        LlenarFormularioValido(pantalla);

        pantalla.CancelarCommand.Execute(null);

        Assert.Equal([false], _terminaciones);
        Assert.Empty(Bd.CrearContexto().Procedimientos);
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_AlReabrirElFormularioSeLimpianLosDatosAnteriores()
    {
        var pantalla = Nuevo();
        LlenarFormularioValido(pantalla);
        pantalla.PesoTexto = "abc";
        pantalla.GuardarCommand.Execute(null);
        Assert.True(pantalla.TieneError);

        pantalla.Nuevo(_mascotaId, "Rocky", _terminaciones.Add);

        Assert.Null(pantalla.VeterinarioSeleccionado);
        Assert.Equal(string.Empty, pantalla.Descripcion);
        Assert.Equal(string.Empty, pantalla.PesoTexto);
        Assert.Equal(DateTime.Today, pantalla.Fecha);
        Assert.False(pantalla.TieneError);
    }
}
