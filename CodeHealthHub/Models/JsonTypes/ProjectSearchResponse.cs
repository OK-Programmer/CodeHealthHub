using System;
using CodeHealthHub.Models.JsonTypes;

namespace CodeHealthHub.Models.JsonTypes;

public class ProjectSearchResponse
{
    public Paging? Paging { get; set; }
    public List<ProjectComponent>? Components { get; set; }
}
