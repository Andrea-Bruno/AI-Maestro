# Range di Fase

Configura le soglie di temperatura che definiscono ogni fase.

## Valori Default

| Transizione | Temperatura Default |
|-------------|-------------------|
| Dry End | 160°C |
| First Crack | 190°C |
| Second Crack | 215°C |

## Rilevamento Automatico

```bash
curl -X POST /api/DetectPhases -d '{
  "timeJson": "[0,60,120,...]",
  "btJson": "[25,80,130,...]"
}'
```

L'algoritmo trova Turning Point (minimo BT), calcola RoR, rileva FC quando RoR scende sotto 3°C/min.
