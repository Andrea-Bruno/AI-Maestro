# Machine Identity & Security

Digital identity for roasting machines with ECDSA cryptographic authentication, encrypted cloud messaging, and blockchain certificate timestamping.

## Machine Digital Identity

Every Maestro AI instance generates a unique **ECDSA P-256 key pair** on first initialisation. The SHA256 hash of the public key becomes the machine's **16-character hexadecimal identifier**.

### Initialisation

```bash
# First call generates a persistent key pair
curl -X POST /api/InitCloud -d '{"cloudEndpoint":"https://api.maestro-ai.cloud/v1"}'
```

Response:
```json
{
  "machineId": "fb2f505cd427c6bc",
  "publicKey": "AsXLQYoRQ1...",
  "endpoint": "https://api.maestro-ai.cloud/v1"
}
```

The key pair is persisted by the EncryptedMessaging library and survives restarts.

### Identity Management

```bash
# View current identity
curl -X POST /api/GetMachineIdentity

# Export private key (for backup / migration)
curl -X POST /api/ExportMachineKey
```

### Why ECDSA P-256?

- **FIPS 186-4** standard
- **256-bit key** provides 128-bit security level
- **Compact signatures** (64 bytes) — suitable for QR codes
- **Hardware-backed** on TPM 2.0 modules when available

## Encrypted Cloud Communication

The `CloudMessaging` protocol provides end-to-end encrypted communication between the roaster and the private cloud infrastructure.

### Protocol

Each message is wrapped in a signed JSON envelope:

```json
{
  "command": "syncProfile",
  "payload": "{...}",
  "machineId": "fb2f505cd427c6bc",
  "timestamp": "2026-07-28T10:00:00Z",
  "signature": "ECDSA_signature_hex"
}
```

### Sending Data

```bash
curl -X POST /api/SendToCloud -d '{
  "command": "syncProfile",
  "payload": "{\"profileName\":\"Ethiopia Yirgacheffe\",\"metrics\":{...}}"
}'
```

### Security Features

- **ECDSA signatures** → message authentication
- **Timestamps** → replay attack protection
- **Machine ID** → sender identity verification
- **JSON envelope** → human-readable, easily debuggable

### Private Cloud Infrastructure

The cloud infrastructure is designed with:
- **Zero-trust architecture** — every request is authenticated
- **End-to-end encryption** — no plaintext data
- **Cold storage** — long-term data archival
- **Proprietary protocols** — no standard ports exposed

## Blockchain Timestamping

Each batch certificate can be timestamped on a simulated blockchain (sequential SHA256 hash chain). See `docs/en/24-certificates.md` for full details.

```bash
curl -X POST /api/TimestampCertificate -d '{
  "batchId": "B001",
  "certificateHash": "<SHA256_hash>"
}'

curl -X POST /api/VerifyTimestamp -d '{"batchId": "B001"}'
```

## Token Transfer (Supply Chain)

1 token = 1 kg of coffee. Transfer ownership between supply chain actors with cryptographic signatures:

```bash
curl -X POST /api/TransferTokens -d '{
  "from": "Roastery1",
  "to": "Distributor2",
  "batchId": "B001",
  "quantityKg": 100,
  "signature": "signature_hex"
}'

curl -X POST /api/GetTokenBalance -d '{"batchId": "B001"}'
```

## API

| Endpoint | Description |
|----------|-------------|
| `InitCloud` | Initialise machine identity + cloud endpoint |
| `GetMachineIdentity` | Return machine ID + public key |
| `SendToCloud` | Send signed message to cloud |
| `ExportMachineKey` | Export private key for backup |
| `TimestampCertificate` | Add block to hash chain |
| `VerifyTimestamp` | Verify chain integrity |
| `TransferTokens` | Transfer token ownership |
| `GetTokenBalance` | Circulating tokens for a batch |

## Disabling

```json
"AiFeatures": {
  "MachineIdentity": false,
  "CloudMessaging": false,
  "Blockchain": false
}
```
