using VeterinariaDrFabio.Datos.Repositorios;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>Validaciones compartidas por los registros clínicos: procedimientos y vacunaciones (RN-07, RN-08).</summary>
internal static class ValidacionClinica
{
    /// <summary>Todo registro clínico lleva un veterinario existente y activo (R-03, RN-07).</summary>
    public static Resultado? ValidarVeterinario(IVeterinarioRepository veterinarios, int veterinarioId)
    {
        if (veterinarioId <= 0)
        {
            return Resultado.Error("Debe seleccionar el veterinario que realizó el registro.");
        }

        var veterinario = veterinarios.ObtenerPorId(veterinarioId);
        if (veterinario is null)
        {
            return Resultado.Error("El veterinario seleccionado no existe.");
        }

        return veterinario.Activo ? null : Resultado.Error("El veterinario seleccionado no está activo.");
    }

    /// <summary>La fecha es obligatoria y no puede ser futura: la historia registra actos ya realizados.</summary>
    public static Resultado? ValidarFecha(DateTime fecha, string descripcion)
    {
        if (fecha == default)
        {
            return Resultado.Error($"La fecha {descripcion} es obligatoria.");
        }

        return fecha.Date > DateTime.Today ? Resultado.Error($"La fecha {descripcion} no puede ser futura.") : null;
    }

    public static string? NuloSiVacio(string? texto)
    {
        var limpio = texto?.Trim();
        return string.IsNullOrEmpty(limpio) ? null : limpio;
    }
}
