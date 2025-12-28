using System;

namespace CodeHealthHub.Models;

public class ProjectSearchResponse
{
    public Paging? Paging { get; set; }
    public List<SonarQubeProject>? Components { get; set; }
}
