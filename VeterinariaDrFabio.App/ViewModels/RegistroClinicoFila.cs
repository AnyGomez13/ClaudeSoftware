using System.Globalization;
using VeterinariaDrFabio.Dominio.Modelos;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Línea de la historia clínica en la ficha (P-07): fecha, tipo, detalle y veterinario.</summary>
public class RegistroClinicoFila
{
    public RegistroClinicoFila(RegistroClinicoItem registro)
    {
        Fecha = registro.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        Tipo = registro.Origen == "Vacunacion" ? "Vacunación" : registro.Origen;
        Detalle = registro.Detalle;
        Veterinario = registro.Veterinario;
    }

    public string Fecha { get; }

    public string Tipo { get; }

    public string Detalle { get; }

    public string Veterinario { get; }
}
