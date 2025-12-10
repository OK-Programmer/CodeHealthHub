using System;

namespace CodeHealthHub.Models;

public class Issue
{
    public required string Key { get; set; }
    public required string Severity { get; set; }
    public required string Project { get; set; }
    public required string Effort { get; set; }
    public required string Debt { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
}
