using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CodeHealthHub.Models;

public class SonarQubeProject()
{
    public int Id { get; set; }

    public required string Key { get; set; }

    public required string Name { get; set; }

    public required string LastAnalysisDate { get; set; }

    [ForeignKey("SonarQubeProjectId")]
    public List<ProjectScan>? ProjectScans { get; set; }

    public double Weight { get; set; }

    public int NumOfDevelopers { get; set; }

    public double DeveloperCostPerHour { get; set; }

    public int SonarQubeInstanceId { get; set; } // Foreign key

    [JsonIgnore]
    public SonarQubeInstance SonarQubeInstance { get; set; } = null!; // Navigation property
}
