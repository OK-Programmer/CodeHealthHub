using System;

namespace CodeHealthHub.Models;

public class Measure
{
    public required string Metric { get; set; }

    public required string Value { get; set; }
}
