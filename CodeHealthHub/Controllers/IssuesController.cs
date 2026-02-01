using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using CodeHealthHub.Data;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.ComponentModel;
using CodeHealthHub.Models.JsonTypes;
using CodeHealthHub.Components;

namespace CodeHealthHub.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IssuesController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    [HttpGet("all")]
    public async Task<ActionResult> GetIssuesData()
    {
        List<ProjectIssue>? allIssues = await _dbContext.ProjectIssues.ToListAsync();
        return Ok(allIssues);
    }

    [HttpGet("refresh")]
    public async Task<ActionResult> FetchAndUpdateIssues()
    {
        Dictionary<int, UriBuilder>? instanceBuilders = Utility.GetAllInstancesURIBuilders(_dbContext);
        if (instanceBuilders == null)
        {
            Debug.WriteLine("GetIssues() could not find any SonarQubeInstances");
            return NotFound("GetIssues() could not find any SonarQubeInstances");
        }

        IssueSearchResponse response = new();
        int pageNumber = 1;

        // Fetch: Call issue search API for each SonarQube instance to get all issues from sonarqube
        foreach (int Id in instanceBuilders.Keys)
        {
            UriBuilder builder = instanceBuilders[Id];
            builder.Path = "/api/issues/search";

            do
            {
                builder.Query = $"p={pageNumber}&ps=500";
                Uri? url = builder.Uri;
                HttpRequestMessage? request = new(HttpMethod.Get, url);
                string? responseContent = await Utility.MakeRequest(request);
                if (responseContent == null) 
                { 
                    Debug.WriteLine("GetIssues(): Empty response from request");
                    return NotFound("GetIssues(): Empty response from request"); 
                }

                IssueSearchResponse? currentResponse = JsonConvert.DeserializeObject<IssueSearchResponse>(responseContent);
                if (currentResponse == null)
                {
                    Debug.WriteLine("GetIssues(): No issues found in response");
                    return NotFound("GetIssues(): No issues found in response");
                }
                else
                {
                    if (pageNumber == 1)
                    {
                        response = currentResponse;
                    }
                    else
                    {
                        response!.Issues.AddRange(currentResponse.Issues);
                    }

                    pageNumber++;
                }
            } while (response != null && response.Issues.Count < response.Total);
        }

        // Update: Remove old issues data and store new issues in database
        if (response != null)
        {
            await _dbContext.ProjectIssues.ExecuteDeleteAsync();

            foreach (Issue issue in response.Issues)
            {
                // Check if issue does not exist in DB
                bool issueExists = await _dbContext.ProjectIssues.AnyAsync(i => i.IssueKey == issue.Key);
                if (issueExists)
                {
                    continue;
                }

                ProjectIssue newIssue = new()
                {
                    IssueKey = issue.Key, 
                    Severity = issue.Severity,
                    Project = issue.Project,
                    Effort = issue.Effort.Contains("min") ? int.Parse(issue.Effort[..^3]) : int.Parse(issue.Effort[..^1]) * 60, // Remove 'min' or 'h' suffix and convert to int
                    Debt = issue.Debt.Contains("min") ? int.Parse(issue.Debt[..^3]) : int.Parse(issue.Debt[..^1]) * 60, // Remove 'min' or 'h' suffix and convert to int
                    Type = issue.Type,
                    Status = issue.Status,
                    CreationDate = DateTime.Parse(issue.CreationDate),
                    SonarQubeProjectId = _dbContext.SonarQubeProjects
                        .Where(p => p.Key == issue.Project)
                        .Select(p => p.Id)
                        .SingleOrDefault()
                };

                await _dbContext.ProjectIssues.AddAsync(newIssue);
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
        else
        {
            return NotFound("GetIssues(): No issues found in response");
        }
    }
}
