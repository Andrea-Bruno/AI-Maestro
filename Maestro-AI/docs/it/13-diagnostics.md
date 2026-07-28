# Diagnostica

Stato del sistema e logging.

## Stato Sistema

- **Server**: LED verde quando raggiungibile
- **Dispositivo**: LED verde quando connesso
- **Uptime**: Tempo di attività
- **Sessioni Attive**: Tostature in corso
- **Profili Salvati**: Totale su disco

## Log Eventi

Cronologia di eventi dispositivo, allarmi, eventi utente ed errori.

Scrivere messaggi personalizzati:
```bash
curl -X POST /api/LogMessage -d '{"level":"INFO","message":"Manutenzione OK"}'
```

## Test Dispositivo

Invia un comando di test all'hardware configurato e riporta esito, latenza ed errori.
