using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IProcedimientoService"/>
/// RF-09, RN-07, RN-08, RNF-07, RNF-08, CU-08.
public class ProcedimientoService : IProcedimientoService
{
    private readonly IProcedimientoRepository _procedimientos;
    private readonly IMascotaRepository _mascotas;
    private readonly IVeterinarioRepository _veterinarios;

    public ProcedimientoService(
        IProcedimientoRepository procedimientos,
        IMascotaRepository mascotas,
        IVeterinarioRepository veterinarios)
    {
        _procedimientos = procedimientos;
        _mascotas = mascotas;
        _veterinarios = veterinarios;
    }

    public Resultado Registrar(Procedimiento procedimiento)
    {
        if (procedimiento.MascotaId <= 0 || _mascotas.ObtenerPorId(procedimiento.MascotaId) is null)
        {
            return Resultado.Error("Debe seleccionar una mascota existente.");
        }

        var errorVeterinario = ValidacionClinica.ValidarVeterinario(_veterinarios, procedimiento.VeterinarioId);
        if (errorVeterinario is not null)
        {
            return errorVeterinario;
        }

        var errorFecha = ValidacionClinica.ValidarFecha(procedimiento.Fecha, "del procedimiento");
        if (errorFecha is not null)
        {
            return errorFecha;
        }

        var tipo = procedimiento.TipoProcedimiento?.Trim();
        if (string.IsNullOrEmpty(tipo))
        {
            return Resultado.Error("El tipo de procedimiento es obligatorio.");
        }

        var descripcion = procedimiento.Descripcion?.Trim();
        if (string.IsNullOrEmpty(descripcion))
        {
            return Resultado.Error("La descripción o diagnóstico es obligatoria.");
        }

        if (procedimiento.PesoEnElMomento is { } peso && (!(peso > 0) || double.IsInfinity(peso)))
        {
            return Resultado.Error("El peso debe ser mayor a cero (en kilogramos).");
        }

        if (procedimiento.ProximaFechaRecomendada is { } proxima && proxima.Date < procedimiento.Fecha.Date)
        {
            return Resultado.Error("La próxima fecha recomendada no puede ser anterior a la fecha del procedimiento.");
        }

        procedimiento.Fecha = procedimiento.Fecha.Date;
        procedimiento.TipoProcedimiento = tipo;
        procedimiento.Descripcion = descripcion;
        procedimiento.Tratamiento = ValidacionClinica.NuloSiVacio(procedimiento.Tratamiento);
        procedimiento.ProximaFechaRecomendada = procedimiento.ProximaFechaRecomendada?.Date;
        procedimiento.Mascota = null;
        procedimiento.Veterinario = null;

        _procedimientos.Agregar(procedimiento);
        return Resultado.Ok();
    }
}
