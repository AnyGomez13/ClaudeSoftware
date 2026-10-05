using System.Globalization;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Fila de la tabla de vacunas del carnet (P-10): vacuna, fecha, refuerzo y veterinario.</summary>
public class VacunaCarnetFila
{
    public VacunaCarnetFila(Vacunacion vacunacion)
    {
        Vacuna = vacunacion.NombreVacuna;
        Aplicada = vacunacion.FechaAplicacion.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        Refuerzo = vacunacion.ProximaFecha is { } proxima ? proxima.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "—";
        Veterinario = vacunacion.Veterinario?.NombreCompleto ?? "—";
    }

    public string Vacuna { get; }

    public string Aplicada { get; }

    public string Refuerzo { get; }

    public string Veterinario { get; }
}
