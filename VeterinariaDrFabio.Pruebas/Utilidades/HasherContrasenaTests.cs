using System.Security.Cryptography;
using System.Text;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Pruebas.Utilidades;

/// <summary>Pruebas de <see cref="HasherContrasena"/> (RNF-05, RN-01, SUP-D03).</summary>
public class HasherContrasenaTests
{
    private readonly HasherContrasena _hasher = new();

    [Fact]
    [Trait("Req", "RNF-05")]
    public void RNF05_VerificarAceptaLaClaveCorrectaYRechazaLaIncorrecta()
    {
        var hash = _hasher.Hashear("Clave-Segura-1", out var salt);

        Assert.True(_hasher.Verificar("Clave-Segura-1", hash, salt));
        Assert.False(_hasher.Verificar("clave-segura-1", hash, salt));
        Assert.False(_hasher.Verificar("otra", hash, salt));
    }

    [Fact]
    [Trait("Req", "RNF-05")]
    public void RNF05_CadaHashUsaUnaSalDistintaYNoGuardaLaClaveEnTextoPlano()
    {
        var hash1 = _hasher.Hashear("misma-clave", out var salt1);
        var hash2 = _hasher.Hashear("misma-clave", out var salt2);

        Assert.NotEqual(salt1, salt2);
        Assert.NotEqual(hash1, hash2);
        Assert.DoesNotContain("misma-clave", hash1);
        Assert.DoesNotContain("misma-clave", salt1);
    }

    [Fact]
    [Trait("Req", "SUP-D03")]
    public void SUPD03_UsaPbkdf2Sha256Con100000IteracionesYBase64()
    {
        var hash = _hasher.Hashear("Clave-Segura-1", out var salt);

        var bytesSal = Convert.FromBase64String(salt);
        var esperado = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes("Clave-Segura-1"), bytesSal, 100_000, HashAlgorithmName.SHA256, 32);

        Assert.Equal(16, bytesSal.Length);
        Assert.Equal(Convert.ToBase64String(esperado), hash);
    }

    [Theory]
    [Trait("Req", "RNF-05")]
    [InlineData("", "hash", "salt")]
    [InlineData("clave", "", "salt")]
    [InlineData("clave", "hash", "")]
    [InlineData("clave", "no es base64!", "tampoco")]
    public void RNF05_VerificarDevuelveFalsoConDatosVaciosOMalFormados(string clave, string hash, string salt)
    {
        Assert.False(_hasher.Verificar(clave, hash, salt));
    }

    [Theory]
    [Trait("Req", "RNF-05")]
    [InlineData("")]
    [InlineData(null)]
    public void RNF05_HashearRechazaUnaContrasenaVacia(string? clave)
    {
        Assert.Throws<ArgumentException>(() => _hasher.Hashear(clave!, out _));
    }
}
