using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

public static class AiAPI
{
    public static string GenerateRoastProfile(string greenJson, string goalJson)
    {
        Log.LogStep("GenerateRoastProfile: generating AI profile");
        var green = JsonSerializer.Deserialize<GreenAnalysis>(greenJson);
        var goal = JsonSerializer.Deserialize<RoastGoal>(goalJson);
        if (green == null || goal == null) return "{\"error\":\"Invalid input\"}";

        var profile = AiProfileGenerator.Generate(green, goal);
        Log.LogStep("GenerateRoastProfile: done");
        return JsonSerializer.Serialize(profile, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true
        });
    }

    public static string PredictOutcome(string greenJson, string goalJson)
    {
        Log.LogStep("PredictOutcome");
        var result = AiProfileGenerator.Predict(
            JsonSerializer.Deserialize<GreenAnalysis>(greenJson) ?? new(),
            JsonSerializer.Deserialize<RoastGoal>(goalJson) ?? new());
        Log.LogStep("PredictOutcome: " + (result?.Length > 100 ? result[..100] + "..." : result));
        return result;
    }

    public static string GenerateCertificate(string roastUUID, string greenJson,
        string roastParamsJson, string postRoastJson, double tasterScore, string privateKeyHex)
    {
        Log.LogStep($"GenerateCertificate: roast={roastUUID}, score={tasterScore}");
        var cert = CertificateGenerator.Generate(roastUUID, greenJson, roastParamsJson,
            postRoastJson, tasterScore, privateKeyHex);

        // Generate QR code payload using QRCoder
        string qrPayload;
        using (var qr = new QRCoder.QRCodeGenerator())
        {
            var data = qr.CreateQrCode(
                $"{{\"batch\":\"{cert.BatchId}\",\"token\":\"{cert.QrToken}\",\"url\":\"/api/VerifyQRToken\"}}",
                QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var png = new QRCoder.PngByteQRCode(data);
            qrPayload = Convert.ToBase64String(png.GetGraphic(4));
        }

        return JsonSerializer.Serialize(new
        {
            cert.BatchId, cert.RoastUUID, cert.Signature, cert.Timestamp,
            qrCodeBase64 = qrPayload, cert.QrToken
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static string VerifyQrToken(string token)
    {
        var cert = CertificateGenerator.VerifyQrToken(token);
        if (cert == null) return "{\"valid\":false,\"message\":\"Token invalid or already used\"}";
        return JsonSerializer.Serialize(new
        {
            valid = true, batchId = cert.BatchId,
            roastUUID = cert.RoastUUID, tasterScore = cert.TasterScore,
            timestamp = cert.Timestamp
        });
    }

    public static string RecordSupplyChainEvent(string batchId, string eventType,
        string actor, string location, double quantityKg, string signature)
    {
        SupplyChainLedger.Record(batchId, eventType, actor, location, quantityKg, signature);
        return "{\"success\":true}";
    }

    public static string GetSupplyChainTrace(string batchId)
    {
        var trace = SupplyChainLedger.GetTrace(batchId);
        var circ = SupplyChainLedger.CirculatingTokens(batchId);
        return JsonSerializer.Serialize(new
        {
            batchId, circulatingKg = circ, events = trace
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static string GetCertificate(string batchId)
    {
        var cert = CertificateGenerator.Get(batchId);
        return cert != null
            ? JsonSerializer.Serialize(cert)
            : "{\"error\":\"Certificate not found\"}";
    }

    public static string DetectCrack(double amplitude, double timeSec, string? freqBandsJson = null)
    {
        var bands = freqBandsJson != null
            ? JsonSerializer.Deserialize<double[]>(freqBandsJson)
            : null;
        return CrackDetector.Detect(amplitude, timeSec, bands);
    }

    public static string SetCrackThreshold(double threshold)
    {
        CrackDetector.SetThreshold(threshold);
        return "{\"success\":true}";
    }

    public static string ResetCrackDetector()
    {
        CrackDetector.Reset();
        return "{\"success\":true}";
    }
}
