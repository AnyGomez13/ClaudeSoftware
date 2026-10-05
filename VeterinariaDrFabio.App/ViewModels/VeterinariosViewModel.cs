using System.Collections.ObjectModel;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-12: lista de veterinarios con alta y edición en un panel de la misma pantalla
/// (R-03, RF-16, SUP-06, CU-13). Fabio y William vienen pre-registrados.
/// </summary>
public class VeterinariosViewModel : BaseViewModel
{
    private readonly IVeterinarioService _veterinarios;
    private bool _enEdicion;
    private int _id;
    private bool _esNuevo = true;
    private string _nombreCompleto = string.Empty;
    private string _registroProfesional = string.Empty;
    private bool _activo = true;
    private string _mensajeError = string.Empty;

    public VeterinariosViewModel(IVeterinarioService veterinarios)
    {
        _veterinarios = veterinarios;
        NuevoCommand = new RelayCommand(Nuevo);
        EditarCommand = new RelayCommand(parametro => Editar(parametro as VeterinarioFila));
        GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        CancelarCommand = new RelayCommand(Cancelar);
        Cargar();
    }

    public ObservableCollection<VeterinarioFila> Veterinarios { get; } = [];

    public RelayCommand NuevoCommand { get; }

    public RelayCommand EditarCommand { get; }

    public RelayCommand GuardarCommand { get; }

    public RelayCommand CancelarCommand { get; }

    /// <summary>Verdadero mientras el panel de alta o edición está abierto.</summary>
    public bool EnEdicion
    {
        get => _enEdicion;
        private set
        {
            if (SetProperty(ref _enEdicion, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TituloFormulario => _esNuevo ? "Nuevo veterinario" : "Editar veterinario";

    /// <summary>El estado solo se cambia en un veterinario existente; uno nuevo siempre queda activo.</summary>
    public bool PuedeCambiarEstado => !_esNuevo;

    public string NombreCompleto
    {
        get => _nombreCompleto;
        set
        {
            if (SetProperty(ref _nombreCompleto, value))
            {
                GuardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RegistroProfesional
    {
        get => _registroProfesional;
        set => SetProperty(ref _registroProfesional, value);
    }

    public bool Activo
    {
        get => _activo;
        set => SetProperty(ref _activo, value);
    }

    public string MensajeError
    {
        get => _mensajeError;
        private set
        {
            if (SetProperty(ref _mensajeError, value))
            {
                OnPropertyChanged(nameof(TieneError));
            }
        }
    }

    public bool TieneError => MensajeError.Length > 0;

    private void Cargar()
    {
        Veterinarios.Clear();
        foreach (var veterinario in _veterinarios.ListarTodos())
        {
            Veterinarios.Add(new VeterinarioFila(veterinario));
        }
    }

    private void Nuevo() => AbrirFormulario(new Veterinario(), esNuevo: true);

    private void Editar(VeterinarioFila? fila)
    {
        if (fila is not null)
        {
            AbrirFormulario(fila.Veterinario, esNuevo: false);
        }
    }

    private void AbrirFormulario(Veterinario veterinario, bool esNuevo)
    {
        _id = veterinario.Id;
        _esNuevo = esNuevo;
        NombreCompleto = veterinario.NombreCompleto;
        RegistroProfesional = veterinario.RegistroProfesional ?? string.Empty;
        Activo = veterinario.Activo;
        MensajeError = string.Empty;
        OnPropertyChanged(nameof(TituloFormulario));
        OnPropertyChanged(nameof(PuedeCambiarEstado));
        EnEdicion = true;
    }

    private bool PuedeGuardar() => EnEdicion && !string.IsNullOrWhiteSpace(NombreCompleto);

    private void Guardar()
    {
        var veterinario = new Veterinario
        {
            Id = _id,
            NombreCompleto = NombreCompleto,
            RegistroProfesional = RegistroProfesional,
            Activo = Activo,
        };

        var resultado = _esNuevo ? _veterinarios.Registrar(veterinario) : _veterinarios.Editar(veterinario);
        if (!resultado.Exito)
        {
            MensajeError = resultado.Mensaje;
            return;
        }

        Cargar();
        Cancelar();
    }

    private void Cancelar()
    {
        MensajeError = string.Empty;
        EnEdicion = false;
    }
}
