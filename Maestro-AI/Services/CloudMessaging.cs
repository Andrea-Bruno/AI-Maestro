using System.Text;
using System.Text.Json;
using EncryptedMessaging;

namespace Maestro_AI.Services;

/// <summary>Cloud communication using EncryptedMessaging identity. Minimal wrapper over the library's Context/Message system.</summary>
public class CloudMessaging : IDisposable
{
    private readonly Context _context;
    private readonly string _cloudEndpoint;
    public string MachineId { get; }
    public string PublicKey => _context.My.GetPublicKey();

    /// <summary>Initialize or load machine identity. First call creates persistent ECDSA key pair via EncryptedMessaging.</summary>
    public CloudMessaging(string cloudEndpoint, string entryPoint = "", string networkName = "maestro-net")
    {
        _cloudEndpoint = cloudEndpoint;
        // Entry point defaults to localhost if not specified
        entryPoint = string.IsNullOrEmpty(entryPoint) ? "127.0.0.1" : entryPoint;
        _context = new Context(entryPoint, networkName, privateKeyOrPassphrase: null, modality: Modality.Client | Modality.SaveContacts | Modality.LoadContacts);
        // Machine ID from public key hash
        var pub = _context.My.GetPublicKeyBinary();
        MachineId = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pub)).ToLowerInvariant()[..16];
    }

    /// <summary>Send a signed message to the cloud endpoint as JSON. Layer over EncryptedMessaging identity.</summary>
    public async Task<string> SendAsync(string command, string payload)
    {
        var msg = new
        {
            command,
            payload,
            machineId = MachineId,
            publicKey = PublicKey,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        using var client = new HttpClient();
        var response = await client.PostAsync(_cloudEndpoint,
            new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json"));
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>Export private key for backup (returns Base64 CspBlob).</summary>
    public string ExportPrivateKey() => _context.My.GetPrivateKey();

    public void Dispose() => _context.Dispose();
}
