using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IVacunacionService"/>
/// RF-11, RN-07, RN-08, RNF-07, RNF-08, CU-09.
public class VacunacionService : IVacunacionService
{
    private readonly IVacunacionRepository _vacunaciones;
    private readonly IMascotaRepository _mascotas;
    private readonly IVeterinarioRepository _veterinarios;

    public VacunacionService(
        IVacunacionRepository vacunaciones,
        IMascotaRepository mascotas,
        IVeterinarioRepository veterinarios)
    {
        _vacunaciones = vacunaciones;
        _mascotas = mascotas;
        _veterinarios = veterinarios;
    }

    public Resultado Registrar(Vacunacion vacunacion)
    {
        if (vacunacion.MascotaId <= 0 || _mascotas.ObtenerPorId(vacunacion.MascotaId) is null)
        {
            return Resultado.Error("Debe seleccionar una mascota existente.");
        }

        var errorVeterinario = ValidacionClinica.ValidarVeterinario(_veterinarios, vacunacion.VeterinarioId);
        if (errorVeterinario is not null)
        {
            return errorVeterinario;
        }

        var nombre = vacunacion.NombreVacuna?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            return Resultado.Error("El nombre de la vacuna es obligatorio.");
        }

        var errorFecha = ValidacionClinica.ValidarFecha(vacunacion.FechaAplicacion, "de aplicación");
        if (errorFecha is not null)
        {
            return errorFecha;
        }

        if (vacunacion.ProximaFecha is { } refuerzo && refuerzo.Date < vacunacion.FechaAplicacion.Date)
        {
            return Resultado.Error("La próxima fecha de refuerzo no puede ser anterior a la fecha de aplicación.");
        }

        vacunacion.NombreVacuna = nombre;
        vacunacion.FechaAplicacion = vacunacion.FechaAplicacion.Date;
        vacunacion.ProximaFecha = vacunacion.ProximaFecha?.Date;
        vacunacion.Lote = ValidacionClinica.NuloSiVacio(vacunacion.Lote);
        vacunacion.Observaciones = ValidacionClinica.NuloSiVacio(vacunacion.Observaciones);
        vacunacion.Mascota = null;
        vacunacion.Veterinario = null;

        _vacunaciones.Agregar(vacunacion);
        return Resultado.Ok();
    }
}
