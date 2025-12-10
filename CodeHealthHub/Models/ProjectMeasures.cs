using System.ComponentModel.DataAnnotations.Schema;

namespace CodeHealthHub.Models;

public class ProjectMeasures
{
    public required int Id { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string LastAnalysisDate { get; set; } = "";
    
    [ForeignKey("ProjectMeasuresId")]
    public required List<Measure> Measures { get; set; }
}
