using System;

namespace CodeHealthHub.Models;

public class IssueSearchResponse
{
    public int Total { get; set; }
    public int P { get; set; }
    public int Ps { get; set; }
    public int EffortTotal { get; set; }
    public List<Issue> Issues { get; set; }

    public IssueSearchResponse()
    {
        Total = 0;
        P = 0;
        Ps = 0;
        EffortTotal = 0;
        Issues = [];
    }
}
