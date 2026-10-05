using System.Globalization;
using VeterinariaDrFabio.Dominio.Modelos;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Fila de la tabla de alertas (P-11): mascota, propietario, tipo y fecha objetivo, resaltada si está vencida.</summary>
public class AlertaFila
{
    public AlertaFila(AlertaProximaFecha alerta)
    {
        Alerta = alerta;
    }

    public AlertaProximaFecha Alerta { get; }

    public string Mascota => Alerta.MascotaNombre;

    public string Propietario => Alerta.PropietarioNombre;

    /// <summary>"Vacunación: Rabia" o "Desparasitación".</summary>
    public string Tipo => Alerta.Tipo == "Vacunacion" ? $"Vacunación: {Alerta.Detalle}" : "Desparasitación";

    public string FechaObjetivo => Alerta.FechaObjetivo.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    /// <summary>Vencida se resalta como error y Próxima como advertencia.</summary>
    public bool EsVencida => Alerta.Vencida;

    public string Estado => EsVencida ? "Vencida" : "Próxima";
}
