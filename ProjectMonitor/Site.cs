using System.Drawing;

namespace ProjectMonitor.Site;

public class Site
{
    public string name { get; set; }
    public string url { get; set; }
    public bool up { get; set; }
    public int ping_time { get; set; }
    public long downloadMillis { get; set; }
    public int downloadSize { get; set; }
    public Color color { get; set; }

    public override string ToString()
    {
        return $"Name: {name}, Url: {url,20}";
    }
}