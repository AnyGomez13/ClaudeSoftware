using System.Globalization;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-07: ficha de la mascota con su edad calculada, su propietario y la historia clínica cronológica
/// de procedimientos y vacunaciones con el veterinario de cada registro (RF-06, RF-08, RF-10, RNF-09, CU-06).
/// </summary>
public class MascotaDetalleViewModel : BaseViewModel
{
    private const string SinDato = "—";

    private readonly IMascotaService _mascotas;
    private readonly INavigationService _navegacion;
    private int _mascotaId;
    private bool _encontrada;
    private string _nombre = string.Empty;
    private string _especie = string.Empty;
    private string _raza = SinDato;
    private string _sexo = SinDato;
    private string _nacimiento = string.Empty;
    private string _edad = string.Empty;
    private string _peso = string.Empty;
    private string _colorSenas = SinDato;
    private string _estado = string.Empty;
    private string _propietarioNombre = string.Empty;
    private string _propietarioTelefono = string.Empty;
    private IReadOnlyList<RegistroClinicoFila> _registros = [];

    public MascotaDetalleViewModel(IMascotaService mascotas, INavigationService navegacion)
    {
        _mascotas = mascotas;
        _navegacion = navegacion;
        NuevoProcedimientoCommand = new RelayCommand(NuevoProcedimiento, () => Encontrada);
        RegistrarVacunaCommand = new RelayCommand(RegistrarVacuna, () => Encontrada);
        GenerarCarnetCommand = new RelayCommand(GenerarCarnet, () => Encontrada);
        VolverCommand = new RelayCommand(_navegacion.Volver, () => _navegacion.PuedeVolver);
    }

    /// <summary>Regresa a la lista de mascotas conservando la búsqueda que había.</summary>
    public RelayCommand VolverCommand { get; }

    /// <summary>Abre la vista previa del carnet de vacunación (P-10) de esta mascota.</summary>
    public RelayCommand GenerarCarnetCommand { get; }

    /// <summary>Abre el formulario P-08 para registrar un procedimiento de esta mascota.</summary>
    public RelayCommand NuevoProcedimientoCommand { get; }

    /// <summary>Abre el formulario P-09 para registrar una vacuna de esta mascota.</summary>
    public RelayCommand RegistrarVacunaCommand { get; }

    public int MascotaId => _mascotaId;

    /// <summary>Falso si la mascota ya no existe; la vista muestra un aviso en lugar de la ficha.</summary>
    public bool Encontrada
    {
        get => _encontrada;
        private set
        {
            if (SetProperty(ref _encontrada, value))
            {
                OnPropertyChanged(nameof(NoEncontrada));
                NuevoProcedimientoCommand.RaiseCanExecuteChanged();
                RegistrarVacunaCommand.RaiseCanExecuteChanged();
                GenerarCarnetCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool NoEncontrada => !Encontrada;

    public string Nombre
    {
        get => _nombre;
        private set => SetProperty(ref _nombre, value);
    }

    public string Especie
    {
        get => _especie;
        private set => SetProperty(ref _especie, value);
    }

    public string Raza
    {
        get => _raza;
        private set => SetProperty(ref _raza, value);
    }

    public string Sexo
    {
        get => _sexo;
        private set => SetProperty(ref _sexo, value);
    }

    public string Nacimiento
    {
        get => _nacimiento;
        private set => SetProperty(ref _nacimiento, value);
    }

    /// <summary>Edad calculada con la fecha actual (RF-08).</summary>
    public string Edad
    {
        get => _edad;
        private set => SetProperty(ref _edad, value);
    }

    public string Peso
    {
        get => _peso;
        private set => SetProperty(ref _peso, value);
    }

    public string ColorSenas
    {
        get => _colorSenas;
        private set => SetProperty(ref _colorSenas, value);
    }

    public string Estado
    {
        get => _estado;
        private set => SetProperty(ref _estado, value);
    }

    public string PropietarioNombre
    {
        get => _propietarioNombre;
        private set => SetProperty(ref _propietarioNombre, value);
    }

    public string PropietarioTelefono
    {
        get => _propietarioTelefono;
        private set => SetProperty(ref _propietarioTelefono, value);
    }

    /// <summary>Procedimientos y vacunaciones en orden cronológico ascendente (RF-10).</summary>
    public IReadOnlyList<RegistroClinicoFila> Registros
    {
        get => _registros;
        private set
        {
            if (SetProperty(ref _registros, value))
            {
                OnPropertyChanged(nameof(SinRegistros));
            }
        }
    }

    public bool SinRegistros => Encontrada && Registros.Count == 0;

    /// <summary>Carga la ficha y la historia clínica de la mascota.</summary>
    public void Cargar(int mascotaId)
    {
        _mascotaId = mascotaId;
        OnPropertyChanged(nameof(MascotaId));
        Recargar();
    }

    /// <summary>Vuelve a leer la ficha y la historia, por ejemplo después de registrar un procedimiento o una vacuna.</summary>
    public void Recargar()
    {
        var historia = _mascotas.ObtenerHistoriaClinica(_mascotaId);
        if (historia is null)
        {
            Encontrada = false;
            Registros = [];
            return;
        }

        var mascota = historia.Mascota;
        Nombre = mascota.Nombre;
        Especie = mascota.Especie;
        Raza = ValorOGuion(mascota.Raza);
        Sexo = ValorOGuion(mascota.Sexo);
        Nacimiento = mascota.FechaNacimiento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            + (mascota.FechaNacimientoEstimada ? " (estimada)" : string.Empty);
        Edad = _mascotas.CalcularEdadLegible(mascota);
        Peso = $"{mascota.Peso.ToString("0.##", CultureInfo.GetCultureInfo("es-CO"))} kg";
        ColorSenas = ValorOGuion(mascota.ColorSenas);
        Estado = mascota.Activo ? "Activa" : "Inactiva";
        PropietarioNombre = mascota.Propietario?.NombreCompleto ?? string.Empty;
        PropietarioTelefono = mascota.Propietario?.Telefono ?? string.Empty;
        Registros = historia.Registros.Select(r => new RegistroClinicoFila(r)).ToList();
        Encontrada = true;
    }

    private void NuevoProcedimiento() =>
        _navegacion.NavegarA<ProcedimientoEdicionViewModel>(vm => vm.Nuevo(_mascotaId, Nombre, AlTerminarRegistro));

    private void RegistrarVacuna() =>
        _navegacion.NavegarA<VacunacionEdicionViewModel>(vm => vm.Nueva(_mascotaId, Nombre, AlTerminarRegistro, Especie));

    private void GenerarCarnet() =>
        _navegacion.NavegarA<CarnetViewModel>(vm => vm.Cargar(_mascotaId));

    /// <summary>Vuelve a la ficha y, si se guardó un registro, la recarga para que aparezca en la historia.</summary>
    private void AlTerminarRegistro(bool seGuardo)
    {
        _navegacion.Volver();
        if (seGuardo)
        {
            Recargar();
        }
    }

    private static string ValorOGuion(string? valor) => string.IsNullOrWhiteSpace(valor) ? SinDato : valor;
}
