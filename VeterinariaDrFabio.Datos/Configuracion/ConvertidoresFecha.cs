using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VeterinariaDrFabio.Datos.Configuracion;

/// <summary>
/// Guarda las fechas como TEXT ISO-8601: yyyy-MM-dd para fechas y yyyy-MM-ddTHH:mm:ss para marcas de tiempo (SUP-D01).
/// </summary>
internal static class ConvertidoresFecha
{
    private const string FormatoFecha = "yyyy-MM-dd";
    private const string FormatoMarcaTiempo = "yyyy-MM-ddTHH:mm:ss";

    private static readonly ValueConverter<DateTime, string> Fecha = new(
        valor => valor.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        texto => DateTime.ParseExact(texto, FormatoFecha, CultureInfo.InvariantCulture));

    private static readonly ValueConverter<DateTime, string> MarcaTiempo = new(
        valor => valor.ToString(FormatoMarcaTiempo, CultureInfo.InvariantCulture),
        texto => DateTime.ParseExact(texto, FormatoMarcaTiempo, CultureInfo.InvariantCulture));

    public static PropertyBuilder<DateTime> ComoFecha(this PropertyBuilder<DateTime> propiedad) =>
        propiedad.HasConversion(Fecha).HasColumnType("TEXT");

    public static PropertyBuilder<DateTime?> ComoFecha(this PropertyBuilder<DateTime?> propiedad) =>
        propiedad.HasConversion(Fecha).HasColumnType("TEXT");

    public static PropertyBuilder<DateTime?> ComoMarcaTiempo(this PropertyBuilder<DateTime?> propiedad) =>
        propiedad.HasConversion(MarcaTiempo).HasColumnType("TEXT");
}
