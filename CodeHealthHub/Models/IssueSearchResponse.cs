using System;

namespace CodeHealthHub.Models;

public class IssueSearchResponse
{
    public int total { get; set; }
    public int p { get; set; }
    public int ps { get; set; }
    public int effortTotal { get; set; }
    public List<Issue> issues { get; set; } = new();
}
