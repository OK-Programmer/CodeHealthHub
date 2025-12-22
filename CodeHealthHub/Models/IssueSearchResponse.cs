using System;

namespace CodeHealthHub.Models;

public class IssueSearchResponse
{
    public int Total { get; set; } = 0;
    public int P { get; set; } = 0;
    public int Ps { get; set; } = 0;
    public int EffortTotal { get; set; } = 0;
    public List<Issue> Issues { get; set; } = [];
}
