using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IVeterinarioService"/>
/// R-03, RF-16, RN-07, CU-13.
public class VeterinarioService : IVeterinarioService
{
    private readonly IVeterinarioRepository _veterinarios;

    public VeterinarioService(IVeterinarioRepository veterinarios)
    {
        _veterinarios = veterinarios;
    }

    public List<Veterinario> ListarActivos() => _veterinarios.ListarActivos();

    public List<Veterinario> ListarTodos() => _veterinarios.ListarTodos();

    public Resultado Registrar(Veterinario veterinario)
    {
        var nombre = veterinario.NombreCompleto?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            return Resultado.Error("El nombre del veterinario es obligatorio.");
        }

        veterinario.NombreCompleto = nombre;
        veterinario.RegistroProfesional = Normalizar(veterinario.RegistroProfesional);
        _veterinarios.Agregar(veterinario);
        return Resultado.Ok();
    }

    public Resultado Editar(Veterinario veterinario)
    {
        var nombre = veterinario.NombreCompleto?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            return Resultado.Error("El nombre del veterinario es obligatorio.");
        }

        var existente = _veterinarios.ObtenerPorId(veterinario.Id);
        if (existente is null)
        {
            return Resultado.Error("El veterinario no existe.");
        }

        existente.NombreCompleto = nombre;
        existente.RegistroProfesional = Normalizar(veterinario.RegistroProfesional);
        existente.Activo = veterinario.Activo;
        _veterinarios.Actualizar(existente);
        return Resultado.Ok();
    }

    private static string? Normalizar(string? texto)
    {
        var limpio = texto?.Trim();
        return string.IsNullOrEmpty(limpio) ? null : limpio;
    }
}
