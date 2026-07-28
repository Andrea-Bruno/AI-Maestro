# Allarmi

Gli allarmi notificano quando le condizioni di tostatura raggiungono soglie configurabili.

## Set Allarmi

Maestro AI supporta **5 set di allarmi**, ciascuno con condizioni multiple. Ideale per profili diversi (es. "Tostatura Chiara", "Tostatura Scura", "Espresso").

## Tipi di Attivazione

| Attivatore | Si attiva quando... |
|------------|---------------------|
| **TemperatureAbove** | BT supera la soglia |
| **TemperatureBelow** | BT scende sotto la soglia |
| **TimeElapsed** | Tempo trascorso supera la soglia |
| **RateOfRiseAbove** | RoR supera la soglia |
| **RateOfRiseBelow** | RoR scende sotto la soglia |
| **PhaseEvent** | Si verifica un evento di fase specifico |

## Selezione Sorgente

Ogni allarme può monitorare: BT, ET, Delta (BT-ET), RoR, ExtraChannel.

## Guard Time

Il parametro `GuardSec` impedisce riattivazioni entro N secondi.

## Azioni

| Azione | Effetto |
|--------|---------|
| **Beep** | Suono |
| **Log** | Scrive nel log eventi |
| **Notify** | Notifica nell'interfaccia |
| **AutoDrop** | Termina automaticamente la tostatura |
