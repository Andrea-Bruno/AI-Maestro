namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Tracks tokenized supply chain events for batch traceability (1 token = 1kg).</summary>
public static class SupplyChainLedger
{
    private static readonly List<SupplyChainEvent> Events = [];

    public static void Record(string batchId, string eventType, string actor,
        string location, double qtyKg, string signature)
    {
        Events.Add(new SupplyChainEvent
        {
            BatchId = batchId, EventType = eventType, Actor = actor,
            Location = location, QuantityKg = qtyKg, Signature = signature,
            Timestamp = DateTime.UtcNow
        });
    }

    public static List<SupplyChainEvent> GetTrace(string batchId) =>
        Events.Where(e => e.BatchId == batchId).OrderBy(e => e.Timestamp).ToList();

    /// <summary>Total tokens in circulation (sum of produced minus sold).</summary>
    public static double CirculatingTokens(string batchId) =>
        Events.Where(e => e.BatchId == batchId).Sum(e =>
            e.EventType == "produced" || e.EventType == "received" ? e.QuantityKg :
            e.EventType == "sold" || e.EventType == "shipped" ? -e.QuantityKg : 0);
}
