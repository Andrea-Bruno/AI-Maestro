using System.Security.Cryptography;
using System.Text;

namespace Maestro_AI.Services;

using Maestro_AI.Models;

public static class CertificateGenerator
{
    private static readonly List<BatchCertificate> Certificates = [];

    /// <summary>Generate a batch certificate with hash chain and QR token.</summary>
    public static BatchCertificate Generate(string roastUUID, string greenJson, string roastParamsJson,
        string postRoastJson, double tasterScore, string privateKeyHex)
    {
        var cert = new BatchCertificate
        {
            RoastUUID = roastUUID,
            GreenHash = HashJson(greenJson),
            RoastParamsHash = HashJson(roastParamsJson),
            PostRoastHash = HashJson(postRoastJson),
            TasterScore = tasterScore,
            QrToken = Guid.NewGuid().ToString("N")
        };

        // Sign the certificate data
        var data = $"{cert.BatchId}|{cert.GreenHash}|{cert.RoastParamsHash}|{cert.PostRoastHash}|{cert.Timestamp:O}";
        cert.Signature = SignData(data, privateKeyHex);

        Certificates.Add(cert);
        return cert;
    }

    /// <summary>Verify a QR token (single-reveal). Returns the certificate or null if already revealed.</summary>
    public static BatchCertificate? VerifyQrToken(string token)
    {
        var cert = Certificates.FirstOrDefault(c => c.QrToken == token);
        if (cert == null) return null;
        if (cert.QrRevealed) return null; // single-reveal: already scanned
        cert.QrRevealed = true;
        cert.QrRevealedAt = DateTime.UtcNow.ToString("O");
        return cert;
    }

    /// <summary>Get certificate by batch ID.</summary>
    public static BatchCertificate? Get(string batchId) =>
        Certificates.FirstOrDefault(c => c.BatchId == batchId);

    private static string HashJson(string json) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));

    private static string SignData(string data, string keyHex)
    {
        using var ecdsa = ECDsa.Create();
        try { ecdsa.ImportECPrivateKey(Convert.FromHexString(keyHex), out _); }
        catch { ecdsa.ImportFromPem(keyHex); }
        return Convert.ToBase64String(ecdsa.SignData(Encoding.UTF8.GetBytes(data), HashAlgorithmName.SHA256));
    }
}
