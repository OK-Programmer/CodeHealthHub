using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CodeHealthHub.Models;

public class ProjectScan
{
    public int Id { get; set; }

    public int SonarQubeProjectId { get; set; } // Foreign key to SonarQubeProject
    
    public DateTime AnalysisDate { get; set; }

    [JsonIgnore]
    public SonarQubeProject? SonarQubeProject { get; set; } // Navigation property

    [ForeignKey("ProjectScanId")]
    public List<Measure>? Measures { get; set; }
}