# Simulatore

Riproduce un profilo salvato come se provenisse da hardware reale.

## Scopo

Test, dimostrazioni, formazione, analisi dati punto per punto.

## Utilizzo

```bash
curl -X POST /api/StartSimulation -d '{"profileName":"Mio Profilo"}'
curl -X POST /api/NextSimulation -d '{"simId":"..."}'
```
