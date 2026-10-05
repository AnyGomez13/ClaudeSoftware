using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="MascotaEdicionViewModel"/>, la pantalla P-06 (RF-05, RF-07, RF-08, RN-03, RN-06, SUP-04, SUP-08).</summary>
public class MascotaEdicionViewModelTests : PantallaTestBase
{
    private readonly List<bool> _terminaciones = [];

    private MascotaEdicionViewModel Nueva()
    {
        var pantalla = Servicios.GetRequiredService<MascotaEdicionViewModel>();
        pantalla.Nueva(_terminaciones.Add);
        return pantalla;
    }

    private MascotaEdicionViewModel Editar(int mascotaId)
    {
        var pantalla = Servicios.GetRequiredService<MascotaEdicionViewModel>();
        pantalla.Cargar(Bd.CrearContexto().Mascotas.Single(m => m.Id == mascotaId), _terminaciones.Add);
        return pantalla;
    }

    private static void LlenarFormularioValido(MascotaEdicionViewModel pantalla)
    {
        pantalla.PropietarioSeleccionado = pantalla.Propietarios[0];
        pantalla.Nombre = "Luna";
        pantalla.Especie = "Gato";
        pantalla.FechaNacimiento = new DateTime(2023, 5, 20);
        pantalla.PesoTexto = "4,2";
    }

    private Mascota Leer(int id) => Bd.CrearContexto().Mascotas.Single(m => m.Id == id);

