using System;

namespace CodeHealthHub.Models;

public class Issue
{
    public string Key { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Project { get; set; } = "";
    public string Effort { get; set; } = "";
    public string Debt { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
}
