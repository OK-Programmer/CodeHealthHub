using System;

namespace CodeHealthHub.Models;

public class ProjectSearchResponse
{
    public required Paging Paging { get; set; }
    public required List<SonarQubeProject> Components { get; set; }
}
