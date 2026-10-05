using Microsoft.EntityFrameworkCore;

namespace VeterinariaDrFabio.Datos.Repositorios;

internal static class ContextoExtensiones
{
    /// <summary>Marca la entidad como modificada (adjuntándola si hace falta) y guarda los cambios.</summary>
    public static void GuardarModificacion<T>(this DbContext contexto, T entidad)
        where T : class
    {
        if (contexto.Entry(entidad).State == EntityState.Detached)
        {
            contexto.Attach(entidad);
        }

        contexto.Entry(entidad).State = EntityState.Modified;
        contexto.SaveChanges();
    }
}
