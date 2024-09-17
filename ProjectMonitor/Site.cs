namespace ProjectMonitor.Site;

public class Site
{
    public string name { get; set; }
    public string url { get; set; }
    public string loadApi { get; set; }
    public bool up { get; set; }
    public int pingMillis { get; set; }
    public long downloadMillis { get; set; }
    public int downloadSize { get; set; }
    public float memLoad { get; set; }
    public float cpuLoad { get; set; }
    public string colorHex { get; set; }

    public override string ToString()
    {
        return $"Name: {name}, Url: {url,20}";
    }
}