using System;
using System.Text.Json.Serialization;

namespace CodeHealthHub.Models;

public class Measure
{
    public int Id { get; set; }
    public string? Metric { get; set; }
    public string? Value { get; set; }

    public int ProjectScanId { get; set; } // Foreign key to ProjectScan
    [JsonIgnore]
    public ProjectScan? ProjectScan { get; set; } // Navigation property
}
