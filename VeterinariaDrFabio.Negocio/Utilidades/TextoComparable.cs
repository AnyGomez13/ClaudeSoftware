using System.Globalization;
using System.Text;

namespace VeterinariaDrFabio.Negocio.Utilidades;

/// <summary>Normaliza textos escritos a mano para compararlos sin importar tildes, mayúsculas ni espacios en los extremos.</summary>
public static class TextoComparable
{
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var descompuesto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sinTildes = descompuesto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(sinTildes.ToArray()).ToLowerInvariant();
    }
}
