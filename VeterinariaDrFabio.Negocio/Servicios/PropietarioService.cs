using System.Text.RegularExpressions;
using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IPropietarioService"/>
/// RF-02, RF-03, RF-04, RN-04, RN-14, SUP-08, CU-02, CU-03, CU-04.
public partial class PropietarioService : IPropietarioService
{
    private readonly IPropietarioRepository _propietarios;

    public PropietarioService(IPropietarioRepository propietarios)
    {
        _propietarios = propietarios;
    }

    public Resultado Registrar(Propietario propietario)
    {
        var error = Normalizar(propietario);
        if (error is not null)
        {
            return error;
        }

        _propietarios.Agregar(propietario);
        return Resultado.Ok();
    }

    public Resultado Editar(Propietario propietario)
    {
        var existente = _propietarios.ObtenerPorId(propietario.Id);
        if (existente is null)
        {
            return Resultado.Error("El propietario no existe.");
        }

        var error = Normalizar(propietario);
        if (error is not null)
        {
            return error;
        }

        existente.NombreCompleto = propietario.NombreCompleto;
        existente.Telefono = propietario.Telefono;
        existente.Documento = propietario.Documento;
        existente.Correo = propietario.Correo;
        existente.Direccion = propietario.Direccion;
        existente.Activo = propietario.Activo;
        _propietarios.Actualizar(existente);
        return Resultado.Ok();
    }

    /// <summary>Indica si el texto es un celular colombiano: 10 dígitos que inicia en 3, sin prefijo (RN-14).</summary>
    public static bool EsCelularColombiano(string? telefono) =>
        !string.IsNullOrEmpty(telefono) && CelularColombiano().IsMatch(telefono);

    public List<Propietario> Buscar(string texto) => _propietarios.Buscar(texto);

    public Propietario? ObtenerPorId(int id) => _propietarios.ObtenerPorId(id);

    /// <summary>Limpia los textos y valida las reglas mínimas; devuelve el error o nulo si todo está bien.</summary>
    private static Resultado? Normalizar(Propietario propietario)
    {
        var nombre = propietario.NombreCompleto?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            return Resultado.Error("El nombre completo del propietario es obligatorio.");
        }

        var telefono = propietario.Telefono?.Trim();
        if (string.IsNullOrEmpty(telefono))
        {
            return Resultado.Error("El teléfono del propietario es obligatorio.");
        }

        if (!EsCelularColombiano(telefono))
        {
            return Resultado.Error("El teléfono debe ser un celular colombiano de 10 dígitos que inicia en 3 (ejemplo: 3001234567).");
        }

        propietario.NombreCompleto = nombre;
        propietario.Telefono = telefono;
        propietario.Documento = NuloSiVacio(propietario.Documento);
        propietario.Correo = NuloSiVacio(propietario.Correo);
        propietario.Direccion = NuloSiVacio(propietario.Direccion);
        return null;
    }

    private static string? NuloSiVacio(string? texto)
    {
        var limpio = texto?.Trim();
        return string.IsNullOrEmpty(limpio) ? null : limpio;
    }

    [GeneratedRegex(@"^3[0-9]{9}\z")]
    private static partial Regex CelularColombiano();
}
