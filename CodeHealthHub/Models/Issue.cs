using System;

namespace CodeHealthHub.Models;

public class Issue
{
    public string key { get; set; }
    public string severity { get; set; }
    public string project { get; set; }
    public string effort { get; set; }
    public string debt { get; set; }
    public string type { get; set; }
    public string status { get; set; }
}
