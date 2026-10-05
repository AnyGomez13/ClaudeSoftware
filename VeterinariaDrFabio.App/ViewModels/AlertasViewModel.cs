using System.Collections.ObjectModel;
using VeterinariaDrFabio.App.Infraestructura;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>
/// Pantalla P-11: vacunaciones y desparasitaciones próximas o vencidas, con el envío semiautomático del
/// recordatorio por el enlace gratuito de WhatsApp (RF-14, RF-15, RN-10, RN-11, RNF-04, RNF-06, CU-11, CU-12).
/// </summary>
public class AlertasViewModel : BaseViewModel
{
    private readonly IAlertaService _alertas;
    private readonly IRecordatorioService _recordatorios;
    private readonly IAbridorDeEnlaces _abridor;
    private readonly IDialogService _dialogos;
    private Recordatorio? _recordatorioPreparado;
    private string _destinatario = string.Empty;

    public AlertasViewModel(
        IAlertaService alertas,
        IRecordatorioService recordatorios,
        IAbridorDeEnlaces abridor,
        IDialogService dialogos)
    {
        _alertas = alertas;
        _recordatorios = recordatorios;
        _abridor = abridor;
        _dialogos = dialogos;
        EnviarRecordatorioCommand = new RelayCommand(parametro => EnviarRecordatorio(parametro as AlertaFila));
        MarcarEnviadoCommand = new RelayCommand(MarcarEnviado, () => HayRecordatorio);
        CerrarRecordatorioCommand = new RelayCommand(CerrarRecordatorio, () => HayRecordatorio);
        Cargar();
    }

    public ObservableCollection<AlertaFila> Alertas { get; } = [];

    public RelayCommand EnviarRecordatorioCommand { get; }

    public RelayCommand MarcarEnviadoCommand { get; }

    public RelayCommand CerrarRecordatorioCommand { get; }

    public bool SinAlertas => Alertas.Count == 0;

    /// <summary>Hay un recordatorio preparado cuyo mensaje se muestra mientras se envía por WhatsApp.</summary>
    public bool HayRecordatorio => _recordatorioPreparado is not null;

    public string TituloRecordatorio => $"Recordatorio para {_destinatario}";

    /// <summary>Mensaje armado por el sistema, tal como llegará al propietario.</summary>
    public string MensajeRecordatorio => _recordatorioPreparado?.Mensaje ?? string.Empty;

    /// <summary>Recarga la lista de alertas; las ya enviadas no vuelven a aparecer.</summary>
    public void Cargar()
    {
        Alertas.Clear();
        foreach (var alerta in _alertas.ProximasFechas())
        {
            Alertas.Add(new AlertaFila(alerta));
        }

        OnPropertyChanged(nameof(SinAlertas));
    }

    private void EnviarRecordatorio(AlertaFila? fila)
    {
        if (fila is null)
        {
            return;
        }

        var resultado = _recordatorios.Preparar(fila.Alerta);
        if (!resultado.Exito || resultado.Valor is null)
        {
            _dialogos.MostrarError("Recordatorio por WhatsApp", resultado.Mensaje);
            return;
        }

        _destinatario = $"{fila.Propietario} ({fila.Mascota})";
        Establecer(resultado.Valor);

        try
        {
            _abridor.Abrir(resultado.Valor.Enlace);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or ArgumentException)
        {
            _dialogos.MostrarError(
                "Recordatorio por WhatsApp",
                $"No se pudo abrir WhatsApp automáticamente: {ex.Message} El mensaje queda a la vista para enviarlo manualmente.");
        }
    }

    private void MarcarEnviado()
    {
        if (_recordatorioPreparado is null)
        {
            return;
        }

        var resultado = _recordatorios.MarcarEnviado(_recordatorioPreparado);
        if (!resultado.Exito)
        {
            _dialogos.MostrarError("Recordatorio por WhatsApp", resultado.Mensaje);
            return;
        }

        Establecer(null);
        Cargar();
    }

    private void CerrarRecordatorio() => Establecer(null);

    private void Establecer(Recordatorio? recordatorio)
    {
        _recordatorioPreparado = recordatorio;
        OnPropertyChanged(nameof(HayRecordatorio));
        OnPropertyChanged(nameof(TituloRecordatorio));
        OnPropertyChanged(nameof(MensajeRecordatorio));
        MarcarEnviadoCommand.RaiseCanExecuteChanged();
        CerrarRecordatorioCommand.RaiseCanExecuteChanged();
    }
}
