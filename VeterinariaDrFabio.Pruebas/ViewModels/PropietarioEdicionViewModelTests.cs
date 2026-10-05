using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="PropietarioEdicionViewModel"/>, la pantalla P-04 (RF-02, RF-04, RN-04, RN-14, SUP-08).</summary>
public class PropietarioEdicionViewModelTests : PantallaTestBase
{
    private readonly List<bool> _terminaciones = [];

    private PropietarioEdicionViewModel Nuevo()
    {
        var pantalla = Servicios.GetRequiredService<PropietarioEdicionViewModel>();
        pantalla.Nuevo(_terminaciones.Add);
        return pantalla;
    }

    private PropietarioEdicionViewModel Editar(Propietario propietario)
    {
        var pantalla = Servicios.GetRequiredService<PropietarioEdicionViewModel>();
        pantalla.Cargar(propietario, _terminaciones.Add);
        return pantalla;
    }

    private Propietario Leer(int id) => Bd.CrearContexto().Propietarios.Single(p => p.Id == id);

    [Fact]
    [Trait("Req", "RF-02")]
    public void RF02_UnFormularioNuevoTieneTituloPropioYNoOfreceInactivar()
    {
        var pantalla = Nuevo();

        Assert.Equal("Nuevo propietario", pantalla.Titulo);
        Assert.True(pantalla.EsNuevo);
        Assert.False(pantalla.EsEdicion);
        Assert.False(pantalla.CambiarEstadoCommand.CanExecute(null));
    }

    [Theory]
    [Trait("Req", "RN-04")]
    [InlineData("", "")]
    [InlineData("Ana Pérez", "")]
    [InlineData("", "3001234567")]
    [InlineData("   ", "3001234567")]
    public void RN04_SinNombreOSinTelefonoElBotonGuardarNoSeHabilita(string nombre, string telefono)
    {
        var pantalla = Nuevo();

        pantalla.NombreCompleto = nombre;
        pantalla.Telefono = telefono;

        Assert.False(pantalla.GuardarCommand.CanExecute(null));
        pantalla.GuardarCommand.Execute(null);
        Assert.Empty(_terminaciones);
    }

    [Fact]
    [Trait("Req", "RF-02")]
    public void RF02_GuardarRegistraAlPropietarioYAvisaQueTermino()
    {
        var pantalla = Nuevo();
        pantalla.NombreCompleto = "  María Torres ";
        pantalla.Telefono = "3151112233";
        pantalla.Documento = "1020304050";
        pantalla.Correo = "maria@correo.co";

        Assert.True(pantalla.GuardarCommand.CanExecute(null));
        pantalla.GuardarCommand.Execute(null);

        Assert.Equal([true], _terminaciones);
        var guardado = Bd.CrearContexto().Propietarios.Single();
        Assert.Equal("María Torres", guardado.NombreCompleto);
        Assert.Equal("3151112233", guardado.Telefono);
        Assert.Equal("1020304050", guardado.Documento);
        Assert.Equal("maria@correo.co", guardado.Correo);
        Assert.Null(guardado.Direccion);
    }

