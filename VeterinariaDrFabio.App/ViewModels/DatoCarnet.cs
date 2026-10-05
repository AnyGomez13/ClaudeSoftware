namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Par etiqueta y valor de la vista previa del carnet (P-10), por ejemplo "Especie: Perro".</summary>
public class DatoCarnet
{
    public DatoCarnet(string etiqueta, string valor)
    {
        Etiqueta = etiqueta;
        Valor = valor;
    }

    public string Etiqueta { get; }

    public string Valor { get; }
}
