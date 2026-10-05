using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.App.Infraestructura;

/// <summary>
/// Registro de todas las dependencias de la aplicación (SUP-D09). Está separado de <c>App</c> para que las pruebas
/// usen exactamente el mismo registro que la aplicación real.
/// </summary>
internal static class ComposicionDeServicios
{
    public static IServiceCollection Registrar(IServiceCollection servicios, string cadenaConexion)
    {
        // Cada resolución recibe su propio contexto, de modo que un error al guardar no contamina a los demás.
        servicios.AddDbContext<VeterinariaDbContext>(
            opciones => opciones.UseSqlite(cadenaConexion),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        servicios.AddTransient<IUsuarioRepository, UsuarioRepository>();
        servicios.AddTransient<IVeterinarioRepository, VeterinarioRepository>();
        servicios.AddTransient<IPropietarioRepository, PropietarioRepository>();
        servicios.AddTransient<IMascotaRepository, MascotaRepository>();
        servicios.AddTransient<IProcedimientoRepository, ProcedimientoRepository>();
        servicios.AddTransient<IVacunacionRepository, VacunacionRepository>();
        servicios.AddTransient<IRecordatorioRepository, RecordatorioRepository>();

        servicios.AddTransient<HasherContrasena>();
        servicios.AddTransient<CalculadoraEdad>();
        servicios.AddTransient<GeneradorEnlaceWhatsApp>();
        servicios.AddTransient<GeneradorCarnetPdf>();
        servicios.AddSingleton<IConectividad, ConectividadRed>();

        // La sesión vive mientras la aplicación esté abierta, por eso el servicio es único.
        servicios.AddSingleton<IAutenticacionService, AutenticacionService>();
        servicios.AddTransient<IVeterinarioService, VeterinarioService>();
        servicios.AddTransient<IPropietarioService, PropietarioService>();
        servicios.AddTransient<IMascotaService, MascotaService>();
        servicios.AddTransient<IProcedimientoService, ProcedimientoService>();
        servicios.AddTransient<IVacunacionService, VacunacionService>();
        servicios.AddTransient<ICarnetService, CarnetService>();
        servicios.AddTransient<IAlertaService, AlertaService>();
        servicios.AddTransient<IRecordatorioService, RecordatorioService>();

        // Presentación: navegación y diálogos únicos; cada pantalla se crea nueva al navegar a ella.
        servicios.AddSingleton<INavigationService, NavigationService>();
        servicios.AddSingleton<IDialogService, DialogService>();
        servicios.AddSingleton<IAbridorDeEnlaces, AbridorDeEnlaces>();
        servicios.AddSingleton<LoginViewModel>();
        servicios.AddSingleton<MainViewModel>();
        servicios.AddTransient<PropietariosViewModel>();
        servicios.AddTransient<PropietarioEdicionViewModel>();
        servicios.AddTransient<MascotasViewModel>();
        servicios.AddTransient<MascotaEdicionViewModel>();
        servicios.AddTransient<MascotaDetalleViewModel>();
        servicios.AddTransient<ProcedimientoEdicionViewModel>();
        servicios.AddTransient<VacunacionEdicionViewModel>();
        servicios.AddTransient<CarnetViewModel>();
        servicios.AddTransient<AlertasViewModel>();
        servicios.AddTransient<VeterinariosViewModel>();
        servicios.AddTransient<MainWindow>();

        return servicios;
    }
}
