using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using CodeHealthHub.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeHealthHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    [HttpGet("projects")]
    public async Task<ActionResult<IEnumerable<SonarQubeProject>>> GetProjects() {
        // Fetch all projects from the database
        List<SonarQubeProject> projects = await _dbContext.SonarQubeProjects.ToListAsync();
        if (projects.Count == 0)
        {
            return NotFound();
        }
        else
        {
            return Ok(projects);
        }

    }

    [HttpGet("measures")]
    public async Task<ActionResult> GetAllMeasures() {
        List<ProjectMeasures> listOfMeasures = await _dbContext.ProjectMeasures
            .Include(pm => pm.Measures)
            .ToListAsync();

        if (listOfMeasures.Count == 0)
        {
            return NotFound("No measures found.");
        }
        else
        {
            return Ok(listOfMeasures);
        }
    }

    protected async Task<ProjectMeasures?> GetMeasures(UriBuilder uriBuilder, string projectKey) {
        List<string> healthScoreMetrics = 
        [
            "security_rating", 
            "reliability_rating", 
            "sqale_rating", 
            "security_review_rating", 
            "sqale_index", // maintainability debt
            "reliability_remediation_effort", // reliability debt
            "security_remediation_effort" // security debt
        ];

        uriBuilder.Path = "/api/measures/component";
        uriBuilder.Query = $"metricKeys={string.Join(",",healthScoreMetrics.ToArray())}&component={projectKey}";
        Uri? uri = uriBuilder.Uri;

        HttpRequestMessage? request = new(HttpMethod.Get, uri);
        string? response = await Utility.MakeRequest(request);
        
        ProjectMeasures? projMeasure;
        if (response == null)
        {
            Debug.WriteLine("GetMeasures(): Null response from request");
            return null;
        }
        else
        {
            projMeasure = JsonConvert.DeserializeObject<MeasureSearchResponse>(response)!.Component;
        }

        if (projMeasure == null)
        {
            Debug.WriteLine("GetMeasures(): No measures found in response");
            return null;
        }
        else
        {
            // Retrieve last analysis date from associated project
            SonarQubeProject? project = _dbContext.SonarQubeProjects.FirstOrDefault(p => p.Key == projMeasure.Key);
            if (project != null)
            {
                projMeasure.LastAnalysisDate = project.LastAnalysisDate;
            }
            return projMeasure;
        }
    }

    // TODO: No function calls this yet. Call this in RefreshAndUpdateProjects to update measures together with project
    [HttpGet("refresh-measures")]
    public async Task<ActionResult> FetchAndUpdateMeasures() 
    {
        List<ProjectMeasures> projMeasuresList = [];
        List<UriBuilder> builders = Utility.GetInstancesURIBuilders(_dbContext);
        List<string> projectKeys = await _dbContext.SonarQubeProjects.Select(p => p.Key).ToListAsync();

        // for each SonarQube instance, and for each project in the instance, fetch measures
        foreach(UriBuilder builder in builders) {
            builder.Path = "/api/measures/component";
            foreach(string key in projectKeys) {
                // for each project key, fetch measures
                ProjectMeasures? projectMeasures = await GetMeasures(builder, key);
                if (projectMeasures == null) 
                { 
                    Debug.WriteLine($"RefreshMeasures(): No measures found for project {key}"); 
                    break;
                }
                else
                { 
                    projMeasuresList.Add(projectMeasures); 
                }
            }
        }

        // If projectMeasures is null, return NotFound
        if (projMeasuresList == null)
        {
            Debug.WriteLine("No measures found.");
            return NotFound();
        }
        else 
        {
            // Store new ProjectMeasures in database
            foreach(ProjectMeasures pm in projMeasuresList)
            {
                // Check if ProjectMeasures entry already exists
                bool existingPM = await _dbContext.ProjectMeasures.AnyAsync(p => 
                    p.Key == pm.Key && 
                    p.LastAnalysisDate == pm.LastAnalysisDate
                );

                if (!existingPM)
                {
                    _dbContext.ProjectMeasures.Add(pm); 
                }
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }

    [HttpGet("issues")]
    public async Task<ActionResult<List<Issue>>> GetIssues() {
        int pageNumber = 1;
        IssueSearchResponse response = new();
        List<UriBuilder> builders = Utility.GetInstancesURIBuilders(_dbContext);

        foreach (UriBuilder builder in builders)
        {
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
                    return NotFound(); 
                }

                IssueSearchResponse? currentResponse = JsonConvert.DeserializeObject<IssueSearchResponse>(responseContent);
                if (currentResponse == null)
                {
                    Debug.WriteLine("GetIssues(): No issues found in response");
                    return NotFound();
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

        if (response != null)
        {
            return Ok(response);
        }
        else
        {
            return NotFound();
        }
    }
}
