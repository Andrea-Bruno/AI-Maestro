# Certificates, Blockchain & Supply Chain

Blockchain-ready batch certification with QR code authentication, supply chain traceability, and token-based ownership transfer.

## Digital Certificates

Each roast batch can generate a verifiable digital certificate containing the complete batch history:

```json
{
  "batchId": "B001",
  "roastUUID": "a1b2c3d4-...",
  "greenHash": "SHA256(green_analysis.json)",
  "roastParamsHash": "SHA256(roast_parameters.json)",
  "postRoastHash": "SHA256(post_roast_analysis.json)",
  "tasterScore": 85,
  "signature": "ECDSA_P256_SIGN(producer_private_key)",
  "qrToken": "unique_single_reveal_token",
  "qrRevealed": false
}
```

### Hashing

Three independent SHA256 hashes capture the entire production history:
- **Green hash**: origin, variety, density, moisture, green Agtron
- **Roast params hash**: time series, events, machine settings
- **Post-roast hash**: final Agtron, density, cupping scores

Any tampering with the underlying data is immediately detectable.

### ECDSA Signature

The certificate is signed with the producer's **ECDSA P-256 private key**. Anyone with the corresponding public key can verify authenticity. See [19-signatures.md](19-signatures.md) for key generation.

## QR Code Authentication

Each certificate includes a **single-reveal QR code** for anti-counterfeiting:
1. The QR encodes a unique token + batch ID
2. The **first scan** reveals the full certificate and marks the token as used
3. Subsequent scans return "invalid — already used" with the first scan timestamp

### Generating a Certificate with QR

```bash
curl -X POST /api/GenerateCertificate -d '{
  "roastUUID": "a1b2c3d4-...",
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11}",
  "roastParamsJson": "{\"profileName\":\"Ethiopia\"}",
  "postRoastJson": "{\"agtronFinal\":65,\"densityFinal\":400}",
  "tasterScore": 85,
  "privateKeyHex": "..."
}'
```

Response includes the certificate JSON + `qrPngBase64` for rendering the QR code.

## Supply Chain Traceability

Track batch movement through the supply chain with signed events:

| Event | Description |
|-------|-------------|
| `produced` | Batch created at roastery |
| `stored` | Placed in inventory |
| `shipped` | Sent to distributor |
| `received` | Arrived at destination |
| `sold` | Final sale to consumer |

```bash
curl -X POST /api/RecordSupplyChainEvent -d '{
  "batchId": "B001",
  "eventType": "shipped",
  "actor": "Roastery1",
  "location": "Milan, Italy",
  "quantityKg": 100,
  "signature": "actor_signature_hex"
}'

curl -X POST /api/GetSupplyChainTrace -d '{"batchId": "B001"}'
```

## Tokenization (1 Token = 1 kg)

Each batch is tokenised: **1 token = 1 kg** of roasted coffee. Tokens can be transferred between actors:

```bash
# Transfer 100 kg
curl -X POST /api/TransferTokens -d '{
  "from": "Roastery1", "to": "Distributor2",
  "batchId": "B001", "quantityKg": 100,
  "signature": "signature_hex"
}'

# Check circulating tokens
curl -X POST /api/GetTokenBalance -d '{"batchId": "B001"}'
```

### Token Flow

```
Farmer (green beans) → 100 tokens → Roastery (roasted coffee)
  → 80 tokens → Distributor → 75 tokens → Retailer → consumers
```

## Blockchain Timestamping

Each certificate hash can be timestamped on a simulated blockchain (sequential hash chain):

```bash
curl -X POST /api/TimestampCertificate -d '{"batchId":"B001","certificateHash":"<SHA256>"}'
curl -X POST /api/VerifyTimestamp -d '{"batchId":"B001"}'
```

## End-to-End Workflow

```
1. Roast → save profile
2. Measure post-roast: Agtron, density, cupping
3. Generate certificate with ECDSA signature
4. Print QR code on packaging
5. Record supply chain events
6. Transfer tokens at each ownership change
7. Timestamp final certificate on blockchain
```

## API Reference

| Endpoint | Description |
|----------|-------------|
| `GenerateCertificate` | Create certificate + QR code |
| `VerifyQrToken` | Single-reveal QR verification |
| `RecordSupplyChainEvent` | Log supply chain event |
| `GetSupplyChainTrace` | Full batch trace |
| `TimestampCertificate` | Blockchain timestamp |
| `VerifyTimestamp` | Verify chain integrity |
| `TransferTokens` | Transfer token ownership |
| `GetTokenBalance` | Circulating tokens |

## Disabling

```json
"AiFeatures": {
  "CertificateGeneration": false,
  "SupplyChain": false,
  "Blockchain": false
}
```
