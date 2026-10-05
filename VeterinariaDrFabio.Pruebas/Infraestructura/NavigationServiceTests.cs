using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Pruebas.ViewModels;

namespace VeterinariaDrFabio.Pruebas.Infraestructura;

/// <summary>Pruebas de <see cref="NavigationService"/> (RF-01, RN-13).</summary>
public class NavigationServiceTests
{
    private sealed class PrimeraViewModel : BaseViewModel
    {
        public string Dato { get; set; } = string.Empty;
    }

    private sealed class SegundaViewModel : BaseViewModel
    {
    }

    private readonly AutenticacionFalsa _autenticacion = new();
    private readonly NavigationService _navegacion;

    public NavigationServiceTests()
    {
        var servicios = new ServiceCollection();
        servicios.AddTransient<PrimeraViewModel>();
        servicios.AddTransient<SegundaViewModel>();
        _navegacion = new NavigationService(servicios.BuildServiceProvider(), _autenticacion);
    }

    private void IniciarSesion() => _autenticacion.IniciarSesion(AutenticacionFalsa.UsuarioValido, AutenticacionFalsa.ClaveValida);

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_SinSesionNoSePuedeNavegar()
    {
        Assert.Throws<InvalidOperationException>(() => _navegacion.NavegarA<PrimeraViewModel>());
        Assert.Throws<InvalidOperationException>(() => _navegacion.NavegarASeccion<PrimeraViewModel>());
        Assert.Null(_navegacion.ViewModelActual);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_NavegarCreaElViewModelLoInicializaYNotificaElCambio()
    {
        IniciarSesion();
        var cambios = new List<string?>();
        _navegacion.PropertyChanged += (_, e) => cambios.Add(e.PropertyName);

        _navegacion.NavegarA<PrimeraViewModel>(vm => vm.Dato = "mascota 7");

        var actual = Assert.IsType<PrimeraViewModel>(_navegacion.ViewModelActual);
        Assert.Equal("mascota 7", actual.Dato);
        Assert.Contains(nameof(INavigationService.ViewModelActual), cambios);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_VolverRegresaALaVistaAnterior()
    {
        IniciarSesion();
        _navegacion.NavegarA<PrimeraViewModel>();
        var primera = _navegacion.ViewModelActual;
        Assert.False(_navegacion.PuedeVolver);

        _navegacion.NavegarA<SegundaViewModel>();
        Assert.IsType<SegundaViewModel>(_navegacion.ViewModelActual);
        Assert.True(_navegacion.PuedeVolver);

        _navegacion.Volver();

        Assert.Same(primera, _navegacion.ViewModelActual);
        Assert.False(_navegacion.PuedeVolver);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_NavegarASeccionDescartaElHistorial()
    {
        IniciarSesion();
        _navegacion.NavegarA<PrimeraViewModel>();
        _navegacion.NavegarA<SegundaViewModel>();

        _navegacion.NavegarASeccion<PrimeraViewModel>();

        Assert.IsType<PrimeraViewModel>(_navegacion.ViewModelActual);
        Assert.False(_navegacion.PuedeVolver);
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_VolverSinHistorialNoHaceNada()
    {
        IniciarSesion();
        _navegacion.NavegarA<PrimeraViewModel>();
        var actual = _navegacion.ViewModelActual;

        _navegacion.Volver();

        Assert.Same(actual, _navegacion.ViewModelActual);
    }

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_LimpiarQuitaLaVistaYElHistorial()
    {
        IniciarSesion();
        _navegacion.NavegarA<PrimeraViewModel>();
        _navegacion.NavegarA<SegundaViewModel>();

        _navegacion.Limpiar();

        Assert.Null(_navegacion.ViewModelActual);
        Assert.False(_navegacion.PuedeVolver);
    }
}
