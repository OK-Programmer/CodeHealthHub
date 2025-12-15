using System;
using System.Text.Json.Serialization;

namespace CodeHealthHub.Models;

public class Measure
{
    public required int Id { get; set; }
    public required string Metric { get; set; }
    public required string Value { get; set; }

    public required int ProjectScanId { get; set; } // Foreign key to ProjectScan
    [JsonIgnore]
    public ProjectScan? ProjectScan { get; set; } // Navigation property
}
