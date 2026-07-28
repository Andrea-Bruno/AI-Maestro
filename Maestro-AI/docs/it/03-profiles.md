# Profili

I profili salvano dati completi di tostatura: serie temporali, eventi, metadati e metriche.

## Elenco Profili

La sezione **List** mostra tutti i profili salvati. Per ogni profilo puoi:

- **Load**: Aprire nel tab Analisi
- **Delete**: Eliminare permanentemente

I profili vengono salvati automaticamente con il formato `Tostatura AAAA-MM-GG HH:mm`.

## Proprietà Profilo

Modifica i metadati dettagliati per ogni profilo tramite il pulsante **Salva**: peso in/out, umidità, colore Agtron, temperatura ambiente, temperatura caffè verde, dimensione setaccio, velocità tamburo, altitudine.

## Progettazione Curva

Crea profili target specificando eventi cardine: Charge → Dry End → FC Start → Drop. Il progettista crea una curva BT uniforme attraverso questi punti di riferimento. Clicca **Crea** per salvare.

## Trasformatore

| Operazione | Descrizione |
|------------|-------------|
| **Time Scale** | Allunga/comprime l'asse temporale di un fattore |
| **Temp Offset** | Sposta tutte le temperature BT/ET di un offset |
| **Invert** | Inverti la curva BT attorno al punto medio |
| **C° → F°** | Converti temperature da Celsius a Fahrenheit |

Clicca **Applica** per eseguire la trasformazione.

## Importa / Esporta

Formati supportati: Maestro JSON (.maestro) e .alog (.alog).

Clicca **Importa** per caricare un file, o seleziona un profilo e clicca **Esporta** per scaricarlo.

## Firma Profili

I profili possono essere firmati crittograficamente con chiave ECDSA P-256:
1. Clicca **Genera Chiavi** per creare una nuova coppia di chiavi
2. Inserisci la **Chiave privata** e clicca **Firma**
3. Usa **Verifica** con la **Chiave pubblica** per controllare l'integrità
