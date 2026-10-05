using System.Windows;
using System.Windows.Threading;

namespace VeterinariaDrFabio.Pruebas.Vistas;

/// <summary>
/// Hilo STA único con un <see cref="Dispatcher"/> y la aplicación WPF (recursos de colores y estilos) cargada,
/// para probar las vistas XAML sin mostrarlas en pantalla. WPF permite una sola aplicación por proceso.
/// </summary>
internal static class HiloUi
{
    private static readonly Lazy<Dispatcher> Despachador = new(Iniciar);

    public static T Ejecutar<T>(Func<T> accion) => Despachador.Value.Invoke(accion);

    public static void Ejecutar(Action accion) => Despachador.Value.Invoke(accion);

    private static Dispatcher Iniciar()
    {
        Dispatcher? despachador = null;
        using var listo = new ManualResetEventSlim();

        var hilo = new Thread(() =>
        {
            var aplicacion = new VeterinariaDrFabio.App.App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            aplicacion.InitializeComponent();
            despachador = Dispatcher.CurrentDispatcher;
            listo.Set();
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "Hilo UI de pruebas",
        };
        hilo.SetApartmentState(ApartmentState.STA);
        hilo.Start();
        listo.Wait();
        return despachador!;
    }
}
