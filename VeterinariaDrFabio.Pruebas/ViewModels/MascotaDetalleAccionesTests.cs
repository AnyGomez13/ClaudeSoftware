using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>
/// Pruebas de las acciones de la ficha P-07: abrir los formularios de procedimiento (P-08) y vacunación (P-09)
/// y volver con la historia actualizada (RF-09, RF-10, RF-11, RN-07).
/// </summary>
public class MascotaDetalleAccionesTests : PantallaTestBase
{
    private int _mascotaId;

    private MascotaDetalleViewModel AbrirFicha()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        _mascotaId = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky").Id;
        Navegacion.NavegarASeccion<MascotasViewModel>();
        Navegacion.NavegarA<MascotaDetalleViewModel>(vm => vm.Cargar(_mascotaId));
        return Assert.IsType<MascotaDetalleViewModel>(Navegacion.ViewModelActual);
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_NuevoProcedimientoAbreElFormularioDeLaMascota()
    {
        var ficha = AbrirFicha();

        ficha.NuevoProcedimientoCommand.Execute(null);

        var formulario = Assert.IsType<ProcedimientoEdicionViewModel>(Navegacion.ViewModelActual);
        Assert.Equal("Nuevo procedimiento — Rocky", formulario.Titulo);
        Assert.True(Navegacion.PuedeVolver);
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_RegistrarVacunaAbreElFormularioDeLaMascota()
    {
        var ficha = AbrirFicha();

        ficha.RegistrarVacunaCommand.Execute(null);

        var formulario = Assert.IsType<VacunacionEdicionViewModel>(Navegacion.ViewModelActual);
        Assert.Equal("Registrar vacuna — Rocky", formulario.Titulo);
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_AlGuardarUnProcedimientoSeVuelveALaFichaYApareceEnLaHistoria()
    {
        var ficha = AbrirFicha();
        Assert.True(ficha.SinRegistros);
        ficha.NuevoProcedimientoCommand.Execute(null);
        var formulario = Assert.IsType<ProcedimientoEdicionViewModel>(Navegacion.ViewModelActual);
        formulario.VeterinarioSeleccionado = formulario.Veterinarios.Single(v => v.Texto == "William");
        formulario.Fecha = new DateTime(2026, 10, 4);
        formulario.TipoProcedimiento = "Consulta";
        formulario.Descripcion = "Control general";

        formulario.GuardarCommand.Execute(null);

        Assert.Same(ficha, Navegacion.ViewModelActual);
        var registro = Assert.Single(ficha.Registros);
        Assert.Equal("04/10/2026", registro.Fecha);
        Assert.Equal("Procedimiento", registro.Tipo);
        Assert.Equal("Consulta: Control general", registro.Detalle);
        Assert.Equal("William", registro.Veterinario);
        Assert.False(ficha.SinRegistros);
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_AlGuardarUnaVacunaSeVuelveALaFichaYApareceEnLaHistoria()
    {
        var ficha = AbrirFicha();
        ficha.RegistrarVacunaCommand.Execute(null);
        var formulario = Assert.IsType<VacunacionEdicionViewModel>(Navegacion.ViewModelActual);
        formulario.VeterinarioSeleccionado = formulario.Veterinarios.Single(v => v.Texto == "Fabio");
        formulario.NombreVacuna = "Rabia";
        formulario.FechaAplicacion = new DateTime(2026, 10, 4);
        formulario.ProximaFecha = new DateTime(2027, 10, 4);

        formulario.GuardarCommand.Execute(null);

        Assert.Same(ficha, Navegacion.ViewModelActual);
        var registro = Assert.Single(ficha.Registros);
        Assert.Equal("Vacunación", registro.Tipo);
        Assert.Equal("Rabia. Próximo refuerzo: 04/10/2027", registro.Detalle);
        Assert.Equal("Fabio", registro.Veterinario);
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_LosRegistrosNuevosSeOrdenanPorFechaJuntoALosExistentes()
    {
        var ficha = AbrirFicha();
        var fabio = DatosDePrueba.IdDeVeterinario(Bd, "Fabio");
        DatosDePrueba.CrearProcedimiento(Bd, _mascotaId, fabio, "Control", new DateTime(2026, 9, 10));
        ficha.Recargar();
        ficha.RegistrarVacunaCommand.Execute(null);
        var formulario = Assert.IsType<VacunacionEdicionViewModel>(Navegacion.ViewModelActual);
        formulario.VeterinarioSeleccionado = formulario.Veterinarios[0];
        formulario.NombreVacuna = "Rabia";
        formulario.FechaAplicacion = new DateTime(2026, 2, 1);

        formulario.GuardarCommand.Execute(null);

        Assert.Equal(["01/02/2026", "10/09/2026"], ficha.Registros.Select(r => r.Fecha).ToList());
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_AlCancelarSeVuelveALaFichaSinCambios()
    {
        var ficha = AbrirFicha();
        ficha.NuevoProcedimientoCommand.Execute(null);
        var formulario = Assert.IsType<ProcedimientoEdicionViewModel>(Navegacion.ViewModelActual);
        formulario.Descripcion = "No se guarda";

        formulario.CancelarCommand.Execute(null);

        Assert.Same(ficha, Navegacion.ViewModelActual);
        Assert.Empty(ficha.Registros);
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_SinMascotaEncontradaLosBotonesNoSeHabilitan()
    {
        AbrirFicha();
        var ficha = Assert.IsType<MascotaDetalleViewModel>(Navegacion.ViewModelActual);

        ficha.Cargar(999);

        Assert.False(ficha.NuevoProcedimientoCommand.CanExecute(null));
        Assert.False(ficha.RegistrarVacunaCommand.CanExecute(null));
    }
}
