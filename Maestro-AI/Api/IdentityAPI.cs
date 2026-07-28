using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Services;

public static class IdentityAPI
{
    private static CloudMessaging? _cloud;

    public static string InitCloud(string cloudEndpoint, string? entryPoint = null)
    {
        Log.LogStep($"InitCloud: endpoint={cloudEndpoint}");
        _cloud = new CloudMessaging(cloudEndpoint, entryPoint ?? "");
        // On creation, EncryptedMessaging generates persistent ECDSA key pair
        return JsonSerializer.Serialize(new { machineId = _cloud.MachineId, publicKey = _cloud.PublicKey });
    }

    public static string GetMachineIdentity()
    {
        Log.LogStep("GetMachineIdentity");
        if (_cloud == null) return "{\"error\":\"Cloud not initialized\"}";
        return JsonSerializer.Serialize(new { machineId = _cloud.MachineId, publicKey = _cloud.PublicKey });
    }

    public static async Task<string> SendToCloud(string command, string payload)
    {
        Log.LogStep($"SendToCloud: command={command}");
        if (_cloud == null) return "{\"error\":\"Cloud not initialized\"}";
        return await _cloud.SendAsync(command, payload);
    }

    public static string ExportMachineKey()
    {
        Log.LogStep("ExportMachineKey");
        if (_cloud == null) return "{\"error\":\"Cloud not initialized\"}";
        return JsonSerializer.Serialize(new { privateKeyBase64 = _cloud.ExportPrivateKey(), warning = "Keep secure. Grants machine identity." });
    }

    // Predictive trainer
    public static string RecordTrainingData(string greenJson, string resultJson)
    {
        Log.LogStep("RecordTrainingData");
        PredictiveTrainer.Record(greenJson, resultJson);
        return "{\"success\":true}";
    }

    public static string TrainModel()
    {
        Log.LogStep("TrainModel");
        return PredictiveTrainer.Train();
    }
    public static string GetTrainingStatus()
    {
        Log.LogStep("GetTrainingStatus");
        return PredictiveTrainer.GetStatus();
    }

    // Blockchain (simulated hash chain)
    private static readonly List<BlockRecord> Chain = [];

    public static string TimestampCertificate(string batchId, string certificateHash)
    {
        Log.LogStep($"TimestampCertificate: batchId={batchId}");
        var prev = Chain.Count > 0 ? Chain[^1].Hash : "GENESIS";
        var hash = ComputeHash($"{batchId}|{certificateHash}|{prev}|{DateTime.UtcNow:O}");
        Chain.Add(new BlockRecord { BatchId = batchId, Hash = hash, PreviousHash = prev, Timestamp = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { blockIndex = Chain.Count - 1, hash, previousHash = prev, timestamped = true });
    }

    public static string VerifyTimestamp(string batchId)
    {
        Log.LogStep($"VerifyTimestamp: batchId={batchId}");
        var r = Chain.FirstOrDefault(x => x.BatchId == batchId);
        if (r == null) return "{\"verified\":false}";
        var expected = ComputeHash($"{r.BatchId}|{r.Hash}|{r.PreviousHash}|{r.Timestamp:O}");
        return JsonSerializer.Serialize(new { verified = r.Hash == expected, r.BatchId, r.Timestamp });
    }

    public static string TransferTokens(string from, string to, string batchId, double qty, string signature)
    {
        Log.LogStep($"TransferTokens: from={from}, to={to}, batchId={batchId}, qty={qty}");
        return JsonSerializer.Serialize(new { transferred = true, from, to, batchId, qty });
    }

    public static string GetTokenBalance(string batchId)
    {
        Log.LogStep($"GetTokenBalance: batchId={batchId}");
        return JsonSerializer.Serialize(new { batchId, circulatingKg = 100.0 }); // simplified
    }

    private static string ComputeHash(string data) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(data)));

    private record BlockRecord
    {
        public string BatchId { get; init; } = "";
        public string Hash { get; init; } = "";
        public string PreviousHash { get; init; } = "";
        public DateTime Timestamp { get; init; }
    }
}
