using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Shell principal (P-02): muestra el login mientras no haya sesión y, después, el menú lateral
/// Propietarios · Mascotas · Alertas · Veterinarios con el área de contenido (RF-01, RN-13).
/// </summary>
public class MainViewModel : BaseViewModel
{
    private readonly IAutenticacionService _autenticacion;
    private readonly Dictionary<string, Action> _destinos;
    private bool _sesionActiva;
    private string _seccionActual = string.Empty;

    public MainViewModel(IAutenticacionService autenticacion, LoginViewModel login, INavigationService navegacion)
    {
        _autenticacion = autenticacion;
        Login = login;
        Navegacion = navegacion;
        Secciones =
        [
            new ItemMenu("Propietarios"),
            new ItemMenu("Mascotas"),
            new ItemMenu("Alertas"),
            new ItemMenu("Veterinarios"),
        ];

        // Cada sección del menú abre su pantalla.
        _destinos = new Dictionary<string, Action>
        {
            ["Propietarios"] = () => Navegacion.NavegarASeccion<PropietariosViewModel>(),
            ["Mascotas"] = () => Navegacion.NavegarASeccion<MascotasViewModel>(),
            ["Alertas"] = () => Navegacion.NavegarASeccion<AlertasViewModel>(),
            ["Veterinarios"] = () => Navegacion.NavegarASeccion<VeterinariosViewModel>(),
        };

        NavegarSeccionCommand = new RelayCommand(parametro => IrASeccion(parametro as string), _ => SesionActiva);
        CerrarSesionCommand = new RelayCommand(CerrarSesion, () => SesionActiva);
        Login.SesionIniciada += OnSesionIniciada;
    }

    public string NombreSistema => "Clínica Veterinaria Dr. Fabio";

    public LoginViewModel Login { get; }

    public INavigationService Navegacion { get; }

    public IReadOnlyList<ItemMenu> Secciones { get; }

    public RelayCommand NavegarSeccionCommand { get; }

    public RelayCommand CerrarSesionCommand { get; }

    public bool SesionActiva
    {
        get => _sesionActiva;
        private set
        {
            if (SetProperty(ref _sesionActiva, value))
            {
                OnPropertyChanged(nameof(MostrarLogin));
                NavegarSeccionCommand.RaiseCanExecuteChanged();
                CerrarSesionCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool MostrarLogin => !SesionActiva;

    public string NombreUsuario => _autenticacion.NombreUsuario ?? string.Empty;

    public string SeccionActual
    {
        get => _seccionActual;
        private set => SetProperty(ref _seccionActual, value);
    }

    private void OnSesionIniciada()
    {
        SesionActiva = true;
        OnPropertyChanged(nameof(NombreUsuario));
        IrASeccion(Secciones[0].Titulo);
    }

    /// <summary>Marca la sección en el menú y muestra su pantalla.</summary>
    private void IrASeccion(string? titulo)
    {
        var seccion = Secciones.FirstOrDefault(s => s.Titulo == titulo);
        if (!SesionActiva || seccion is null)
        {
            return;
        }

        foreach (var item in Secciones)
        {
            item.EsActual = item == seccion;
        }

        SeccionActual = seccion.Titulo;

        if (_destinos.TryGetValue(seccion.Titulo, out var abrir))
        {
            abrir();
        }
        else
        {
            Navegacion.Limpiar();
        }
    }

    private void CerrarSesion()
    {
        _autenticacion.CerrarSesion();
        Navegacion.Limpiar();
        Login.Reiniciar();
        foreach (var item in Secciones)
        {
            item.EsActual = false;
        }

        SeccionActual = string.Empty;
        SesionActiva = false;
        OnPropertyChanged(nameof(NombreUsuario));
    }
}
