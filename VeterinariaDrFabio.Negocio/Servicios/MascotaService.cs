using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IMascotaService"/>
/// RF-05, RF-07, RF-08, RN-02, RN-03, RN-06, SUP-04, SUP-08, CU-05, CU-07.
public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _mascotas;
    private readonly IPropietarioRepository _propietarios;
    private readonly CalculadoraEdad _calculadoraEdad;

    public MascotaService(
        IMascotaRepository mascotas,
        IPropietarioRepository propietarios,
        CalculadoraEdad calculadoraEdad)
    {
        _mascotas = mascotas;
        _propietarios = propietarios;
        _calculadoraEdad = calculadoraEdad;
    }

    public Resultado Registrar(Mascota mascota)
    {
        var error = Normalizar(mascota);
        if (error is not null)
        {
            return error;
        }

        _mascotas.Agregar(mascota);
        return Resultado.Ok();
    }

    public Resultado Editar(Mascota mascota)
    {
        var existente = _mascotas.ObtenerPorId(mascota.Id);
        if (existente is null)
        {
            return Resultado.Error("La mascota no existe.");
        }

        var error = Normalizar(mascota);
        if (error is not null)
        {
            return error;
        }

        existente.PropietarioId = mascota.PropietarioId;
        existente.Nombre = mascota.Nombre;
        existente.Especie = mascota.Especie;
        existente.Raza = mascota.Raza;
        existente.Sexo = mascota.Sexo;
        existente.FechaNacimiento = mascota.FechaNacimiento;
        existente.FechaNacimientoEstimada = mascota.FechaNacimientoEstimada;
        existente.Peso = mascota.Peso;
        existente.ColorSenas = mascota.ColorSenas;
        existente.Activo = mascota.Activo;
        _mascotas.Actualizar(existente);
        return Resultado.Ok();
    }

    public List<Mascota> Buscar(string texto) => _mascotas.Buscar(texto);

    public Mascota? ObtenerPorId(int id) => _mascotas.ObtenerPorId(id);

    public int CalcularEdadMeses(Mascota mascota) => _calculadoraEdad.EnMeses(mascota.FechaNacimiento);

    public string CalcularEdadLegible(Mascota mascota) => _calculadoraEdad.Legible(mascota.FechaNacimiento);

    /// <summary>Limpia los textos y valida las reglas; devuelve el error o nulo si todo está bien.</summary>
    private Resultado? Normalizar(Mascota mascota)
    {
        if (mascota.PropietarioId <= 0 || _propietarios.ObtenerPorId(mascota.PropietarioId) is null)
        {
            return Resultado.Error("Debe seleccionar un propietario existente.");
        }

        var nombre = mascota.Nombre?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            return Resultado.Error("El nombre de la mascota es obligatorio.");
        }

        var especie = mascota.Especie?.Trim();
        if (string.IsNullOrEmpty(especie))
        {
            return Resultado.Error("La especie es obligatoria.");
        }

        if (mascota.FechaNacimiento == default)
        {
            return Resultado.Error("La fecha de nacimiento es obligatoria (puede ser estimada).");
        }

        if (mascota.FechaNacimiento.Date > DateTime.Today)
        {
            return Resultado.Error("La fecha de nacimiento no puede ser futura.");
        }

        if (!(mascota.Peso > 0) || double.IsInfinity(mascota.Peso))
        {
            return Resultado.Error("El peso debe ser mayor a cero (en kilogramos).");
        }

        if (!TryNormalizarSexo(mascota.Sexo, out var sexo))
        {
            return Resultado.Error("El sexo debe ser Macho o Hembra.");
        }

        mascota.Nombre = nombre;
        mascota.Especie = especie;
        mascota.FechaNacimiento = mascota.FechaNacimiento.Date;
        mascota.Sexo = sexo;
        mascota.Raza = NuloSiVacio(mascota.Raza);
        mascota.ColorSenas = NuloSiVacio(mascota.ColorSenas);
        return null;
    }

    private static bool TryNormalizarSexo(string? texto, out string? sexo)
    {
        var limpio = texto?.Trim();
        if (string.IsNullOrEmpty(limpio))
        {
            sexo = null;
            return true;
        }

        sexo = limpio.ToLowerInvariant() switch
        {
            "macho" => "Macho",
            "hembra" => "Hembra",
            _ => null,
        };
        return sexo is not null;
    }

    private static string? NuloSiVacio(string? texto)
    {
        var limpio = texto?.Trim();
        return string.IsNullOrEmpty(limpio) ? null : limpio;
    }
}
