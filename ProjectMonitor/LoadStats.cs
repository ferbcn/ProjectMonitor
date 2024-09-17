namespace ProjectMonitor;

public class LoadStats
{
    string url { get; set; }
    public float mem { get; set; }
    public float cpu { get; set; }
    
    public override string ToString()
    {
        return $"Mem Load: {mem}, CPU Load: {cpu}";
    }
}