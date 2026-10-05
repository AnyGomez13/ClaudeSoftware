using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.App.Vistas;
using VeterinariaDrFabio.Pruebas.Datos;
using VeterinariaDrFabio.Pruebas.ViewModels;

namespace VeterinariaDrFabio.Pruebas.Vistas;

/// <summary>
/// Pruebas de las vistas P-01 a P-04 y P-12: cargan el XAML real fuera de pantalla con el registro de dependencias de
/// la aplicación, comprueban qué se muestra en cada estado y que no haya errores de binding (RF-01, RF-02, RF-03, RF-16, RNF-01).
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

    private sealed record Escena(
        MainWindow Ventana, MainViewModel Modelo, AutenticacionFalsa Autenticacion, AbridorFalso Abridor, DialogoFalso Dialogos)
    {
        public FrameworkElement Raiz => (FrameworkElement)Ventana.Content;

        public FrameworkElement Elemento(string nombre) => (FrameworkElement)Ventana.FindName(nombre);

        public FrameworkElement Principal => Elemento("VistaPrincipal");

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

        /// <summary>La vista de pantalla que muestra hoy el área de contenido.</summary>
        public T Vista<T>()
            where T : FrameworkElement => Descendientes(Principal).OfType<T>().Single();

        public T Control<T>(FrameworkElement vista, string nombre) => (T)vista.FindName(nombre);

        public Button Boton(DependencyObject dentroDe, string texto) =>
            Descendientes(dentroDe).OfType<Button>().Single(b => b.Content as string == texto);

        public void IniciarSesionDesdeElModelo()
        {
            Modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
            Modelo.Login.Contrasena = AutenticacionFalsa.ClaveValida;
            Modelo.Login.IniciarSesionCommand.Execute(null);
            Dibujar();
        }

        public void IrASeccion(string titulo)
        {
            var boton = Descendientes(Principal).OfType<Button>().Single(b => b.Content as string == titulo);
            boton.Command.Execute(boton.CommandParameter);
            Dibujar();
        }
    }

    private static Escena CrearEscena(BaseDatosTemporal bd)
    {
        var autenticacion = new AutenticacionFalsa();
        var abridor = new AbridorFalso();
        var dialogos = new DialogoFalso();
        var proveedor = ComposicionDePrueba.Crear(bd, autenticacion, dialogos, new ConectividadDePrueba(), abridor);

        var escena = new Escena(
            proveedor.GetRequiredService<MainWindow>(), proveedor.GetRequiredService<MainViewModel>(), autenticacion, abridor, dialogos);
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

    private static List<string> TextosDeMenu(DependencyObject raiz)
    {
        var estiloMenu = (Style)Application.Current.Resources["BotonMenu"];
        return Descendientes(raiz).OfType<Button>().Where(b => b.Style == estiloMenu).Select(b => (string)b.Content).ToList();
    }

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

    private static string Texto(FrameworkElement vista, string nombre) => vista.FindName(nombre) switch
    {
        TextBlock bloque => bloque.Text,
        Run corrida => corrida.Text,
        var otro => throw new InvalidOperationException($"{nombre} no es un texto: {otro}"),
    };

    /// <summary>Agrega una vacunación y un procedimiento a Rocky, la primera mascota de <see cref="SembrarPropietarios"/>.</summary>
    private static void SembrarHistoria(BaseDatosTemporal bd)
    {
        var rocky = bd.Escalar<int>("SELECT Id FROM Mascota WHERE Nombre = 'Rocky';");
        var fabio = DatosDePrueba.IdDeVeterinario(bd, "Fabio");
        DatosDePrueba.CrearVacunacion(bd, rocky, fabio, "Rabia", new DateTime(2026, 1, 10), new DateTime(2027, 1, 10));
        DatosDePrueba.CrearProcedimiento(bd, rocky, fabio, "Consulta", new DateTime(2026, 3, 5));
    }

    private static void AbrirFichaDe(Escena escena, string mascota)
    {
        var tabla = escena.Control<DataGrid>(escena.Vista<MascotasView>(), "TablaMascotas");
        var abrir = Descendientes(tabla).OfType<Button>()
            .Where(b => b.Content as string == "Abrir ficha")
            .First(b => ((MascotaFila)b.CommandParameter).Nombre == mascota);
        abrir.Command.Execute(abrir.CommandParameter);
        escena.Dibujar();
    }

    /// <summary>Deja a Rocky con una vacunación vencida y otra próxima, para la pantalla de alertas.</summary>
    private static void SembrarAlertas(BaseDatosTemporal bd)
    {
        var rocky = bd.Escalar<int>("SELECT Id FROM Mascota WHERE Nombre = 'Rocky';");
        var fabio = DatosDePrueba.IdDeVeterinario(bd, "Fabio");
        DatosDePrueba.CrearVacunacion(bd, rocky, fabio, "Moquillo", DateTime.Today.AddDays(-300), DateTime.Today.AddDays(-5));
        DatosDePrueba.CrearVacunacion(bd, rocky, fabio, "Parvovirus", DateTime.Today.AddDays(-200), DateTime.Today.AddDays(10));
    }

    private static Color ColorDeInsignia(DependencyObject tabla, string texto)
    {
        var bloque = Descendientes(tabla).OfType<TextBlock>().First(t => t.Text == texto);
        var insignia = (Border)VisualTreeHelper.GetParent(bloque);
        return ((SolidColorBrush)insignia.Background).Color;
    }

    private static void SembrarPropietarios(BaseDatosTemporal bd)
    {
        var ana = DatosDePrueba.CrearPropietario(bd, "Ana Pérez", "3001234567");
        DatosDePrueba.CrearPropietario(bd, "Luis Gómez", "3109876543", activo: false);
        DatosDePrueba.CrearMascota(bd, ana.Id, "Rocky");
        DatosDePrueba.CrearMascota(bd, ana.Id, "Misu");
    }

    [Fact]
    [Trait("Req", "RN-13")]
    public void RN13_AlAbrirLaAplicacionSoloSeVeElLogin()
    {
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);

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
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            var ingresar = escena.Boton(escena.VistaLogin, "Ingresar");
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
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
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
    public void RF01_ConCredencialesValidasSeMuestraElShellConElMenuLateralYLaListaDePropietarios()
    {
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);

            escena.IniciarSesionDesdeElModelo();

            Assert.Equal(Visibility.Collapsed, escena.Elemento("VistaLogin").Visibility);
            Assert.Equal(Visibility.Visible, escena.Elemento("VistaPrincipal").Visibility);
            Assert.Equal(["Propietarios", "Mascotas", "Alertas", "Veterinarios"], TextosDeMenu(escena.Principal));
            Assert.Contains("Cerrar sesión", TextosDeBotones(escena.Principal));
            // Se excluye el botón de esquina que WPF agrega a cada DataGrid ("seleccionar todo", sin texto).
            var deshabilitados = Descendientes(escena.Principal).OfType<Button>()
                .Where(b => b.Command is not RoutedUICommand && !b.IsEnabled)
                .Select(b => $"{b.Content} (comando: {b.Command?.GetType().Name ?? "ninguno"})")
                .ToList();
            Assert.True(deshabilitados.Count == 0, "Botones deshabilitados: " + string.Join("; ", deshabilitados));
            var textos = Descendientes(escena.Principal).OfType<TextBlock>().Select(t => t.Text).ToList();
            Assert.Contains("Clínica Veterinaria Dr. Fabio", textos);
            Assert.Contains(AutenticacionFalsa.UsuarioValido, textos);
            Assert.NotNull(escena.Vista<PropietariosView>());
        });
    }

    [Fact]
    [Trait("Req", "RF-01")]
    public void RF01_ElMenuCambiaDeSeccionYCerrarSesionVuelveAlLogin()
    {
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();

            escena.IrASeccion("Alertas");

            Assert.Equal("Alertas", escena.Modelo.SeccionActual);
            Assert.Contains("Alertas", Descendientes(escena.Principal).OfType<TextBlock>().Select(t => t.Text));
            var estiloMenu = (Style)Application.Current.Resources["BotonMenu"];
            var fondos = Descendientes(escena.Principal).OfType<Button>()
                .Where(b => b.Style == estiloMenu)
                .ToDictionary(b => (string)b.Content, b => ((SolidColorBrush)((Border)b.Template.FindName("Fondo", b)).Background).Color);
            var oscuro = (Color)Application.Current.Resources["PrimarioOscuroColor"];
            Assert.Equal(["Alertas"], fondos.Where(f => f.Value == oscuro).Select(f => f.Key).ToList());

            escena.Boton(escena.Principal, "Cerrar sesión").Command.Execute(null);
            escena.Dibujar();

            Assert.False(escena.Autenticacion.SesionActiva);
            Assert.Equal(Visibility.Visible, escena.Elemento("VistaLogin").Visibility);
            Assert.Equal(Visibility.Collapsed, escena.Elemento("VistaPrincipal").Visibility);
            Assert.Equal(string.Empty, escena.ControlDelLogin<PasswordBox>("CampoContrasena").Password);
        });
    }

    [Fact]
    [Trait("Req", "RF-03")]
    public void RF03_LaPantallaDePropietariosMuestraLaTablaFiltraYListaLasMascotas()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            var vista = escena.Vista<PropietariosView>();
            var tabla = escena.Control<DataGrid>(vista, "TablaPropietarios");

            Assert.Equal(2, tabla.Items.Count);
            Assert.Equal(["Nombre", "Teléfono", "Mascotas", "Estado", string.Empty], tabla.Columns.Select(c => (string)c.Header).ToList());
            Assert.Equal(Visibility.Collapsed, escena.Control<TextBlock>(vista, "MensajeSinResultados").Visibility);

            escena.Control<TextBox>(vista, "CampoBusqueda").Text = "Pérez";
            escena.Dibujar();
            Assert.Single(tabla.Items);

            tabla.SelectedItem = tabla.Items[0];
            escena.Dibujar();
            Assert.Equal("Mascotas de Ana Pérez", escena.Control<TextBlock>(vista, "TituloMascotas").Text);
            Assert.Equal(2, escena.Control<DataGrid>(vista, "TablaMascotas").Items.Count);

            escena.Control<TextBox>(vista, "CampoBusqueda").Text = "zzz";
            escena.Dibujar();
            Assert.Empty(tabla.Items);
            Assert.Equal(Visibility.Visible, escena.Control<TextBlock>(vista, "MensajeSinResultados").Visibility);
        });
    }

    [Fact]
    [Trait("Req", "RF-02")]
    public void RF02_NuevoPropietarioAbreElFormularioValidaElCelularYAlGuardarVuelveALaLista()
    {
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();

            escena.Boton(escena.Vista<PropietariosView>(), "Nuevo propietario").Command.Execute(null);
            escena.Dibujar();

            var formulario = escena.Vista<PropietarioEdicionView>();
            Assert.Equal("Nuevo propietario", escena.Control<TextBlock>(formulario, "TituloFormulario").Text);
            Assert.False(escena.Boton(formulario, "Guardar").IsEnabled);
            Assert.Equal(Visibility.Collapsed, escena.Control<Button>(formulario, "BotonCambiarEstado").Visibility);

            escena.Control<TextBox>(formulario, "CampoNombre").Text = "María Torres";
            escena.Control<TextBox>(formulario, "CampoTelefono").Text = "123";
            escena.Dibujar();
            Assert.Equal(Visibility.Visible, escena.Control<TextBlock>(formulario, "ErrorTelefono").Visibility);

            escena.Control<TextBox>(formulario, "CampoTelefono").Text = "3151112233";
            escena.Dibujar();
            Assert.Equal(Visibility.Collapsed, escena.Control<TextBlock>(formulario, "ErrorTelefono").Visibility);
            Assert.True(escena.Boton(formulario, "Guardar").IsEnabled);

            escena.Boton(formulario, "Guardar").Command.Execute(null);
            escena.Dibujar();

            var lista = escena.Vista<PropietariosView>();
            var tabla = escena.Control<DataGrid>(lista, "TablaPropietarios");
            Assert.Single(tabla.Items);
            Assert.Equal("María Torres", ((PropietarioFila)tabla.Items[0]).NombreCompleto);
        });
    }

    [Fact]
    [Trait("Req", "RF-04")]
    public void RF04_EditarMuestraElBotonInactivarYCancelarVuelveALaLista()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            var tabla = escena.Control<DataGrid>(escena.Vista<PropietariosView>(), "TablaPropietarios");
            var primera = (PropietarioFila)tabla.Items[0];

            var editar = Descendientes(tabla).OfType<Button>().First(b => b.Content as string == "Editar");
            editar.Command.Execute(editar.CommandParameter);
            escena.Dibujar();

            var formulario = escena.Vista<PropietarioEdicionView>();
            Assert.Equal("Editar propietario", escena.Control<TextBlock>(formulario, "TituloFormulario").Text);
            Assert.Equal(primera.NombreCompleto, escena.Control<TextBox>(formulario, "CampoNombre").Text);
            var inactivar = escena.Control<Button>(formulario, "BotonCambiarEstado");
            Assert.Equal(Visibility.Visible, inactivar.Visibility);
            Assert.Equal("Inactivar", inactivar.Content);

            escena.Boton(formulario, "Cancelar").Command.Execute(null);
            escena.Dibujar();

            Assert.NotNull(escena.Vista<PropietariosView>());
        });
    }

    [Fact]
    [Trait("Req", "RF-16")]
    public void RF16_LaPantallaDeVeterinariosListaYPermiteAgregarUnoNuevo()
    {
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();

            escena.IrASeccion("Veterinarios");

            var vista = escena.Vista<VeterinariosView>();
            var tabla = escena.Control<DataGrid>(vista, "TablaVeterinarios");
            Assert.Equal(["Fabio", "William"], tabla.Items.Cast<VeterinarioFila>().Select(f => f.NombreCompleto).ToList());
            Assert.Equal(Visibility.Collapsed, escena.Control<Border>(vista, "PanelFormulario").Visibility);

            escena.Boton(vista, "Nuevo veterinario").Command.Execute(null);
            escena.Dibujar();
            Assert.Equal(Visibility.Visible, escena.Control<Border>(vista, "PanelFormulario").Visibility);
            Assert.Equal("Nuevo veterinario", escena.Control<TextBlock>(vista, "TituloFormulario").Text);
            Assert.False(escena.Boton(vista, "Guardar").IsEnabled);
            Assert.Equal(Visibility.Collapsed, escena.Control<CheckBox>(vista, "CampoActivo").Visibility);

            escena.Control<TextBox>(vista, "CampoNombre").Text = "Camila Rojas";
            escena.Dibujar();
            Assert.True(escena.Boton(vista, "Guardar").IsEnabled);
            escena.Boton(vista, "Guardar").Command.Execute(null);
            escena.Dibujar();

            Assert.Equal(3, tabla.Items.Count);
            Assert.Equal(Visibility.Collapsed, escena.Control<Border>(vista, "PanelFormulario").Visibility);
        });
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_LaPantallaDeMascotasMuestraLaTablaFiltraYAbreLaFichaConSuHistoria()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        SembrarHistoria(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();

            escena.IrASeccion("Mascotas");

            var vista = escena.Vista<MascotasView>();
            var tabla = escena.Control<DataGrid>(vista, "TablaMascotas");
            Assert.Equal(2, tabla.Items.Count);
            Assert.Equal(["Mascota", "Especie", "Propietario", "Edad", "Estado", string.Empty], tabla.Columns.Select(c => (string)c.Header).ToList());

            escena.Control<TextBox>(vista, "CampoBusqueda").Text = "Rock";
            escena.Dibujar();
            Assert.Single(tabla.Items);

            var abrir = Descendientes(tabla).OfType<Button>().First(b => b.Content as string == "Abrir ficha");
            abrir.Command.Execute(abrir.CommandParameter);
            escena.Dibujar();

            var ficha = escena.Vista<MascotaDetalleView>();
            Assert.Equal("Rocky", Texto(ficha, "TituloMascota"));
            Assert.Equal("Perro", Texto(ficha, "DatoEspecie"));
            Assert.Equal("Ana Pérez", Texto(ficha, "DatoPropietario"));
            Assert.False(string.IsNullOrWhiteSpace(Texto(ficha, "DatoEdad")));
            var historia = escena.Control<DataGrid>(ficha, "TablaHistoria");
            Assert.Equal(["Consulta: Descripción", "Rabia. Próximo refuerzo: 10/01/2027"], historia.Items.Cast<RegistroClinicoFila>().Select(r => r.Detalle).Reverse().ToList());
            Assert.Equal(Visibility.Collapsed, escena.Control<TextBlock>(ficha, "MensajeSinRegistros").Visibility);
        });
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_UnaFichaSinRegistrosAvisaYUnaInexistenteMuestraElMensaje()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            escena.IrASeccion("Mascotas");
            var abrir = Descendientes(escena.Vista<MascotasView>()).OfType<Button>().First(b => b.Content as string == "Abrir ficha");

            abrir.Command.Execute(abrir.CommandParameter);
            escena.Dibujar();

            var ficha = escena.Vista<MascotaDetalleView>();
            Assert.Equal(Visibility.Visible, escena.Control<TextBlock>(ficha, "MensajeSinRegistros").Visibility);

            var detalle = (MascotaDetalleViewModel)escena.Modelo.Navegacion.ViewModelActual!;
            detalle.Cargar(999);
            escena.Dibujar();
            Assert.Equal(Visibility.Visible, escena.Control<TextBlock>(ficha, "MensajeNoEncontrada").Visibility);
            Assert.Equal(Visibility.Collapsed, escena.Control<Grid>(ficha, "Ficha").Visibility);
        });
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_NuevaMascotaMuestraElSelectorDePropietarioCalculaLaEdadYAlGuardarVuelveALaLista()
    {
        using var bd = new BaseDatosTemporal();
        DatosDePrueba.CrearPropietario(bd, "Ana Pérez", "3001234567");
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            escena.IrASeccion("Mascotas");

            escena.Boton(escena.Vista<MascotasView>(), "Nueva mascota").Command.Execute(null);
            escena.Dibujar();

            var formulario = escena.Vista<MascotaEdicionView>();
            Assert.Equal("Nueva mascota", Texto(formulario, "TituloFormulario"));
            Assert.Equal(Visibility.Collapsed, escena.Control<TextBlock>(formulario, "AvisoSinPropietarios").Visibility);
            Assert.Equal(Visibility.Collapsed, escena.Control<Button>(formulario, "BotonCambiarEstado").Visibility);
            Assert.False(escena.Boton(formulario, "Guardar").IsEnabled);
            var propietario = escena.Control<ComboBox>(formulario, "CampoPropietario");
            Assert.Single(propietario.Items);

            propietario.SelectedIndex = 0;
            escena.Control<TextBox>(formulario, "CampoNombre").Text = "Luna";
            escena.Control<ComboBox>(formulario, "CampoEspecie").Text = "Gato";
            escena.Control<DatePicker>(formulario, "CampoFechaNacimiento").SelectedDate = DateTime.Today.AddYears(-1);
            escena.Control<TextBox>(formulario, "CampoPeso").Text = "abc";
            escena.Dibujar();
            Assert.Equal("1 año", Texto(formulario, "EdadCalculada"));
            Assert.Equal(Visibility.Visible, escena.Control<TextBlock>(formulario, "ErrorPeso").Visibility);

            escena.Control<TextBox>(formulario, "CampoPeso").Text = "4,2";
            escena.Dibujar();
            Assert.Equal(Visibility.Collapsed, escena.Control<TextBlock>(formulario, "ErrorPeso").Visibility);
            Assert.True(escena.Boton(formulario, "Guardar").IsEnabled);

            escena.Boton(formulario, "Guardar").Command.Execute(null);
            escena.Dibujar();

            var tabla = escena.Control<DataGrid>(escena.Vista<MascotasView>(), "TablaMascotas");
            Assert.Single(tabla.Items);
            Assert.Equal("Luna", ((MascotaFila)tabla.Items[0]).Nombre);
        });
    }

    [Fact]
    [Trait("Req", "RN-03")]
    public void RN03_SinPropietariosElFormularioDeMascotaLoAvisaYNoSePuedeGuardar()
    {
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            escena.IrASeccion("Mascotas");

            escena.Boton(escena.Vista<MascotasView>(), "Nueva mascota").Command.Execute(null);
            escena.Dibujar();

            var formulario = escena.Vista<MascotaEdicionView>();
            Assert.Equal(Visibility.Visible, escena.Control<TextBlock>(formulario, "AvisoSinPropietarios").Visibility);
            Assert.False(escena.Boton(formulario, "Guardar").IsEnabled);
        });
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_EditarUnaMascotaMuestraSusDatosYElBotonInactivar()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            escena.IrASeccion("Mascotas");
            var tabla = escena.Control<DataGrid>(escena.Vista<MascotasView>(), "TablaMascotas");
            var editar = Descendientes(tabla).OfType<Button>().First(b => b.Content as string == "Editar");

            editar.Command.Execute(editar.CommandParameter);
            escena.Dibujar();

            var formulario = escena.Vista<MascotaEdicionView>();
            Assert.Equal("Editar mascota", Texto(formulario, "TituloFormulario"));
            Assert.Equal("Misu", escena.Control<TextBox>(formulario, "CampoNombre").Text);
            Assert.Equal("12,5", escena.Control<TextBox>(formulario, "CampoPeso").Text);
            Assert.Equal(new DateTime(2020, 3, 15), escena.Control<DatePicker>(formulario, "CampoFechaNacimiento").SelectedDate);
            var inactivar = escena.Control<Button>(formulario, "BotonCambiarEstado");
            Assert.Equal(Visibility.Visible, inactivar.Visibility);
            Assert.Equal("Inactivar", inactivar.Content);

            escena.Boton(formulario, "Cancelar").Command.Execute(null);
            escena.Dibujar();
            Assert.NotNull(escena.Vista<MascotasView>());
        });
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_LaFichaOfreceRegistrarUnProcedimientoYAlGuardarApareceEnLaHistoria()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            escena.IrASeccion("Mascotas");
            AbrirFichaDe(escena, "Rocky");
            var ficha = escena.Vista<MascotaDetalleView>();
            var historia = escena.Control<DataGrid>(ficha, "TablaHistoria");
            Assert.Empty(historia.Items);
            Assert.True(escena.Boton(ficha, "Registrar vacuna").IsEnabled);

            escena.Boton(ficha, "Nuevo procedimiento").Command.Execute(null);
            escena.Dibujar();

            var formulario = escena.Vista<ProcedimientoEdicionView>();
            Assert.Equal("Nuevo procedimiento — Rocky", Texto(formulario, "TituloFormulario"));
            Assert.False(escena.Boton(formulario, "Guardar").IsEnabled);
            var veterinario = escena.Control<ComboBox>(formulario, "CampoVeterinario");
            Assert.Equal(2, veterinario.Items.Count);
            Assert.Equal(DateTime.Today, escena.Control<DatePicker>(formulario, "CampoFecha").SelectedDate);

            veterinario.SelectedIndex = 0;
            escena.Control<ComboBox>(formulario, "CampoTipo").Text = "Desparasitación";
            escena.Control<TextBox>(formulario, "CampoDescripcion").Text = "Desparasitación interna";
            escena.Control<TextBox>(formulario, "CampoPeso").Text = "abc";
            escena.Dibujar();
            Assert.Equal(Visibility.Visible, escena.Control<TextBlock>(formulario, "ErrorPeso").Visibility);
            Assert.True(escena.Boton(formulario, "Guardar").IsEnabled);

            escena.Control<TextBox>(formulario, "CampoPeso").Text = "12,5";
            escena.Dibujar();
            Assert.Equal(Visibility.Collapsed, escena.Control<TextBlock>(formulario, "ErrorPeso").Visibility);
            escena.Boton(formulario, "Guardar").Command.Execute(null);
            escena.Dibujar();

            var fichaDeNuevo = escena.Vista<MascotaDetalleView>();
            var tabla = escena.Control<DataGrid>(fichaDeNuevo, "TablaHistoria");
            var registro = Assert.IsType<RegistroClinicoFila>(Assert.Single(tabla.Items));
            Assert.Equal("Procedimiento", registro.Tipo);
            Assert.StartsWith("Desparasitación: Desparasitación interna", registro.Detalle);
            Assert.Equal(Visibility.Collapsed, escena.Control<TextBlock>(fichaDeNuevo, "MensajeSinRegistros").Visibility);
        });
    }

    [Fact]
    [Trait("Req", "RF-11")]
    public void RF11_LaFichaOfreceRegistrarUnaVacunaYAlGuardarApareceEnLaHistoria()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            escena.IrASeccion("Mascotas");
            AbrirFichaDe(escena, "Rocky");

            escena.Boton(escena.Vista<MascotaDetalleView>(), "Registrar vacuna").Command.Execute(null);
            escena.Dibujar();

            var formulario = escena.Vista<VacunacionEdicionView>();
            Assert.Equal("Registrar vacuna — Rocky", Texto(formulario, "TituloFormulario"));
            Assert.False(escena.Boton(formulario, "Guardar").IsEnabled);
            escena.Control<ComboBox>(formulario, "CampoVeterinario").SelectedIndex = 1;
            escena.Control<ComboBox>(formulario, "CampoVacuna").Text = "Rabia";
            escena.Control<DatePicker>(formulario, "CampoProximaFecha").SelectedDate = DateTime.Today.AddYears(1);
            escena.Control<TextBox>(formulario, "CampoLote").Text = "L-2026-01";
            escena.Dibujar();
            Assert.True(escena.Boton(formulario, "Guardar").IsEnabled);

            escena.Boton(formulario, "Guardar").Command.Execute(null);
            escena.Dibujar();

            var tabla = escena.Control<DataGrid>(escena.Vista<MascotaDetalleView>(), "TablaHistoria");
            var registro = Assert.IsType<RegistroClinicoFila>(Assert.Single(tabla.Items));
            Assert.Equal("Vacunación", registro.Tipo);
            Assert.StartsWith("Rabia. Lote: L-2026-01", registro.Detalle);
            Assert.Equal("William", registro.Veterinario);
        });
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_CancelarUnFormularioClinicoVuelveALaFichaSinCambios()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            escena.IrASeccion("Mascotas");
            AbrirFichaDe(escena, "Rocky");

            escena.Boton(escena.Vista<MascotaDetalleView>(), "Nuevo procedimiento").Command.Execute(null);
            escena.Dibujar();
            escena.Boton(escena.Vista<ProcedimientoEdicionView>(), "Cancelar").Command.Execute(null);
            escena.Dibujar();
            Assert.Empty(escena.Control<DataGrid>(escena.Vista<MascotaDetalleView>(), "TablaHistoria").Items);

            escena.Boton(escena.Vista<MascotaDetalleView>(), "Registrar vacuna").Command.Execute(null);
            escena.Dibujar();
            escena.Boton(escena.Vista<VacunacionEdicionView>(), "Cancelar").Command.Execute(null);
            escena.Dibujar();
            Assert.Empty(escena.Control<DataGrid>(escena.Vista<MascotaDetalleView>(), "TablaHistoria").Items);
        });
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_LaFichaGeneraLaVistaPreviaDelCarnetYDescargaElPdf()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        SembrarHistoria(bd);
        var carpeta = Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas", Guid.NewGuid().ToString("N"));
        try
        {
            HiloUi.Ejecutar(() =>
            {
                var escena = CrearEscena(bd);
                escena.IniciarSesionDesdeElModelo();
                escena.IrASeccion("Mascotas");
                AbrirFichaDe(escena, "Rocky");
                Assert.True(escena.Boton(escena.Vista<MascotaDetalleView>(), "Generar carnet").IsEnabled);

                escena.Boton(escena.Vista<MascotaDetalleView>(), "Generar carnet").Command.Execute(null);
                escena.Dibujar();

                var carnet = escena.Vista<CarnetView>();
                Assert.Equal(Visibility.Visible, escena.Control<ScrollViewer>(carnet, "VistaPrevia").Visibility);
                Assert.Equal(Visibility.Collapsed, escena.Control<Border>(carnet, "PanelAviso").Visibility);
                Assert.Contains("Rocky", escena.Control<ItemsControl>(carnet, "DatosMascota").Items.Cast<DatoCarnet>().Select(d => d.Valor));
                Assert.Equal(["Ana Pérez", "3001234567"], escena.Control<ItemsControl>(carnet, "DatosPropietario").Items.Cast<DatoCarnet>().Select(d => d.Valor).ToList());
                var tabla = escena.Control<DataGrid>(carnet, "TablaVacunas");
                Assert.Equal(["Vacuna", "Aplicada", "Próximo refuerzo", "Veterinario"], tabla.Columns.Select(c => (string)c.Header).ToList());
                Assert.Equal("Rabia", Assert.IsType<VacunaCarnetFila>(Assert.Single(tabla.Items)).Vacuna);
                Assert.StartsWith("Generado el ", Texto(carnet, "TextoFechaGeneracion"));
                var descargar = escena.Boton(carnet, "Descargar PDF");
                Assert.True(descargar.IsEnabled);

                var ruta = Path.Combine(carpeta, "carnet.pdf");
                escena.Dialogos.RutaDeGuardado = ruta;
                descargar.Command.Execute(null);

                Assert.True(new FileInfo(ruta).Length > 1024);
                Assert.Contains(ruta, Assert.Single(escena.Dialogos.Mensajes));

                escena.Boton(carnet, "Volver").Command.Execute(null);
                escena.Dibujar();
                Assert.NotNull(escena.Vista<MascotaDetalleView>());
            });
        }
        finally
        {
            if (Directory.Exists(carpeta))
            {
                Directory.Delete(carpeta, recursive: true);
            }
        }
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_UnaMascotaSinVacunasMuestraElAvisoEnLugarDelCarnet()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();
            escena.IrASeccion("Mascotas");
            AbrirFichaDe(escena, "Misu");

            escena.Boton(escena.Vista<MascotaDetalleView>(), "Generar carnet").Command.Execute(null);
            escena.Dibujar();

            var carnet = escena.Vista<CarnetView>();
            Assert.Equal(Visibility.Visible, escena.Control<Border>(carnet, "PanelAviso").Visibility);
            Assert.Contains("no tiene vacunas", Texto(carnet, "MensajeAviso"));
            Assert.Equal(Visibility.Collapsed, escena.Control<ScrollViewer>(carnet, "VistaPrevia").Visibility);
            Assert.False(escena.Boton(carnet, "Descargar PDF").IsEnabled);
        });
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_LaPantallaDeAlertasResaltaVencidasYProximasYEnviaElRecordatorio()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        SembrarAlertas(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();

            escena.IrASeccion("Alertas");

            var vista = escena.Vista<AlertasView>();
            var tabla = escena.Control<DataGrid>(vista, "TablaAlertas");
            Assert.Equal(["Mascota", "Propietario", "Tipo", "Fecha objetivo", "Estado", string.Empty], tabla.Columns.Select(c => (string)c.Header).ToList());
            Assert.Equal(["Vacunación: Moquillo", "Vacunación: Parvovirus"], tabla.Items.Cast<AlertaFila>().Select(a => a.Tipo).ToList());
            Assert.Equal((Color)Application.Current.Resources["ErrorColor"], ColorDeInsignia(tabla, "Vencida"));
            Assert.Equal((Color)Application.Current.Resources["AdvertenciaColor"], ColorDeInsignia(tabla, "Próxima"));
            Assert.Equal(Visibility.Collapsed, escena.Control<TextBlock>(vista, "MensajeSinAlertas").Visibility);
            Assert.Equal(Visibility.Collapsed, escena.Control<Border>(vista, "PanelRecordatorio").Visibility);

            var enviar = Descendientes(tabla).OfType<Button>().First(b => b.Content as string == "Enviar recordatorio");
            Assert.True(enviar.IsEnabled);
            enviar.Command.Execute(enviar.CommandParameter);
            escena.Dibujar();

            Assert.StartsWith("https://wa.me/573001234567?text=", Assert.Single(escena.Abridor.Enlaces));
            Assert.Equal(Visibility.Visible, escena.Control<Border>(vista, "PanelRecordatorio").Visibility);
            Assert.Equal("Recordatorio para Ana Pérez (Rocky)", Texto(vista, "TituloRecordatorio"));
            Assert.Contains("vacuna Moquillo", escena.Control<TextBox>(vista, "MensajeRecordatorio").Text);

            escena.Boton(vista, "Marcar como enviado").Command.Execute(null);
            escena.Dibujar();

            Assert.Equal(Visibility.Collapsed, escena.Control<Border>(vista, "PanelRecordatorio").Visibility);
            Assert.Equal("Vacunación: Parvovirus", Assert.IsType<AlertaFila>(Assert.Single(tabla.Items)).Tipo);
        });
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_SinAlertasLaPantallaLoInforma()
    {
        using var bd = new BaseDatosTemporal();
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            escena.IniciarSesionDesdeElModelo();

            escena.IrASeccion("Alertas");

            var vista = escena.Vista<AlertasView>();
            Assert.Empty(escena.Control<DataGrid>(vista, "TablaAlertas").Items);
            Assert.Equal(Visibility.Visible, escena.Control<TextBlock>(vista, "MensajeSinAlertas").Visibility);
        });
    }

    [Fact]
    [Trait("Req", "RNF-01")]
    public void RNF01_LasVistasNoGeneranErroresDeBinding()
    {
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        SembrarHistoria(bd);
        SembrarAlertas(bd);
        var escucha = new EscuchaDeEnlaces();
        PresentationTraceSources.Refresh();
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
        PresentationTraceSources.DataBindingSource.Listeners.Add(escucha);
        try
        {
            HiloUi.Ejecutar(() =>
            {
                var escena = CrearEscena(bd);
                escena.Modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
                escena.Modelo.Login.Contrasena = "incorrecta";
                escena.Modelo.Login.IniciarSesionCommand.Execute(null);
                escena.Dibujar();
                escena.IniciarSesionDesdeElModelo();

                var lista = escena.Vista<PropietariosView>();
                var tabla = escena.Control<DataGrid>(lista, "TablaPropietarios");
                tabla.SelectedItem = tabla.Items[0];
                escena.Dibujar();
                escena.Boton(lista, "Nuevo propietario").Command.Execute(null);
                escena.Dibujar();
                escena.Control<TextBox>(escena.Vista<PropietarioEdicionView>(), "CampoTelefono").Text = "12";
                escena.Dibujar();
                escena.Boton(escena.Vista<PropietarioEdicionView>(), "Cancelar").Command.Execute(null);
                escena.Dibujar();

                escena.IrASeccion("Mascotas");
                AbrirFichaDe(escena, "Rocky");
                var fichaRocky = escena.Vista<MascotaDetalleView>();
                escena.Boton(fichaRocky, "Nuevo procedimiento").Command.Execute(null);
                escena.Dibujar();
                var formularioProcedimiento = escena.Vista<ProcedimientoEdicionView>();
                escena.Control<ComboBox>(formularioProcedimiento, "CampoVeterinario").SelectedIndex = 0;
                escena.Control<TextBox>(formularioProcedimiento, "CampoPeso").Text = "abc";
                escena.Control<DatePicker>(formularioProcedimiento, "CampoProximaFecha").SelectedDate = DateTime.Today.AddMonths(3);
                escena.Dibujar();
                escena.Boton(formularioProcedimiento, "Guardar").Command.Execute(null);
                escena.Dibujar();
                escena.Control<TextBox>(formularioProcedimiento, "CampoPeso").Text = string.Empty;
                escena.Boton(formularioProcedimiento, "Cancelar").Command.Execute(null);
                escena.Dibujar();
                escena.Boton(escena.Vista<MascotaDetalleView>(), "Registrar vacuna").Command.Execute(null);
                escena.Dibujar();
                var formularioVacuna = escena.Vista<VacunacionEdicionView>();
                escena.Control<ComboBox>(formularioVacuna, "CampoVeterinario").SelectedIndex = 1;
                escena.Control<ComboBox>(formularioVacuna, "CampoVacuna").Text = "Rabia";
                escena.Dibujar();
                escena.Boton(formularioVacuna, "Guardar").Command.Execute(null);
                escena.Dibujar();
                ((MascotaDetalleViewModel)escena.Modelo.Navegacion.ViewModelActual!).Cargar(999);
                escena.Dibujar();
                escena.IrASeccion("Mascotas");
                escena.Boton(escena.Vista<MascotasView>(), "Nueva mascota").Command.Execute(null);
                escena.Dibujar();
                var formularioMascota = escena.Vista<MascotaEdicionView>();
                escena.Control<ComboBox>(formularioMascota, "CampoPropietario").SelectedIndex = 0;
                escena.Control<TextBox>(formularioMascota, "CampoPeso").Text = "abc";
                escena.Control<DatePicker>(formularioMascota, "CampoFechaNacimiento").SelectedDate = DateTime.Today.AddYears(-3);
                escena.Dibujar();
                escena.Boton(formularioMascota, "Cancelar").Command.Execute(null);
                escena.Dibujar();
                var editarMascota = Descendientes(escena.Vista<MascotasView>()).OfType<Button>().First(b => b.Content as string == "Editar");
                editarMascota.Command.Execute(editarMascota.CommandParameter);
                escena.Dibujar();

                escena.IrASeccion("Mascotas");
                AbrirFichaDe(escena, "Rocky");
                escena.Boton(escena.Vista<MascotaDetalleView>(), "Generar carnet").Command.Execute(null);
                escena.Dibujar();
                escena.Boton(escena.Vista<CarnetView>(), "Volver").Command.Execute(null);
                escena.Dibujar();
                escena.IrASeccion("Mascotas");
                AbrirFichaDe(escena, "Misu");
                escena.Boton(escena.Vista<MascotaDetalleView>(), "Generar carnet").Command.Execute(null);
                escena.Dibujar();

                escena.IrASeccion("Alertas");
                var vistaAlertas = escena.Vista<AlertasView>();
                var enviarAlerta = Descendientes(vistaAlertas).OfType<Button>().First(b => b.Content as string == "Enviar recordatorio");
                enviarAlerta.Command.Execute(enviarAlerta.CommandParameter);
                escena.Dibujar();
                escena.Boton(vistaAlertas, "Cerrar").Command.Execute(null);
                escena.Dibujar();
                enviarAlerta.Command.Execute(enviarAlerta.CommandParameter);
                escena.Dibujar();
                escena.Boton(vistaAlertas, "Marcar como enviado").Command.Execute(null);
                escena.Dibujar();

                escena.IrASeccion("Veterinarios");
                var veterinarios = escena.Vista<VeterinariosView>();
                escena.Boton(veterinarios, "Nuevo veterinario").Command.Execute(null);
                escena.Dibujar();
                var editar = Descendientes(veterinarios).OfType<Button>().First(b => b.Content as string == "Editar");
                editar.Command.Execute(editar.CommandParameter);
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
        using var bd = new BaseDatosTemporal();
        SembrarPropietarios(bd);
        SembrarHistoria(bd);
        SembrarAlertas(bd);
        HiloUi.Ejecutar(() =>
        {
            var escena = CrearEscena(bd);
            GuardarPng(escena.Raiz, "muestra-login.png");

            escena.Modelo.Login.Usuario = AutenticacionFalsa.UsuarioValido;
            escena.Modelo.Login.Contrasena = "incorrecta";
            escena.Modelo.Login.IniciarSesionCommand.Execute(null);
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-login-error.png");

            escena.IniciarSesionDesdeElModelo();
            var lista = escena.Vista<PropietariosView>();
            var tabla = escena.Control<DataGrid>(lista, "TablaPropietarios");
            tabla.SelectedItem = tabla.Items[0];
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-propietarios.png");

            escena.Boton(lista, "Nuevo propietario").Command.Execute(null);
            escena.Dibujar();
            var formulario = escena.Vista<PropietarioEdicionView>();
            escena.Control<TextBox>(formulario, "CampoNombre").Text = "María Torres";
            escena.Control<TextBox>(formulario, "CampoTelefono").Text = "12345";
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-propietario-form.png");

            escena.IrASeccion("Mascotas");
            var mascotas = escena.Vista<MascotasView>();
            GuardarPng(escena.Raiz, "muestra-mascotas.png");

            AbrirFichaDe(escena, "Rocky");
            GuardarPng(escena.Raiz, "muestra-ficha.png");

            escena.Boton(escena.Vista<MascotaDetalleView>(), "Nuevo procedimiento").Command.Execute(null);
            escena.Dibujar();
            var formularioProcedimiento = escena.Vista<ProcedimientoEdicionView>();
            escena.Control<ComboBox>(formularioProcedimiento, "CampoVeterinario").SelectedIndex = 0;
            escena.Control<ComboBox>(formularioProcedimiento, "CampoTipo").Text = "Desparasitación";
            escena.Control<TextBox>(formularioProcedimiento, "CampoDescripcion").Text = "Desparasitación interna con praziquantel";
            escena.Control<TextBox>(formularioProcedimiento, "CampoPeso").Text = "12,8";
            escena.Control<DatePicker>(formularioProcedimiento, "CampoProximaFecha").SelectedDate = DateTime.Today.AddMonths(3);
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-procedimiento-form.png");

            escena.Boton(formularioProcedimiento, "Cancelar").Command.Execute(null);
            escena.Dibujar();
            escena.Boton(escena.Vista<MascotaDetalleView>(), "Registrar vacuna").Command.Execute(null);
            escena.Dibujar();
            var formularioVacuna = escena.Vista<VacunacionEdicionView>();
            escena.Control<ComboBox>(formularioVacuna, "CampoVeterinario").SelectedIndex = 1;
            escena.Control<ComboBox>(formularioVacuna, "CampoVacuna").Text = "Parvovirus";
            escena.Control<DatePicker>(formularioVacuna, "CampoProximaFecha").SelectedDate = DateTime.Today.AddYears(1);
            escena.Control<TextBox>(formularioVacuna, "CampoLote").Text = "L-2026-07";
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-vacuna-form.png");
            escena.Boton(formularioVacuna, "Cancelar").Command.Execute(null);
            escena.Dibujar();

            escena.IrASeccion("Mascotas");
            escena.Boton(escena.Vista<MascotasView>(), "Nueva mascota").Command.Execute(null);
            escena.Dibujar();
            var formularioMascota = escena.Vista<MascotaEdicionView>();
            escena.Control<ComboBox>(formularioMascota, "CampoPropietario").SelectedIndex = 0;
            escena.Control<TextBox>(formularioMascota, "CampoNombre").Text = "Luna";
            escena.Control<ComboBox>(formularioMascota, "CampoEspecie").Text = "Gato";
            escena.Control<DatePicker>(formularioMascota, "CampoFechaNacimiento").SelectedDate = DateTime.Today.AddYears(-2).AddMonths(-3);
            escena.Control<TextBox>(formularioMascota, "CampoPeso").Text = "4,2";
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-mascota-form.png");

            escena.IrASeccion("Mascotas");
            AbrirFichaDe(escena, "Rocky");
            escena.Boton(escena.Vista<MascotaDetalleView>(), "Generar carnet").Command.Execute(null);
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-carnet-vista.png");

            escena.IrASeccion("Alertas");
            var vistaAlertas = escena.Vista<AlertasView>();
            GuardarPng(escena.Raiz, "muestra-alertas.png");
            var enviarAlerta = Descendientes(vistaAlertas).OfType<Button>().First(b => b.Content as string == "Enviar recordatorio");
            enviarAlerta.Command.Execute(enviarAlerta.CommandParameter);
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-alertas-recordatorio.png");

            escena.IrASeccion("Veterinarios");
            var veterinarios = escena.Vista<VeterinariosView>();
            var editar = Descendientes(veterinarios).OfType<Button>().First(b => b.Content as string == "Editar");
            editar.Command.Execute(editar.CommandParameter);
            escena.Dibujar();
            GuardarPng(escena.Raiz, "muestra-veterinarios.png");
        });

        Assert.True(File.Exists(Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas", "muestra-veterinarios.png")));
    }
}
