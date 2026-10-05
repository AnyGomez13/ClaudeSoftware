using System.Globalization;
using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Modelos;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="ICarnetService"/>
/// RF-12, RF-13, RN-09, RNF-07, CU-10.
public class CarnetService : ICarnetService
{
    private readonly IMascotaRepository _mascotas;
    private readonly IVacunacionRepository _vacunaciones;
    private readonly GeneradorCarnetPdf _generadorPdf;

    public CarnetService(
        IMascotaRepository mascotas,
        IVacunacionRepository vacunaciones,
        GeneradorCarnetPdf generadorPdf)
    {
        _mascotas = mascotas;
        _vacunaciones = vacunaciones;
        _generadorPdf = generadorPdf;
    }

    public Resultado<CarnetDigital> Generar(int mascotaId)
    {
        var mascota = _mascotas.ObtenerPorId(mascotaId);
        if (mascota?.Propietario is null)
        {
            return Resultado<CarnetDigital>.Error("La mascota no existe.");
        }

        var vacunas = _vacunaciones.ListarPorMascota(mascotaId);
        if (vacunas.Count == 0)
        {
            return Resultado<CarnetDigital>.Error("La mascota no tiene vacunas registradas; no hay carnet para generar.");
        }

        return Resultado<CarnetDigital>.Ok(new CarnetDigital
        {
            Mascota = mascota,
            Propietario = mascota.Propietario,
            Vacunas = vacunas,
            FechaGeneracion = DateTime.Now,
        });
    }

    public Resultado<string> ExportarPdf(CarnetDigital carnet, string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            return Resultado<string>.Error("Debe elegir dónde guardar el archivo PDF.");
        }

        try
        {
            return Resultado<string>.Ok(_generadorPdf.Generar(carnet, ruta));
        }
        catch (ArgumentException ex) when (carnet.Vacunas.Count == 0)
        {
            return Resultado<string>.Error(ex.Message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return Resultado<string>.Error($"No se pudo guardar el PDF en esa ubicación: {ex.Message}");
        }
    }

    public string NombreArchivoSugerido(CarnetDigital carnet)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var nombre = new string(carnet.Mascota.Nombre.Where(c => !invalidos.Contains(c)).ToArray()).Trim().Replace(' ', '_');
        if (nombre.Length == 0)
        {
            nombre = "Mascota";
        }

        return $"Carnet_{nombre}_{carnet.FechaGeneracion.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.pdf";
    }
}
