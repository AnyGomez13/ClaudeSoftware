using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Pruebas.ViewModels;

namespace VeterinariaDrFabio.Pruebas.Infraestructura;

/// <summary>Comprueba que el registro de dependencias de la aplicación real puede crear cada pieza (SUP-D09).</summary>
public class ComposicionDeServiciosTests : PantallaTestBase
{
    [Fact]
    [Trait("Req", "RNF-08")]
    public void RNF08_TodosLosServiciosYViewModelsRegistradosSePuedenCrear()
    {
        Type[] tipos =
        [
            typeof(IPropietarioService),
            typeof(IMascotaService),
            typeof(IProcedimientoService),
            typeof(IVacunacionService),
            typeof(ICarnetService),
            typeof(IAlertaService),
            typeof(IRecordatorioService),
            typeof(IVeterinarioService),
            typeof(IAutenticacionService),
            typeof(LoginViewModel),
            typeof(MainViewModel),
            typeof(PropietariosViewModel),
            typeof(PropietarioEdicionViewModel),
            typeof(MascotasViewModel),
            typeof(MascotaEdicionViewModel),
            typeof(MascotaDetalleViewModel),
            typeof(ProcedimientoEdicionViewModel),
            typeof(VacunacionEdicionViewModel),
            typeof(CarnetViewModel),
            typeof(AlertasViewModel),
            typeof(ConfiguracionViewModel),
            typeof(VeterinariosViewModel),
        ];

        foreach (var tipo in tipos)
        {
            Assert.NotNull(Servicios.GetRequiredService(tipo));
        }
    }

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_LaNavegacionYLaSesionSonUnicasEnTodaLaAplicacion()
    {
        Assert.Same(Servicios.GetRequiredService<VeterinariaDrFabio.App.Infraestructura.INavigationService>(), Navegacion);
        Assert.Same(Servicios.GetRequiredService<MainViewModel>(), Servicios.GetRequiredService<MainViewModel>());
        Assert.NotSame(Servicios.GetRequiredService<PropietariosViewModel>(), Servicios.GetRequiredService<PropietariosViewModel>());
    }
}
