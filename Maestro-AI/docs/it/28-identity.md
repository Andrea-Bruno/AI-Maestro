# Identità Macchina e Sicurezza

Identità digitale per macchine da torrefazione con autenticazione crittografica ECDSA, messaggistica cloud cifrata e timbratura blockchain dei certificati.

## Identità Digitale Macchina

Ogni istanza di Maestro AI genera una coppia di chiavi **ECDSA P-256** univoca alla prima inizializzazione. L'hash SHA256 della chiave pubblica diventa l'**identificatore esadecimale a 16 caratteri** della macchina.

### Inizializzazione

```bash
curl -X POST /api/InitCloud -d '{"cloudEndpoint":"https://api.maestro-ai.cloud/v1"}'
```

Risposta:
```json
{
  "machineId": "fb2f505cd427c6bc",
  "publicKey": "AsXLQYoRQ1...",
  "endpoint": "https://api.maestro-ai.cloud/v1"
}
```

La coppia di chiavi è persistita dalla libreria EncryptedMessaging e sopravvive ai riavvii.

### Gestione Identità

```bash
# Visualizzare identità corrente
curl -X POST /api/GetMachineIdentity

# Esportare chiave privata (per backup/migrazione)
curl -X POST /api/ExportMachineKey
```

### Perché ECDSA P-256?

- Standard **FIPS 186-4**
- Chiave **256-bit** con livello di sicurezza 128-bit
- **Firme compatte** (64 byte) — adatte per QR code
- Supporto hardware su moduli **TPM 2.0** quando disponibile

## Comunicazione Cloud Cifrata

Il protocollo `CloudMessaging` fornisce comunicazione crittografata end-to-end tra la torrefazione e l'infrastruttura cloud privata.

### Protocollo

Ogni messaggio è avvolto in un involucro JSON firmato:

```json
{
  "command": "syncProfile",
  "payload": "{...}",
  "machineId": "fb2f505cd427c6bc",
  "timestamp": "2026-07-28T10:00:00Z",
  "signature": "ECDSA_signature_hex"
}
```

### Invio Dati

```bash
curl -X POST /api/SendToCloud -d '{
  "command": "syncProfile",
  "payload": "{\"profileName\":\"Ethiopia Yirgacheffe\",\"metrics\":{...}}"
}'
```

### Caratteristiche Sicurezza

- **Firme ECDSA** → autenticazione messaggio
- **Timestamp** → protezione attacchi replay
- **ID Macchina** → verifica identità mittente
- **Involucro JSON** → leggibile, facilmente debuggabile

### Infrastruttura Cloud Privata

L'infrastruttura cloud è progettata con:
- **Architettura zero-trust** — ogni richiesta è autenticata
- **Crittografia end-to-end** — nessun dato in chiaro
- **Archiviazione a freddo** — archiviazione dati a lungo termine
- **Protocolli proprietari** — nessuna porta standard esposta

## Timbratura Blockchain

Ogni hash di certificato può essere timestampato su una blockchain simulata (catena hash SHA256 sequenziale). Vedere `docs/it/24-certificates.md` per dettagli completi.

```bash
curl -X POST /api/TimestampCertificate -d '{
  "batchId": "B001",
  "certificateHash": "<hash_SHA256>"
}'

curl -X POST /api/VerifyTimestamp -d '{"batchId": "B001"}'
```

## Trasferimento Token (Filiera)

1 token = 1 kg di caffè. Trasferire proprietà tra attori della filiera con firme crittografiche:

```bash
curl -X POST /api/TransferTokens -d '{
  "from": "Torrefazione1",
  "to": "Distributore2",
  "batchId": "B001",
  "quantityKg": 100,
  "signature": "firma_hex"
}'

curl -X POST /api/GetTokenBalance -d '{"batchId": "B001"}'
```

## API

| Endpoint | Descrizione |
|----------|-------------|
| `InitCloud` | Inizializzare identità macchina + endpoint cloud |
| `GetMachineIdentity` | Restituire ID macchina + chiave pubblica |
| `SendToCloud` | Inviare messaggio firmato al cloud |
| `ExportMachineKey` | Esportare chiave privata per backup |
| `TimestampCertificate` | Aggiungere blocco alla catena hash |
| `VerifyTimestamp` | Verificare integrità catena |
| `TransferTokens` | Trasferire proprietà token |
| `GetTokenBalance` | Token circolanti per un lotto |

## Disabilitazione

```json
"AiFeatures": {
  "MachineIdentity": false,
  "CloudMessaging": false,
  "Blockchain": false
}
```
