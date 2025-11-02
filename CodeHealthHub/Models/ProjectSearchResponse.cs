using System;

namespace CodeHealthHub.Models;

public class ProjectSearchResponse
{
    public Paging paging { get; set; }
    public List<SonarQubeProject> components { get; set; }
}
