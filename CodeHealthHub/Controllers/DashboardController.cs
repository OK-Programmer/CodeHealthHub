using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using CodeHealthHub.Data;
using System.Reflection.Metadata.Ecma335;

namespace CodeHealthHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    protected void RecalculateWeights() {
        List<SonarQubeProject> projects = _dbContext.SonarQubeProjects.ToList();
        double weight = 1.0 / projects.Count;

        foreach (SonarQubeProject project in projects) {
            project.Weight = weight;
            _dbContext.SonarQubeProjects.Update(project);
        }

        _dbContext.SaveChanges();
    }

    [HttpGet("projects")]
    public async Task<ActionResult<IEnumerable<SonarQubeProject>>> GetProjects() {
        // Fetch all projects from the database
        List<SonarQubeProject> projects = _dbContext.SonarQubeProjects.ToList();
        if (projects == null)
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
        // Get URI builders for all SonarQube instances
        List<UriBuilder> builders = Utility.GetInstancesURIBuilders(_dbContext);
        // Fetch all project keys from the database
        List<string> projectKeys = _dbContext.SonarQubeProjects.Select(p => p.Key).ToList();
        List<ProjectMeasures> listOfMeasures = [];

        // for each SonarQube instance, and for each project in the instance, fetch measures
        foreach(UriBuilder builder in builders) {
            builder.Path = "/api/measures/component";
            foreach(string key in projectKeys) {
                // for each project key, fetch measures
                ProjectMeasures? measures = await GetMeasures(builder, key);
                if (measures == null) 
                { 
                    Debug.WriteLine($"GetAllMeasures(): No measures found for project {key}"); 
                    break;
                }
                else
                { 
                    listOfMeasures.AddRange(measures); 
                }
            }
        }

        if (listOfMeasures == null)
        {
            return NotFound();
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
        
        ProjectMeasures measureComponent;
        if (response == null)
        {
            Debug.WriteLine("GetMeasures(): Null response from request");
            return null;
        }
        else
        {
            measureComponent = JsonConvert.DeserializeObject<MeasureSearchResponse>(response)!.component;
        }

        // Combine measures from API response into a single list
        if (measureComponent == null)
        {
            Debug.WriteLine("GetMeasures(): No measures found in response");
            return null;
        }
        else
        {
            return measureComponent;
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
                    Debug.WriteLine($"Page {pageNumber} of issues retrieved. Total issues so far: {response?.issues.Count}");
                    if (pageNumber == 1)
                    {
                        response = currentResponse;
                    }
                    else
                    {
                        response?.issues.AddRange(currentResponse.issues);
                    }

                    pageNumber++;
                }
            } while (response != null && response.issues.Count < response.total);
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
