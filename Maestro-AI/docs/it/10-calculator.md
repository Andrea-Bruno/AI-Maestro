# Calcolatore

Calcolatori integrati per conversioni comuni in tostatura.

## Conversione Temperatura

°C → °F: `°F = °C × 9/5 + 32`
°F → °C: `°C = (°F - 32) × 5/9`

## Conversione Peso

g → kg (÷1000), kg → g (×1000), lb → g (×453.592)

## Resa Estrazione

`Resa % = (Peso Bevanda × TDS%) / Dose Caffè`

Range tipico: 18-22%, rapporto 1:15-1:18.

## Calcolo Densità

`Densità (g/L) = Peso (g) / Volume (mL) × 1000`

## Spettrometro NIR

Registra e recupera campioni spettrali NIR.

| Azione | API |
|--------|-----|
| Registra campione | `RecordSpectra` → `POST /api` |
| Ottieni recenti | `GetSpectra` → `POST /api` |

**Input**: array lunghezze d'onda, array intensità, ID sessione.

## Rilevamento Crack

Rileva eventi di primo/secondo crack da segnali audio/vibrazioni.

| Azione | API |
|--------|-----|
| Rileva crack | `DetectCrack` → `POST /api` |
| Imposta soglia | `SetCrackThreshold` → `POST /api` |
| Resetta contatore | `ResetCrackDetector` → `POST /api` |

**Parametri**: ampiezza, tempo (s), valore soglia.

## Riscaldamento Ibrido

Configura e monitora sistemi di riscaldamento ibrido (tradizionale + microonde + infrarossi).

| Azione | API |
|--------|-----|
| Applica mix | `SetHybridHeating` → `POST /api` |
| Stato riscaldamento | `GetHeatingStatus` → `POST /api` |

**Parametri**: % Tradizionale, % Microonde, % Infrarossi, Frequenza IR (Hz).
