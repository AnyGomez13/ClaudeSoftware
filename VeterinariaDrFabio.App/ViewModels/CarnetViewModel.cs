using System.Globalization;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Modelos;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-10: vista previa del carnet de vacunación y descarga en PDF para enviárselo al cliente
/// (RF-12, RF-13, RN-09, RNF-07, CU-10).
/// </summary>
public class CarnetViewModel : BaseViewModel
{
    public const string FiltroPdf = "Archivo PDF (*.pdf)|*.pdf";

    private readonly ICarnetService _carnets;
    private readonly IDialogService _dialogos;
    private readonly INavigationService _navegacion;
    private readonly CalculadoraEdad _calculadoraEdad;
    private CarnetDigital? _carnet;
    private string _aviso = string.Empty;
    private IReadOnlyList<DatoCarnet> _datosMascota = [];
    private IReadOnlyList<DatoCarnet> _datosPropietario = [];
    private IReadOnlyList<VacunaCarnetFila> _vacunas = [];
    private string _fechaGeneracion = string.Empty;

    public CarnetViewModel(
        ICarnetService carnets,
        IDialogService dialogos,
        INavigationService navegacion,
        CalculadoraEdad calculadoraEdad)
    {
        _carnets = carnets;
        _dialogos = dialogos;
        _navegacion = navegacion;
        _calculadoraEdad = calculadoraEdad;
        DescargarPdfCommand = new RelayCommand(DescargarPdf, () => HayCarnet);
        VolverCommand = new RelayCommand(_navegacion.Volver, () => _navegacion.PuedeVolver);
    }

    public RelayCommand DescargarPdfCommand { get; }

    /// <summary>Regresa a la ficha de la mascota.</summary>
    public RelayCommand VolverCommand { get; }

    public bool HayCarnet => _carnet is not null;

    /// <summary>Explica por qué no hay carnet: la mascota no tiene vacunas o ya no existe.</summary>
    public string Aviso
    {
        get => _aviso;
        private set
        {
            if (SetProperty(ref _aviso, value))
            {
                OnPropertyChanged(nameof(TieneAviso));
            }
        }
    }

    public bool TieneAviso => Aviso.Length > 0;

    public IReadOnlyList<DatoCarnet> DatosMascota
    {
        get => _datosMascota;
        private set => SetProperty(ref _datosMascota, value);
    }

    public IReadOnlyList<DatoCarnet> DatosPropietario
    {
        get => _datosPropietario;
        private set => SetProperty(ref _datosPropietario, value);
    }

    public IReadOnlyList<VacunaCarnetFila> Vacunas
    {
        get => _vacunas;
        private set => SetProperty(ref _vacunas, value);
    }

    /// <summary>Texto "Generado el dd/MM/aaaa" con la fecha en que se armó el carnet.</summary>
    public string FechaGeneracion
    {
        get => _fechaGeneracion;
        private set => SetProperty(ref _fechaGeneracion, value);
    }

    /// <summary>Arma el carnet de la mascota; sin vacunas registradas muestra un aviso en lugar de la vista previa.</summary>
    public void Cargar(int mascotaId)
    {
        var resultado = _carnets.Generar(mascotaId);
        if (!resultado.Exito || resultado.Valor is null)
        {
            _carnet = null;
            Aviso = resultado.Mensaje;
            DatosMascota = [];
            DatosPropietario = [];
            Vacunas = [];
            FechaGeneracion = string.Empty;
        }
        else
        {
            _carnet = resultado.Valor;
            Aviso = string.Empty;
            DatosMascota = ConstruirDatosMascota(_carnet);
            DatosPropietario =
            [
                new DatoCarnet("Nombre", _carnet.Propietario.NombreCompleto),
                new DatoCarnet("Teléfono", _carnet.Propietario.Telefono),
            ];
            Vacunas = _carnet.Vacunas.Select(v => new VacunaCarnetFila(v)).ToList();
            FechaGeneracion = $"Generado el {_carnet.FechaGeneracion.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
        }

        OnPropertyChanged(nameof(HayCarnet));
        DescargarPdfCommand.RaiseCanExecuteChanged();
    }

    private IReadOnlyList<DatoCarnet> ConstruirDatosMascota(CarnetDigital carnet)
    {
        var mascota = carnet.Mascota;
        var datos = new List<DatoCarnet>
        {
            new("Nombre", mascota.Nombre),
            new("Especie", mascota.Especie),
        };
        if (!string.IsNullOrEmpty(mascota.Raza))
        {
            datos.Add(new DatoCarnet("Raza", mascota.Raza));
        }

        if (!string.IsNullOrEmpty(mascota.Sexo))
        {
            datos.Add(new DatoCarnet("Sexo", mascota.Sexo));
        }

        datos.Add(new DatoCarnet(
            "Nacimiento",
            mascota.FechaNacimiento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + (mascota.FechaNacimientoEstimada ? " (estimada)" : string.Empty)));
        datos.Add(new DatoCarnet("Edad", _calculadoraEdad.Legible(mascota.FechaNacimiento, carnet.FechaGeneracion)));
        datos.Add(new DatoCarnet("Peso", $"{mascota.Peso.ToString("0.##", CultureInfo.GetCultureInfo("es-CO"))} kg"));
        return datos;
    }

    private void DescargarPdf()
    {
        if (_carnet is null)
        {
            return;
        }

        var ruta = _dialogos.PedirRutaDeGuardado(_carnets.NombreArchivoSugerido(_carnet), FiltroPdf);
        if (string.IsNullOrWhiteSpace(ruta))
        {
            return;
        }

        var resultado = _carnets.ExportarPdf(_carnet, ruta);
        if (resultado.Exito)
        {
            _dialogos.MostrarMensaje("Carnet de vacunación", $"El carnet se guardó en:{Environment.NewLine}{resultado.Valor}");
        }
        else
        {
            _dialogos.MostrarError("Carnet de vacunación", resultado.Mensaje);
        }
    }
}
