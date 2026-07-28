using System.Security.Cryptography;

namespace Maestro_AI.Services;

/// <summary>Optional profile signing via ECDsa P-256.</summary>
public static class ProfileSigner
{
    /// <summary>Sign data with a private key (PEM or hex). Returns Base64 signature.</summary>
    public static string Sign(string data, string privateKeyHex)
    {
        using var ecdsa = ECDsa.Create();
        try { ecdsa.ImportECPrivateKey(Convert.FromHexString(privateKeyHex), out _); }
        catch { ecdsa.ImportFromPem(privateKeyHex); }
        var sig = ecdsa.SignData(System.Text.Encoding.UTF8.GetBytes(data), HashAlgorithmName.SHA256);
        return Convert.ToBase64String(sig);
    }

    /// <summary>Verify a signature with a public key (PEM or hex).</summary>
    public static bool Verify(string data, string signatureBase64, string publicKeyHex)
    {
        try
        {
            using var ecdsa = ECDsa.Create();
            try { ecdsa.ImportSubjectPublicKeyInfo(Convert.FromHexString(publicKeyHex), out _); }
            catch { ecdsa.ImportFromPem(publicKeyHex); }
            return ecdsa.VerifyData(System.Text.Encoding.UTF8.GetBytes(data),
                Convert.FromBase64String(signatureBase64), HashAlgorithmName.SHA256);
        }
        catch { return false; }
    }

    /// <summary>Generate a new key pair. Returns (privateKeyHex, publicKeyHex).</summary>
    public static (string priv, string pub) GenerateKeys()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var priv = Convert.ToHexString(ecdsa.ExportECPrivateKey());
        var pub = Convert.ToHexString(ecdsa.ExportSubjectPublicKeyInfo());
        return (priv, pub);
    }
}
