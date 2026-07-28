/// <summary>Between-Batch Profiling — tracks roaster recovery for consecutive batches.</summary>
public class BbpCache
{
    public double DropBt { get; set; }
    public double DropEt { get; set; }
    public double ChargeBt { get; set; }
    public double ChargeEt { get; set; }
    public double PreheatTimeSec { get; set; }
    public double TempRecoveryPercent { get; set; } // how close to previous drop temp
    public int BatchCount { get; set; }
}