    [Fact]
    [Trait("Req", "RN-03")]
    public void RN03_ElSelectorOfreceSoloPropietariosActivosYSinPropietariosLoAvisa()
    {
        var pantallaVacia = Nueva();
        Assert.True(pantallaVacia.NoHayPropietarios);
        Assert.False(pantallaVacia.GuardarCommand.CanExecute(null));

        DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearPropietario(Bd, "Luis Gómez", "3109876543", activo: false);
        var pantalla = Nueva();

        Assert.False(pantalla.NoHayPropietarios);
        Assert.Equal(["Ana Pérez — 3001234567"], pantalla.Propietarios.Select(p => p.Texto).ToList());
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_UnFormularioNuevoTieneTituloPropioYNoOfreceInactivar()
    {
        var pantalla = Nueva();

        Assert.Equal("Nueva mascota", pantalla.Titulo);
        Assert.True(pantalla.EsNuevo);
        Assert.False(pantalla.EsEdicion);
        Assert.False(pantalla.CambiarEstadoCommand.CanExecute(null));
        Assert.Null(pantalla.FechaNacimiento);
        Assert.Equal(MascotaEdicionViewModel.SexoNoEspecificado, pantalla.SexoSeleccionado);
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_GuardarSoloSeHabilitaConPropietarioNombreEspecieFechaYPeso()
    {
        DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var pantalla = Nueva();
        Assert.False(pantalla.GuardarCommand.CanExecute(null));

        pantalla.PropietarioSeleccionado = pantalla.Propietarios[0];
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
        pantalla.Nombre = "Luna";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
        pantalla.Especie = "Gato";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
        pantalla.FechaNacimiento = new DateTime(2023, 5, 20);
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
        pantalla.PesoTexto = "4,2";
        Assert.True(pantalla.GuardarCommand.CanExecute(null));

        pantalla.Nombre = "   ";
        Assert.False(pantalla.GuardarCommand.CanExecute(null));
    }

    [Theory]
    [Trait("Req", "RN-06")]
    [InlineData("12,5", 12.5)]
    [InlineData("12.5", 12.5)]
    [InlineData(" 8 ", 8)]
    [InlineData("0,35", 0.35)]
    public void RN06_ElPesoAceptaComaOPuntoDecimal(string texto, double esperado)
    {
        Assert.True(MascotaEdicionViewModel.TryLeerPeso(texto, out var peso));
        Assert.Equal(esperado, peso);
    }

    [Theory]
    [Trait("Req", "RN-06")]
    [InlineData("abc", true)]
    [InlineData("-3", true)]
    [InlineData("0", true)]
    [InlineData("1,2,3", true)]
    [InlineData("NaN", true)]
    [InlineData("12,5", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void RN06_ElPesoInvalidoSeAvisaEnVivo(string texto, bool debeMostrarError)
    {
        var pantalla = Nueva();

        pantalla.PesoTexto = texto;

        Assert.Equal(debeMostrarError, pantalla.TieneErrorPeso);
        Assert.Equal(debeMostrarError ? MascotaEdicionViewModel.MensajeFormatoPeso : string.Empty, pantalla.ErrorPeso);
    }

    [Fact]
    [Trait("Req", "RF-08")]
    public void RF08_LaEdadSeCalculaEnVivoAlEscribirLaFechaDeNacimiento()
    {
        var pantalla = Nueva();
        var cambios = new List<string?>();
        pantalla.PropertyChanged += (_, e) => cambios.Add(e.PropertyName);
        Assert.Equal("—", pantalla.EdadCalculada);

        pantalla.FechaNacimiento = DateTime.Today.AddYears(-2).AddMonths(-3);

        Assert.Equal("2 años 3 meses", pantalla.EdadCalculada);
        Assert.Contains(nameof(MascotaEdicionViewModel.EdadCalculada), cambios);

        pantalla.FechaNacimiento = DateTime.Today.AddDays(1);
        Assert.Equal(MascotaEdicionViewModel.MensajeFechaFutura, pantalla.EdadCalculada);

        pantalla.FechaNacimiento = null;
        Assert.Equal("—", pantalla.EdadCalculada);
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_GuardarRegistraLaMascotaConUnPropietarioExistenteYAvisaQueTermino()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);
        pantalla.Raza = "  Siamés ";
        pantalla.ColorSenas = "";
        pantalla.SexoSeleccionado = "Hembra";
        pantalla.FechaEstimada = true;

        pantalla.GuardarCommand.Execute(null);

        Assert.Equal([true], _terminaciones);
        var guardada = Bd.CrearContexto().Mascotas.Single();
        Assert.Equal(ana.Id, guardada.PropietarioId);
        Assert.Equal("Luna", guardada.Nombre);
        Assert.Equal("Siamés", guardada.Raza);
        Assert.Equal("Hembra", guardada.Sexo);
        Assert.Equal(new DateTime(2023, 5, 20), guardada.FechaNacimiento);
        Assert.True(guardada.FechaNacimientoEstimada);
        Assert.Equal(4.2, guardada.Peso);
        Assert.Null(guardada.ColorSenas);
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_ElSexoNoEspecificadoSeGuardaComoNulo()
    {
        DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);

        pantalla.GuardarCommand.Execute(null);

        Assert.Null(Bd.CrearContexto().Mascotas.Single().Sexo);
    }

    [Fact]
    [Trait("Req", "RN-06")]
    public void RN06_UnPesoCeroLoRechazaElServicioYElFormularioNoSeCierra()
    {
        DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);
        pantalla.PesoTexto = "0";

        pantalla.GuardarCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Contains("kilogramos", pantalla.MensajeError);
        Assert.Empty(_terminaciones);
        Assert.Empty(Bd.CrearContexto().Mascotas);
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_UnaFechaFuturaLaRechazaElServicio()
    {
        DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var pantalla = Nueva();
        LlenarFormularioValido(pantalla);
        pantalla.FechaNacimiento = DateTime.Today.AddDays(2);

        pantalla.GuardarCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Contains("futura", pantalla.MensajeError);
        Assert.Empty(_terminaciones);
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_EditarCargaLosDatosYGuardaElNuevoPeso()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        var pantalla = Editar(rocky.Id);
        Assert.Equal("Editar mascota", pantalla.Titulo);
        Assert.True(pantalla.EsEdicion);
        Assert.Equal("Rocky", pantalla.Nombre);
        Assert.Equal("Perro", pantalla.Especie);
        Assert.Equal("12,5", pantalla.PesoTexto);
        Assert.Equal(new DateTime(2020, 3, 15), pantalla.FechaNacimiento);
        Assert.Equal("Ana Pérez — 3001234567", pantalla.PropietarioSeleccionado?.Texto);

        pantalla.PesoTexto = "13,2";
        pantalla.GuardarCommand.Execute(null);

        Assert.Equal([true], _terminaciones);
        Assert.Equal(13.2, Leer(rocky.Id).Peso);
        Assert.Equal("Rocky", Leer(rocky.Id).Nombre);
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_SePuedeCambiarElPropietarioDeLaMascota()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var luis = DatosDePrueba.CrearPropietario(Bd, "Luis Gómez", "3109876543");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        var pantalla = Editar(rocky.Id);

        pantalla.PropietarioSeleccionado = pantalla.Propietarios.Single(p => p.Id == luis.Id);
        pantalla.GuardarCommand.Execute(null);

        Assert.Equal(luis.Id, Leer(rocky.Id).PropietarioId);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_AlEditarSeIncluyeAlPropietarioActualAunqueEsteInactivo()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        Bd.Ejecutar($"UPDATE Propietario SET Activo = 0 WHERE Id = {ana.Id};");

        var pantalla = Editar(rocky.Id);

        Assert.Equal("Ana Pérez — 3001234567 (inactivo)", pantalla.PropietarioSeleccionado?.Texto);
        Assert.True(pantalla.GuardarCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_CancelarNoGuardaNadaYAvisaQueSeCancelo()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        var pantalla = Editar(rocky.Id);
        pantalla.PesoTexto = "99";

        pantalla.CancelarCommand.Execute(null);

        Assert.Equal([false], _terminaciones);
        Assert.Equal(12.5, Leer(rocky.Id).Peso);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_InactivarPideConfirmacionYConservaLaMascotaYSuHistoria()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        DatosDePrueba.CrearVacunacion(Bd, rocky.Id, DatosDePrueba.IdDeVeterinario(Bd, "Fabio"), "Rabia", new DateTime(2026, 1, 10));
        var pantalla = Editar(rocky.Id);
        Assert.Equal("Inactivar", pantalla.TextoCambiarEstado);

        pantalla.CambiarEstadoCommand.Execute(null);

        Assert.Single(Dialogos.Confirmaciones);
        Assert.Contains("Rocky", Dialogos.Confirmaciones[0]);
        Assert.Equal([true], _terminaciones);
        Assert.False(Leer(rocky.Id).Activo);
        Assert.Equal(1, Bd.Escalar<int>("SELECT COUNT(*) FROM Vacunacion;"));
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_SiElUsuarioNoConfirmaLaMascotaSigueActiva()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        Dialogos.RespuestaConfirmar = false;
        var pantalla = Editar(rocky.Id);

        pantalla.CambiarEstadoCommand.Execute(null);

        Assert.Empty(_terminaciones);
        Assert.True(Leer(rocky.Id).Activo);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_UnaMascotaInactivaSePuedeReactivarSinConfirmar()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        Bd.Ejecutar($"UPDATE Mascota SET Activo = 0 WHERE Id = {rocky.Id};");
        var pantalla = Editar(rocky.Id);
        Assert.Equal("Reactivar", pantalla.TextoCambiarEstado);

        pantalla.CambiarEstadoCommand.Execute(null);

        Assert.Empty(Dialogos.Confirmaciones);
        Assert.Equal([true], _terminaciones);
        Assert.True(Leer(rocky.Id).Activo);
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_AlReabrirElFormularioNuevoSeLimpianLosDatosAnteriores()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        var pantalla = Editar(rocky.Id);
        pantalla.PesoTexto = "0";
        pantalla.GuardarCommand.Execute(null);
        Assert.True(pantalla.TieneError);

        pantalla.Nueva(_terminaciones.Add);

        Assert.Equal(string.Empty, pantalla.Nombre);
        Assert.Equal(string.Empty, pantalla.PesoTexto);
        Assert.Null(pantalla.FechaNacimiento);
        Assert.Null(pantalla.PropietarioSeleccionado);
        Assert.False(pantalla.TieneError);
        Assert.True(pantalla.EsNuevo);
    }
}
