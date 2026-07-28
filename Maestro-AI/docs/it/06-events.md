# Eventi

Gli eventi marcano momenti specifici durante la tostatura e attivano azioni programmabili.

## Tipi di Evento

| Tipo | Descrizione |
|------|-------------|
| **Button** | Azione con un clic |
| **Slider** | Input a intervallo con min/max/step |
| **Quantifier** | Input numerico |

## Definire un'Azione

Ogni evento ha un comando che definisce cosa succede:
```
SetGas(50)    → Imposta gas al 50%
Log(messaggio) → Scrive nel log
```

## Eventi Utente

Eventi personalizzati aggiunti durante la tostatura con etichetta e valore opzionale.
