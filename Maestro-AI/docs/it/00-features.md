# Panoramica Funzionalità AI

Maestro AI include una suite di funzionalità AI per l'analisi del caffè, l'ottimizzazione della tostatura, la certificazione e la tracciabilità della filiera. Ogni funzionalità può essere abilitata o disabilitata indipendentemente tramite `appsettings.json`.

## Riferimento Funzionalità

| Funzionalità | Flag Config | Default | Documentazione |
|-------------|-------------|---------|----------------|
| Generazione Profilo AI | `ProfileGeneration` | `true` | [23-ai-profile.md](23-ai-profile.md) |
| Analisi Energetica | `EnergyAnalysis` | `true` | [26-energy.md](26-energy.md) |
| Generazione Certificati | `CertificateGeneration` | `true` | [24-certificates.md](24-certificates.md) |
| Filiera | `SupplyChain` | `true` | [24-certificates.md](24-certificates.md) |
| Cupping | `Cupping` | `true` | [04-analysis.md](04-analysis.md) |
| Rilevamento Cracking | `CrackDetection` | `true` | [25-predictive.md](25-predictive.md) |
| Trainer Predittivo | `PredictiveTrainer` | `true` | [29-cloud.md](29-cloud.md) |
| Messaggistica Cloud | `CloudMessaging` | `true` | [28-identity.md](28-identity.md) |
| Identità Macchina | `MachineIdentity` | `true` | [28-identity.md](28-identity.md) |
| Blockchain | `Blockchain` | `true` | [24-certificates.md](24-certificates.md) |
| Spettroscopia | `Spectroscopy` | `true` | [27-sensors.md](27-sensors.md) |
| Riscaldamento Ibrido | `HybridHeating` | `true` | [27-sensors.md](27-sensors.md) |
| Sensori Extra | `ExtraSensors` | `true` | [27-sensors.md](27-sensors.md) |
| Firma Profili | `ProfileSigning` | `true` | [19-signatures.md](19-signatures.md) |
| Import / Export | `ImportExport` | `true` | [11-import-export.md](11-import-export.md) |
| External Instruments | `ExternalInstruments` | `true` | [33-instruments.md](33-instruments.md) |

> **Supporto GPIO:** Maestro AI supporta la [52Pi EP-0129 GPIO 40-PIN Hat](https://wiki.52pi.com/index.php?title=EP-0129) per il controllo diretto dei pin via Raspberry Pi. Vedi [09-hardware.md](09-hardware.md) per schemi di cablaggio, opzioni sensori e API (`SetHeaterPwm`, `SetFanSpeed`, `SetGpioPin`).

## Configurazione

Aggiungi la seguente sezione al tuo `appsettings.json`:

```json
"AiFeatures": {
  "Enabled": true,
  "ProfileGeneration": true,
  "EnergyAnalysis": true,
  "CertificateGeneration": true,
  "SupplyChain": true,
  "Cupping": true,
  "CrackDetection": true,
  "PredictiveTrainer": true,
  "CloudMessaging": true,
  "MachineIdentity": true,
  "Blockchain": true,
  "Spectroscopy": true,
  "HybridHeating": true,
  "ExtraSensors": true,
  "ProfileSigning": true,
  "ImportExport": true
}
```

Impostando `"Enabled": false` si disabilitano TUTTE le funzionalità AI contemporaneamente. I singoli flag sovrascrivono l'interruttore principale quando impostati.

### Comportamento Quando Disabilitato

Quando una funzionalità è disabilitata:
- Il pannello UI corrispondente è **nascosto** dall'interfaccia client
- Le API lato server restituiscono `{ "error": "feature disabled" }`
- La funzionalità non consuma risorse né esegue operazioni in background

## Avvio Veloce

1. **Generazione Profilo**: Misura densità, umidità e Agtron del caffè verde → inserisci nel pannello AI Profile del tab Roast → clicca **Generate**
2. **Cupping**: Dopo una tostatura, valuta flavour, acidità, corpo, ecc. nel tab Analysis → salva i punteggi
3. **Certificati**: Genera un certificato batch con QR code → stampa sulla confezione
4. **Filiera**: Registra eventi (spedito, ricevuto, venduto) mentre il caffè si muove lungo la catena
5. **Blockchain**: Timbra i certificati sulla catena di hash per verifica a prova di manomissione

Vedi la documentazione individuale di ogni funzionalità per istruzioni dettagliate.

## Informazioni sul Modello AI

Il sistema AI si basa su una rete neurale addestrata su dati provenienti da macchine da tostatura distribuite. Riceve:
- Dati del caffè verde (densità, umidità, colore, origine)
- Parametri di tostatura (serie temporali, eventi, impostazioni macchina)
- Analisi post-tostatura (Agtron finale, densità, perdita peso)
- Valutazioni sensoriali (punteggi cupping Q-Grader)

Il modello apprende correlazioni non ovvie tra input e qualità, e migliora continuamente man mano che vengono raccolti nuovi dati. Tutti i dati sono memorizzati in un cloud privato con crittografia end-to-end.

**Concetto chiave**: L'AI non tosta il caffè — impara la relazione tra ciò che entra e ciò che esce, poi raccomanda il percorso ottimale.
