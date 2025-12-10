using System;
using System.Text.Json.Serialization;

namespace CodeHealthHub.Models;

public class Measure
{
    public required int Id { get; set; }
    public required string Metric { get; set; }
    public required string Value { get; set; }

    // Foreign key to associate Measure with ProjectMeasures
    public required int ProjectMeasuresId { get; set; }
    [JsonIgnore]
    public ProjectMeasures? ProjectMeasures { get; set; }
}
