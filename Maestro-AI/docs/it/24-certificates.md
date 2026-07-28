# Certificati, Blockchain e Filiera

Certificazione batch pronta per blockchain con autenticazione QR code, tracciabilità della filiera e trasferimento di proprietà basato su token.

## Certificati Digitali

Ogni lotto tostato può generare un certificato digitale verificabile contenente la cronologia completa del lotto. Il certificato è strutturato come segue:

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

Tre hash SHA256 indipendenti catturano l'intera cronologia di produzione:
- **Hash caffè verde**: origine, varietà, densità, umidità, Agtron verde
- **Hash parametri tostatura**: serie temporali, eventi, impostazioni macchina
- **Hash post-tostatura**: Agtron finale, densità, punteggi cupping

Qualsiasi manomissione dei dati sottostanti è immediatamente rilevabile.

### Firma ECDSA

Il certificato è firmato con la **chiave privata ECDSA P-256** del produttore usando l'API `SignProfile`. Chiunque abbia la chiave pubblica corrispondente può verificare l'autenticità del certificato. Vedere `docs/it/19-signatures.md` per le istruzioni di generazione delle chiavi.

## Autenticazione QR Code

Ogni certificato include un **codice QR a singola rivelazione** per l'anticonterfezione:

1. Il QR codifica un token univoco + ID lotto
2. La **prima scansione** rivela il certificato completo e segna il token come `qrRevealed: true`
3. Le scansioni successive restituiscono `"invalid — already used"` con il timestamp della prima scansione

Questo fornisce ai consumatori finali autenticità del prodotto verificabile. Il QR è generato come PNG usando QRCoder e restituito come data URL in base64.

### Generare un Certificato con QR

```bash
curl -X POST /api/GenerateCertificate -d '{
  "roastUUID": "a1b2c3d4-...",
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "roastParamsJson": "{\"profileName\":\"Ethiopia\",\"metrics\":{...}}",
  "postRoastJson": "{\"agtronFinal\":65,\"densityFinal\":400}",
  "tasterScore": 85,
  "privateKeyHex": "..."   ← dalla propria coppia di chiavi ECDSA
}'
```

La risposta include il JSON del certificato + `qrPngBase64` per il rendering del codice QR.

### Verificare un Token QR

```bash
curl -X POST /api/VerifyQrToken -d '{"token": "..."}'
```

Restituisce `{ "valid": true, "certificate": {...}, "firstScan": "2026-07-28T12:00:00Z" }` o `{ "valid": false, "reason": "already used", "firstScan": "..." }`.

## Tracciabilità Filiera

Tracciare il movimento dei lotti attraverso la filiera con eventi firmati:

| Evento | Descrizione |
|-------|-------------|
| `produced` | Lotto creato in torrefazione |
| `stored` | Inserito in magazzino |
| `shipped` | Spedito al distributore |
| `received` | Arrivato a destinazione |
| `sold` | Vendita finale al consumatore |

Ogni evento è firmato dall'attore e timestampato. La traccia completa è disponibile tramite:

```bash
curl -X POST /api/GetSupplyChainTrace -d '{"batchId": "B001"}'
```

### Registrare un Evento

```bash
curl -X POST /api/RecordSupplyChainEvent -d '{
  "batchId": "B001",
  "eventType": "shipped",
  "actor": "Torrefazione1",
  "location": "Milano, Italia",
  "quantityKg": 100,
  "signature": "firma_attore_hex"
}'
```

## Tokenizzazione (1 Token = 1 kg)

Ogni lotto è tokenizzato con un rapporto fisso di **1 token = 1 chilogrammo di caffè tostato**. I token rappresentano la proprietà digitale e possono essere trasferiti tra gli attori della filiera:

```bash
# Trasferire 100 kg da Torrefazione1 a Distributore2
curl -X POST /api/TransferTokens -d '{
  "from": "Torrefazione1",
  "to": "Distributore2",
  "batchId": "B001",
  "quantityKg": 100,
  "signature": "firma_hex"
}'

# Controllare token circolanti
curl -X POST /api/GetTokenBalance -d '{"batchId": "B001"}'
```

### Flusso Token

```
Agricoltore (caffè verde)
  ↓ 100 token trasferiti
Torrefazione (caffè tostato)
  ↓ 80 token trasferiti
Distributore (confezionamento sfuso)
  ↓ 75 token trasferiti
Venditore al dettaglio (pacchetti unitari)
  ↓ 70 token = 70 × 1 kg venduti ai consumatori
```

Ad ogni passo, il contratto intelligente verifica che il mittente abbia token sufficienti prima di permettere il trasferimento.

## Timbratura Blockchain

Ogni hash di certificato può essere timestampato su una blockchain simulata (catena hash sequenziale):

```bash
curl -X POST /api/TimestampCertificate -d '{
  "batchId": "B001",
  "certificateHash": "<hash_SHA256>"
}'
```

Restituisce: `{ "blockIndex": 42, "blockHash": "0000a1b2...", "previousHash": "0000c3d4..." }`

Verifica:
```bash
curl -X POST /api/VerifyTimestamp -d '{"batchId": "B001"}'
```

La blockchain è una catena hash locale (non un registro distribuito) — adatta per dimostrazioni e audit interni. Per la produzione, la stessa struttura dati può essere inviata a blockchain pubbliche (Ethereum, Hyperledger) tramite l'API `SendToCloud`.

## Flusso di Lavoro Completo

```
1. Tostatura → salvare profilo
2. Misurare post-tostatura: Agtron, densità, cupping
3. Generare certificato con firma ECDSA
4. Stampare codice QR sulla confezione
5. Registrare eventi filiera (spedito → ricevuto → venduto)
6. Trasferire token ad ogni cambio di proprietà
7. Timbrare certificato finale su blockchain
8. Consumatore scansiona QR → verifica autenticità + traccia completa
```

## Riferimento API

| Endpoint | Descrizione |
|----------|-------------|
| `GenerateCertificate` | Creare certificato + codice QR |
| `VerifyQrToken` | Verifica QR a singola rivelazione |
| `GetCertificate` | Recuperare certificato per ID lotto |
| `RecordSupplyChainEvent` | Registrare evento filiera |
| `GetSupplyChainTrace` | Traccia completa lotto |
| `TimestampCertificate` | Timbratura blockchain |
| `VerifyTimestamp` | Verificare integrità catena |
| `TransferTokens` | Trasferire proprietà token |
| `GetTokenBalance` | Token circolanti |

## Disabilitazione Funzionalità

```json
"AiFeatures": {
  "CertificateGeneration": false,   // nasconde certificato + QR
  "SupplyChain": false,             // nasconde tracciabilità
  "Blockchain": false               // nasconde timbratura
}
```
