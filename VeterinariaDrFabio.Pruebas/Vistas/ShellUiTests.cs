using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.App.Vistas;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Pruebas.ViewModels;

namespace VeterinariaDrFabio.Pruebas.Vistas;

/// <summary>
/// Pruebas de las vistas P-01 (login) y P-02 (shell): cargan el XAML real fuera de pantalla, comprueban
/// qué se muestra en cada estado y que no haya errores de binding (RF-01, RN-13, RNF-01).
/// </summary>
public class ShellUiTests
{
    private const double Ancho = 1180;
    private const double Alto = 680;

    private sealed class EscuchaDeEnlaces : TraceListener
    {
        public List<string> Mensajes { get; } = [];

        public override void Write(string? mensaje)
        {
        }

        public override void WriteLine(string? mensaje) => Mensajes.Add(mensaje ?? string.Empty);
    }

    private sealed record Escena(MainWindow Ventana, MainViewModel Modelo, AutenticacionFalsa Autenticacion)
    {
        public FrameworkElement Raiz => (FrameworkElement)Ventana.Content;

        public FrameworkElement Elemento(string nombre) => (FrameworkElement)Ventana.FindName(nombre);

        public void Dibujar()
        {
            // Los comandos piden reevaluar su estado con prioridad de fondo; se procesa la cola como lo hace la aplicación.
            Ventana.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
            Raiz.Measure(new Size(Ancho, Alto));
            Raiz.Arrange(new Rect(0, 0, Ancho, Alto));
            Raiz.UpdateLayout();
        }

        public LoginView VistaLogin => (LoginView)((Grid)Elemento("VistaLogin")).Children[0];

        public T ControlDelLogin<T>(string nombre) => (T)VistaLogin.FindName(nombre);

        public void IniciarSesionDesdeElModelo()
        {
            Modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
            Modelo.Login.Contrasena = AutenticacionFalsa.ClaveValida;
            Modelo.Login.IniciarSesionCommand.Execute(null);
            Dibujar();
        }
    }

    private static Escena CrearEscena()
    {
        var autenticacion = new AutenticacionFalsa();
        var servicios = new ServiceCollection();
        servicios.AddSingleton<IAutenticacionService>(autenticacion);
        servicios.AddSingleton<INavigationService, NavigationService>();
        servicios.AddSingleton<LoginViewModel>();
        servicios.AddSingleton<MainViewModel>();
        servicios.AddTransient<MainWindow>();
        var proveedor = servicios.BuildServiceProvider();

        var escena = new Escena(proveedor.GetRequiredService<MainWindow>(), proveedor.GetRequiredService<MainViewModel>(), autenticacion);
        escena.Dibujar();
        return escena;
    }

