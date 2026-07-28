# Guida Rapida

Benvenuto in Maestro AI — piattaforma moderna per il controllo della tostatura del caffè.

## Primo Avvio

1. **Avvia il server**:
   ```bash
   cd Maestro-AI
   dotnet run --launch-profile http
   ```
   Il server parte su `http://localhost:5252`.

2. **Apri il client**: Apri `Maestro-AI-Client/index.html` in qualsiasi browser moderno.

3. **Verifica connessione**: Il LED nella toolbar diventa verde quando connesso. Vai al tab **Dashboard** per vedere lo stato del server.

## Simulazione Rapida

Maestro AI funziona in **modalità simulata** di default (nessun hardware richiesto):

1. Vai al tab **Roast**
2. Clicca **Start Roast**
3. Clicca **Add Sample** ripetutamente per simulare letture temperatura
4. Osserva le curve BT/ET aggiornarsi in tempo reale sul grafico ECharts
5. Seleziona **FC Start** dal menu eventi quando vuoi simulare il primo crack
6. Clicca **Drop & Stop** per terminare

Il profilo viene salvato automaticamente e appare nel tab **Profiles**.

## Prossimi Passi

- Leggi la guida **Roast Monitor** per capire l'interfaccia
- Configura **Allarmi** per ricevere notifiche a temperature critiche
- Collega hardware reale tramite `appsettings.json`
