using VeterinariaDrFabio.Dominio.Modelos;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>Carnet digital de vacunación y su exportación a PDF (RF-12, RF-13, RN-09, CU-10).</summary>
public interface ICarnetService
{
    /// <summary>
    /// Arma el carnet con los datos de la mascota y su propietario y todas sus vacunas en orden cronológico.
    /// Devuelve un error si la mascota no existe o no tiene vacunas registradas.
    /// </summary>
    Resultado<CarnetDigital> Generar(int mascotaId);

    /// <summary>Escribe el carnet como PDF en la ruta elegida y devuelve esa ruta.</summary>
    Resultado<string> ExportarPdf(CarnetDigital carnet, string ruta);

    /// <summary>Nombre de archivo sugerido para guardar el PDF, por ejemplo "Carnet_Rocky_2026-10-05.pdf".</summary>
    string NombreArchivoSugerido(CarnetDigital carnet);
}
