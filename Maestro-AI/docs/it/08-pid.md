# Controllore PID

Controllo software della temperatura per tostatrici senza PID integrato.

## Parametri

| Parametro | Descrizione | Range Tipico |
|-----------|-------------|--------------|
| **Kp** | Guadagno proporzionale | 10-50 |
| **Ki** | Guadagno integrale | 0.01-0.1 |
| **Kd** | Guadagno derivativo | 1-10 |

## Consigli Taratura

1. Ki = Kd = 0, aumenta Kp fino a oscillazione
2. Riduci Kp del 30%, aumenta Ki fino a eliminare offset
3. Aggiungi Kd per ridurre overshoot