    [Theory]
    [Trait("Req", "RN-14")]
    [InlineData("2001234567", true)]
    [InlineData("300123", true)]
    [InlineData("573001234567", true)]
    [InlineData("300 123 4567", true)]
    [InlineData("3001234567", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void RN14_ElFormatoDelCelularSeValidaEnVivo(string telefono, bool debeMostrarError)
    {
        var pantalla = Nuevo();
        var cambios = new List<string?>();
        pantalla.PropertyChanged += (_, e) => cambios.Add(e.PropertyName);

        pantalla.Telefono = telefono;

        Assert.Equal(debeMostrarError, pantalla.TieneErrorTelefono);
        Assert.Equal(debeMostrarError ? PropietarioEdicionViewModel.MensajeFormatoTelefono : string.Empty, pantalla.ErrorTelefono);
        if (telefono.Length > 0)
        {
            Assert.Contains(nameof(PropietarioEdicionViewModel.ErrorTelefono), cambios);
        }
    }

    [Fact]
    [Trait("Req", "RN-14")]
    public void RN14_ConUnTelefonoInvalidoElServicioRechazaYElFormularioNoSeCierra()
    {
        var pantalla = Nuevo();
        pantalla.NombreCompleto = "María Torres";
        pantalla.Telefono = "12345";

        pantalla.GuardarCommand.Execute(null);

        Assert.True(pantalla.TieneError);
        Assert.Contains("celular colombiano", pantalla.MensajeError);
        Assert.Empty(_terminaciones);
        Assert.Empty(Bd.CrearContexto().Propietarios);
    }

    [Fact]
    [Trait("Req", "RF-04")]
    public void RF04_EditarCargaLosDatosYGuardaLosCambios()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var pantalla = Editar(Leer(ana.Id));
        Assert.Equal("Editar propietario", pantalla.Titulo);
        Assert.True(pantalla.EsEdicion);
        Assert.Equal("Ana Pérez", pantalla.NombreCompleto);
        Assert.Equal("3001234567", pantalla.Telefono);

        pantalla.Telefono = "3205550000";
        pantalla.Direccion = "Calle 5 # 6-7";
        pantalla.GuardarCommand.Execute(null);

        Assert.Equal([true], _terminaciones);
        var guardado = Leer(ana.Id);
        Assert.Equal("3205550000", guardado.Telefono);
        Assert.Equal("Calle 5 # 6-7", guardado.Direccion);
        Assert.Equal("Ana Pérez", guardado.NombreCompleto);
    }

    [Fact]
    [Trait("Req", "RF-04")]
    public void RF04_CancelarNoGuardaNadaYAvisaQueSeCancelo()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var pantalla = Editar(Leer(ana.Id));
        pantalla.Telefono = "3205550000";

        pantalla.CancelarCommand.Execute(null);

        Assert.Equal([false], _terminaciones);
        Assert.Equal("3001234567", Leer(ana.Id).Telefono);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_InactivarPideConfirmacionYConservaAlPropietario()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        var pantalla = Editar(Leer(ana.Id));
        Assert.Equal("Inactivar", pantalla.TextoCambiarEstado);

        pantalla.CambiarEstadoCommand.Execute(null);

        Assert.Single(Dialogos.Confirmaciones);
        Assert.Contains("Ana Pérez", Dialogos.Confirmaciones[0]);
        Assert.Equal([true], _terminaciones);
        Assert.False(Leer(ana.Id).Activo);
        Assert.Equal(1, Bd.Escalar<int>("SELECT COUNT(*) FROM Mascota;"));
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_SiElUsuarioNoConfirmaElPropietarioSigueActivo()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        Dialogos.RespuestaConfirmar = false;
        var pantalla = Editar(Leer(ana.Id));

        pantalla.CambiarEstadoCommand.Execute(null);

        Assert.Empty(_terminaciones);
        Assert.True(Leer(ana.Id).Activo);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_UnPropietarioInactivoSePuedeReactivarSinConfirmar()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567", activo: false);
        var pantalla = Editar(Leer(ana.Id));
        Assert.Equal("Reactivar", pantalla.TextoCambiarEstado);

        pantalla.CambiarEstadoCommand.Execute(null);

        Assert.Empty(Dialogos.Confirmaciones);
        Assert.Equal([true], _terminaciones);
        Assert.True(Leer(ana.Id).Activo);
    }

    [Fact]
    [Trait("Req", "RF-02")]
    public void RF02_AlReabrirElFormularioNuevoSeLimpianLosDatosAnteriores()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var pantalla = Editar(Leer(ana.Id));
        pantalla.Telefono = "123";
        pantalla.GuardarCommand.Execute(null);
        Assert.True(pantalla.TieneError);

        pantalla.Nuevo(_terminaciones.Add);

        Assert.Equal(string.Empty, pantalla.NombreCompleto);
        Assert.Equal(string.Empty, pantalla.Telefono);
        Assert.False(pantalla.TieneError);
        Assert.True(pantalla.EsNuevo);
    }
}
