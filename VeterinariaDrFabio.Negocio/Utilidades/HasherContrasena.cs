using System.Security.Cryptography;
using System.Text;

namespace VeterinariaDrFabio.Negocio.Utilidades;

/// <summary>
/// Hash de contraseñas con PBKDF2 (HMAC-SHA256, 100 000 iteraciones); hash y sal en base64 (RNF-05, RN-01, SUP-D03).
/// </summary>
public class HasherContrasena
{
    private const int Iteraciones = 100_000;
    private const int BytesSal = 16;
    private const int BytesHash = 32;

    /// <summary>Genera una sal aleatoria nueva y devuelve el hash de la clave; la sal sale por <paramref name="salt"/>.</summary>
    public string Hashear(string clave, out string salt)
    {
        if (string.IsNullOrEmpty(clave))
        {
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(clave));
        }

        var bytesSal = RandomNumberGenerator.GetBytes(BytesSal);
        salt = Convert.ToBase64String(bytesSal);
        return Convert.ToBase64String(Derivar(clave, bytesSal));
    }

    /// <summary>Indica si la clave corresponde al hash y la sal guardados. Devuelve falso ante datos mal formados.</summary>
    public bool Verificar(string clave, string hash, string salt)
    {
        if (string.IsNullOrEmpty(clave) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
        {
            return false;
        }

        try
        {
            var hashEsperado = Convert.FromBase64String(hash);
            var bytesSal = Convert.FromBase64String(salt);
            return CryptographicOperations.FixedTimeEquals(Derivar(clave, bytesSal), hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Derivar(string clave, byte[] sal) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(clave), sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);
}
