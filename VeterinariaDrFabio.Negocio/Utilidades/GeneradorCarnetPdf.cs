using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VeterinariaDrFabio.Dominio.Modelos;

namespace VeterinariaDrFabio.Negocio.Utilidades;

/// <summary>
/// Genera el carnet de vacunación en PDF con QuestPDF, licencia Community, gratuita para la clínica (RF-13, RN-09, RNF-06, SUP-D08).
/// </summary>
public class GeneradorCarnetPdf
{
    private const string ColorPrimario = "#2A9D8F";
    private const string ColorTextoPrincipal = "#1D2733";
    private const string ColorTextoSecundario = "#5B6B7B";
    private const string ColorBorde = "#E1E8ED";
    private const string ColorFondo = "#F7F9FA";

    static GeneradorCarnetPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Escribe el carnet en <paramref name="ruta"/> (crea las carpetas que falten y reemplaza el archivo si existe)
    /// y devuelve esa misma ruta.
    /// </summary>
    /// <exception cref="ArgumentException">La ruta está vacía o el carnet no tiene vacunas.</exception>
    public string Generar(CarnetDigital carnet, string ruta)
    {
        ArgumentNullException.ThrowIfNull(carnet);
        if (string.IsNullOrWhiteSpace(ruta))
        {
            throw new ArgumentException("La ruta del archivo PDF es obligatoria.", nameof(ruta));
        }

        if (carnet.Vacunas.Count == 0)
        {
            throw new ArgumentException("El carnet no tiene vacunas registradas.", nameof(carnet));
        }

        var directorio = Path.GetDirectoryName(Path.GetFullPath(ruta));
        if (!string.IsNullOrEmpty(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        ConstruirDocumento(carnet).GeneratePdf(ruta);
        return ruta;
    }

    /// <summary>Arma el documento del carnet; es interno para poder dibujarlo como imagen en las pruebas.</summary>
    internal static IDocument ConstruirDocumento(CarnetDigital carnet)
    {
        return Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(36);
            pagina.DefaultTextStyle(estilo => estilo.FontSize(10.5f).FontColor(ColorTextoPrincipal));

            pagina.Header().Column(columna =>
            {
                columna.Item().Text("Clínica Veterinaria Dr. Fabio").FontSize(11).SemiBold().FontColor(ColorPrimario);
                columna.Item().Text("Carnet de vacunación").FontSize(22).Bold();
                columna.Item().PaddingTop(6).LineHorizontal(1).LineColor(ColorBorde);
            });

            pagina.Content().PaddingVertical(16).Column(columna =>
            {
                columna.Spacing(16);
                columna.Item().Row(fila =>
                {
                    fila.RelativeItem().Element(c => Tarjeta(c, "Mascota", DatosMascota(carnet)));
                    fila.ConstantItem(16);
                    fila.RelativeItem().Element(c => Tarjeta(c, "Propietario", DatosPropietario(carnet)));
                });
                columna.Item().Element(c => TablaVacunas(c, carnet));
            });

            pagina.Footer().AlignCenter().Text(texto =>
            {
                texto.DefaultTextStyle(estilo => estilo.FontSize(9).FontColor(ColorTextoSecundario));
                texto.Span($"Generado el {Fecha(carnet.FechaGeneracion)} · Página ");
                texto.CurrentPageNumber();
                texto.Span(" de ");
                texto.TotalPages();
            });
        }));
    }

    private static List<(string Etiqueta, string Valor)> DatosMascota(CarnetDigital carnet)
    {
        var mascota = carnet.Mascota;
        var nacimiento = Fecha(mascota.FechaNacimiento) + (mascota.FechaNacimientoEstimada ? " (estimada)" : string.Empty);
        var datos = new List<(string, string)>
        {
            ("Nombre", mascota.Nombre),
            ("Especie", mascota.Especie),
        };
        if (!string.IsNullOrEmpty(mascota.Raza))
        {
            datos.Add(("Raza", mascota.Raza));
        }

        if (!string.IsNullOrEmpty(mascota.Sexo))
        {
            datos.Add(("Sexo", mascota.Sexo));
        }

        datos.Add(("Nacimiento", nacimiento));
        datos.Add(("Edad", new CalculadoraEdad().Legible(mascota.FechaNacimiento, carnet.FechaGeneracion)));
        datos.Add(("Peso", $"{mascota.Peso.ToString("0.##", CultureInfo.GetCultureInfo("es-CO"))} kg"));
        return datos;
    }

    private static List<(string Etiqueta, string Valor)> DatosPropietario(CarnetDigital carnet) =>
    [
        ("Nombre", carnet.Propietario.NombreCompleto),
        ("Teléfono", carnet.Propietario.Telefono),
    ];

    private static void Tarjeta(IContainer contenedor, string titulo, List<(string Etiqueta, string Valor)> datos)
    {
        contenedor.Background(ColorFondo).Border(1).BorderColor(ColorBorde).Padding(10).Column(columna =>
        {
            columna.Spacing(3);
            columna.Item().Text(titulo).SemiBold().FontColor(ColorPrimario);
            foreach (var (etiqueta, valor) in datos)
            {
                columna.Item().Text(texto =>
                {
                    texto.Span($"{etiqueta}: ").FontColor(ColorTextoSecundario);
                    texto.Span(valor);
                });
            }
        });
    }

    private static void TablaVacunas(IContainer contenedor, CarnetDigital carnet)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.RelativeColumn(3);
                columnas.RelativeColumn(2);
                columnas.RelativeColumn(2);
                columnas.RelativeColumn(2);
                columnas.RelativeColumn(2);
            });

            tabla.Header(cabecera =>
            {
                foreach (var titulo in new[] { "Vacuna", "Aplicada", "Próximo refuerzo", "Veterinario", "Lote" })
                {
                    cabecera.Cell().Background(ColorPrimario).Padding(6)
                        .Text(titulo).Bold().FontColor(Colors.White);
                }
            });

            foreach (var vacuna in carnet.Vacunas.OrderBy(v => v.FechaAplicacion))
            {
                var valores = new[]
                {
                    vacuna.NombreVacuna,
                    Fecha(vacuna.FechaAplicacion),
                    vacuna.ProximaFecha is { } refuerzo ? Fecha(refuerzo) : "—",
                    vacuna.Veterinario?.NombreCompleto ?? "—",
                    string.IsNullOrEmpty(vacuna.Lote) ? "—" : vacuna.Lote,
                };

                foreach (var valor in valores)
                {
                    tabla.Cell().BorderBottom(1).BorderColor(ColorBorde).Padding(6).Text(valor);
                }
            }
        });
    }

    private static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