    private static IEnumerable<DependencyObject> Descendientes(DependencyObject raiz)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(raiz); i++)
        {
            var hijo = VisualTreeHelper.GetChild(raiz, i);
            yield return hijo;
            foreach (var nieto in Descendientes(hijo))
            {
                yield return nieto;
            }
        }
    }

    private static List<string> TextosDeBotones(DependencyObject raiz) =>
        Descendientes(raiz).OfType<Button>().Select(b => b.Content as string ?? string.Empty).ToList();

    private static void GuardarPng(FrameworkElement elemento, string nombre)
    {
        var imagen = new RenderTargetBitmap((int)Ancho, (int)Alto, 96, 96, PixelFormats.Pbgra32);
        imagen.Render(elemento);
        var codificador = new PngBitmapEncoder();
        codificador.Frames.Add(BitmapFrame.Create(imagen));
        var carpeta = Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas");
        Directory.CreateDirectory(carpeta);
        using var archivo = File.Create(Path.Combine(carpeta, nombre));
        codificador.Save(archivo);
    }

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_AlAbrirLaAplicacionSoloSeVeElLogin()
    {
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena();

            Assert.Equal(Visibility.Visible, escena.Elemento("VistaLogin").Visibility);
            Assert.Equal(Visibility.Collapsed, escena.Elemento("VistaPrincipal").Visibility);
            Assert.Contains("Ingresar", TextosDeBotones(escena.VistaLogin));
            Assert.Equal(Visibility.Collapsed, escena.ControlDelLogin<TextBlock>("MensajeError").Visibility);
        });
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ElBotonIngresarSoloSeHabilitaConUsuarioYContrasena()
    {
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena();
            var ingresar = Descendientes(escena.VistaLogin).OfType<Button>().Single(b => b.Content as string == "Ingresar");
            Assert.False(ingresar.IsEnabled);

            escena.ControlDelLogin<TextBox>("CampoUsuario").Text = "fabio";
            escena.Dibujar();
            Assert.False(ingresar.IsEnabled);

            escena.ControlDelLogin<PasswordBox>("CampoContrasena").Password = "x";
            escena.Dibujar();
            Assert.True(ingresar.IsEnabled);
        });
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_LaContrasenaEscritaEnElCampoLlegaAlViewModelYSeVaciaTrasUnFallo()
    {
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena();
            var campoContrasena = escena.ControlDelLogin<PasswordBox>("CampoContrasena");
            var campoUsuario = escena.ControlDelLogin<TextBox>("CampoUsuario");

            campoUsuario.Text = AutenticacionFalsa.UsuarioValido;
            campoContrasena.Password = "incorrecta";
            Assert.Equal(AutenticacionFalsa.UsuarioValido, escena.Modelo.Login.Usuario);
            Assert.Equal("incorrecta", escena.Modelo.Login.Contrasena);

            escena.Modelo.Login.IniciarSesionCommand.Execute(null);
            escena.Dibujar();

            Assert.Equal(string.Empty, campoContrasena.Password);
            var mensaje = escena.ControlDelLogin<TextBlock>("MensajeError");
            Assert.Equal(Visibility.Visible, mensaje.Visibility);
            Assert.Equal("Credenciales inválidas", mensaje.Text);
            Assert.Equal(Visibility.Visible, escena.Elemento("VistaLogin").Visibility);
        });
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ConCredencialesValidasSeMuestraElShellConElMenuLateral()
    {
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena();

            escena.IniciarSesionDesdeElModelo();

            Assert.Equal(Visibility.Collapsed, escena.Elemento("VistaLogin").Visibility);
            Assert.Equal(Visibility.Visible, escena.Elemento("VistaPrincipal").Visibility);
            var botones = TextosDeBotones(escena.Elemento("VistaPrincipal"));
            Assert.Equal(["Propietarios", "Mascotas", "Alertas", "Veterinarios", "Cerrar sesión"], botones);
            Assert.All(Descendientes(escena.Elemento("VistaPrincipal")).OfType<Button>(), b => Assert.True(b.IsEnabled, (string)b.Content));
            var textos = Descendientes(escena.Elemento("VistaPrincipal")).OfType<TextBlock>().Select(t => t.Text).ToList();
            Assert.Contains("Clínica Veterinaria Dr. Fabio", textos);
            Assert.Contains(AutenticacionFalsa.UsuarioValido, textos);
        });
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ElMenuCambiaLaSeccionYCerrarSesionVuelveAlLogin()
    {
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena();
            escena.IniciarSesionDesdeElModelo();
            var principal = escena.Elemento("VistaPrincipal");

            var botonAlertas = Descendientes(principal).OfType<Button>().Single(b => b.Content as string == "Alertas");
            botonAlertas.Command.Execute(botonAlertas.CommandParameter);
            escena.Dibujar();

            Assert.Equal("Alertas", escena.Modelo.SeccionActual);
            Assert.Contains("Alertas", Descendientes(principal).OfType<TextBlock>().Select(t => t.Text));
            var fondos = Descendientes(principal).OfType<Button>()
                .Where(b => b.Style == (Style)Application.Current.Resources["BotonMenu"])
                .ToDictionary(b => (string)b.Content, b => ((SolidColorBrush)((Border)b.Template.FindName("Fondo", b)).Background).Color);
            var oscuro = (Color)Application.Current.Resources["PrimarioOscuroColor"];
            Assert.Equal(["Alertas"], fondos.Where(f => f.Value == oscuro).Select(f => f.Key).ToList());

            var botonCerrar = Descendientes(principal).OfType<Button>().Single(b => b.Content as string == "Cerrar sesión");
            botonCerrar.Command.Execute(null);
            escena.Dibujar();

            Assert.False(escena.Autenticacion.SesionActiva);
            Assert.Equal(Visibility.Visible, escena.Elemento("VistaLogin").Visibility);
            Assert.Equal(Visibility.Collapsed, escena.Elemento("VistaPrincipal").Visibility);
            Assert.Equal(string.Empty, escena.ControlDelLogin<PasswordBox>("CampoContrasena").Password);
        });
    }

    [Fact]
    [Trait("Req", "RNF-01")]
    public void RNF01_LasVistasNoGeneranErroresDeBinding()
    {
        var escucha = new EscuchaDeEnlaces();
        PresentationTraceSources.Refresh();
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
        PresentationTraceSources.DataBindingSource.Listeners.Add(escucha);
        try
        {
            HiloUi.Ejecutar(() =>
            {
                var escena = CrearEscena();
                escena.Modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
                escena.Modelo.Login.Contrasena = "incorrecta";
                escena.Modelo.Login.IniciarSesionCommand.Execute(null);
                escena.Dibujar();
                escena.IniciarSesionDesdeElModelo();
                escena.Modelo.NavegarSeccionCommand.Execute("Veterinarios");
                escena.Dibujar();
                escena.Modelo.CerrarSesionCommand.Execute(null);
                escena.Dibujar();
            });
        }
        finally
        {
            PresentationTraceSources.DataBindingSource.Listeners.Remove(escucha);
        }

        Assert.True(escucha.Mensajes.Count == 0, string.Join(Environment.NewLine, escucha.Mensajes));
    }

    [Fact]
    [Trait("Req", "RNF-01")]
    public void RNF01_GeneraImagenesDeMuestraParaInspeccionVisual()
    {
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena();
            GuardarPng(escena.Raiz, "muestra-login.png");

            escena.Modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
            escena.Modelo.Login.Contrasena = "incorrecta";
            escena.Modelo.Login.IniciarSesionCommand.Execute(null);
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-login-error.png");

            escena.IniciarSesionDesdeElModelo();
            escena.Modelo.NavegarSeccionCommand.Execute("Mascotas");
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-shell.png");
        });

        Assert.True(File.Exists(Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas", "muestra-shell.png")));
    }
}
