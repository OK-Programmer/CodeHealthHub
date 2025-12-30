using System;

namespace CodeHealthHub.Models.JsonTypes;

public class MeasureHistory
{
    public string? Metric { get; set; }
    public List<History>? History { get; set; }
}
