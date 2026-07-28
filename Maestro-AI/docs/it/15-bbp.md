# BBP — Between Batch Profiling

Monitoraggio del recupero temperatura tra lotti consecutivi per produzione ottimale.

## Metriche

| Metrica | Descrizione |
|---------|-------------|
| **Drop BT Precedente** | Temperatura chicco fine lotto precedente |
| **Charge BT Corrente** | Temperatura all'avvio nuovo lotto |
| **Tempo Preriscaldo** | Secondi tra Drop e Charge |
| **Recupero %** | `Charge BT / Drop BT × 100` |

## Utilizzo

Obiettivo: Recupero > 80% per produzione costante.
