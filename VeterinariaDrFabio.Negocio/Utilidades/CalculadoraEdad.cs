namespace VeterinariaDrFabio.Negocio.Utilidades;

/// <summary>
/// Calcula la edad de la mascota desde su fecha de nacimiento; nunca se guarda (RF-08, RN-05, SUP-D15).
/// </summary>
public class CalculadoraEdad
{
    /// <summary>Meses cumplidos entre el nacimiento y hoy.</summary>
    public int EnMeses(DateTime nacimiento) => EnMeses(nacimiento, DateTime.Today);

    /// <summary>
    /// Meses cumplidos entre el nacimiento y la fecha dada. Un mes se cumple en el mismo día del mes;
    /// si ese día no existe (por ejemplo 31 o 29 de febrero) se cumple el último día del mes.
    /// </summary>
    public int EnMeses(DateTime nacimiento, DateTime hoy)
    {
        var fechaNacimiento = nacimiento.Date;
        var fechaActual = hoy.Date;

        if (fechaNacimiento > fechaActual)
        {
            throw new ArgumentOutOfRangeException(nameof(nacimiento), "La fecha de nacimiento no puede ser futura.");
        }

        var meses = ((fechaActual.Year - fechaNacimiento.Year) * 12) + fechaActual.Month - fechaNacimiento.Month;
        if (fechaNacimiento.AddMonths(meses) > fechaActual)
        {
            meses--;
        }

        return meses;
    }

    /// <summary>Edad en texto: "Menos de 1 mes", "5 meses", "1 año" o "2 años 3 meses".</summary>
    public string Legible(DateTime nacimiento) => Legible(nacimiento, DateTime.Today);

    public string Legible(DateTime nacimiento, DateTime hoy)
    {
        var meses = EnMeses(nacimiento, hoy);
        if (meses < 1)
        {
            return "Menos de 1 mes";
        }

        var anios = meses / 12;
        var mesesRestantes = meses % 12;

        if (anios == 0)
        {
            return Meses(mesesRestantes);
        }

        var texto = anios == 1 ? "1 año" : $"{anios} años";
        return mesesRestantes == 0 ? texto : $"{texto} {Meses(mesesRestantes)}";
    }

    private static string Meses(int cantidad) => cantidad == 1 ? "1 mes" : $"{cantidad} meses";
}
