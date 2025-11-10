using System;

namespace CodeHealthHub.Models;

public class IssueSearchResponse
{
    public string total { get; set; }
    public string effortTotal { get; set; }
    public List<Issue> issues { get; set; }
}
