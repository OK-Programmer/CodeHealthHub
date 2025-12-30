using System;

namespace CodeHealthHub.Models.JsonTypes;

public class ProjectComponent
{
    public required string key { get; set; }

    public required string name { get; set; }

    public required string lastAnalysisDate { get; set; }
}
