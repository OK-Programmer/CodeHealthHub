using System;

namespace CodeHealthHub.Models;

public class SonarQubeProject()
{
    public required int Id { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string LastAnalysisDate { get; set; }
    public required double Weight { get; set; } = 0.0;
}
