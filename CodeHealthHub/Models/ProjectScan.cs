using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CodeHealthHub.Models;

public class ProjectScan
{
    public int Id { get; set; }

    public required int SonarQubeProjectId { get; set; } // Foreign key to SonarQubeProject

    [JsonIgnore]
    public SonarQubeProject? SonarQubeProject { get; set; } // Navigation property

    public required string AnalysisDate { get; set; } = "";

    public required string CreatedAt { get; set; } = "";
    
    [ForeignKey("ProjectScanId")]
    public List<Measure>? Measures { get; set; }
}
