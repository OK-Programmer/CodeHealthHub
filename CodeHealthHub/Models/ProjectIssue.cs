using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CodeHealthHub.Models.JsonTypes;

namespace CodeHealthHub.Models;

public class ProjectIssue
{
    public int Id { get; set; }

    public string IssueKey { get; set; } = "";

    public string Severity { get; set; } = "";

    public string Project { get; set; } = "";

    public int? Effort { get; set; }

    public int? Debt { get; set; }

    public string Type { get; set; } = "";

    public string Status { get; set; } = "";

    public string CreationDate { get; set; } = "";

    public int SonarQubeProjectId { get; set; } // Foreign key to SonarQubeProject
    
    [JsonIgnore]
    public SonarQubeProject? SonarQubeProject { get; set; } // Navigation property
}
