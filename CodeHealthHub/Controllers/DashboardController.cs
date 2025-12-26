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
        return Ok(projects);
    }

    [HttpGet("measures")]
    public async Task<ActionResult> GetAllMeasures() {
        List<ProjectScan> listOfMeasures = await _dbContext.ProjectScans
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

    [HttpGet("issues")]
    public async Task<ActionResult<List<Issue>>> GetIssues() {
        Dictionary<int, UriBuilder>? instanceBuilders = Utility.GetInstancesURIBuilders(_dbContext);
        if (instanceBuilders == null)
        {
            Debug.WriteLine("GetIssues() could not find any SonarQubeInstances");
            return NotFound("GetIssues() could not find any SonarQubeInstances");
        }

        IssueSearchResponse response = new();
        int pageNumber = 1;

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
