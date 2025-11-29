using System;
using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CodeHealthHub.Data;

namespace CodeHealthHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(AppDbContext dbContext) : ControllerBase
{
    private const string authHeader = "Authorization";
    private readonly string bearerToken = "Bearer " + Environment.GetEnvironmentVariable("SonarUserToken");
    private readonly AppDbContext _dbContext = dbContext;
    readonly HttpClient client = new();

    protected List<UriBuilder> GetURIBuilders() {
        // Fetch SonarQube instances from DB
        List<SonarQubeInstance> instances = _dbContext.SonarQubeInstances.ToList();
        List<UriBuilder> builders = [];

        // Create URI builders for each instance
        foreach (SonarQubeInstance instance in instances) {
            UriBuilder builder = new()
            {
                Scheme = instance.Scheme,
                Host = instance.Host,
                Port = instance.Port
            };
            builders.Add(builder);
        }

        // Return list of URI builders/all SonarQube instances
        return builders;
    }

    protected async Task<string> MakeRequest(HttpRequestMessage request) {
        try 
        {
            request.Headers.Add(authHeader, bearerToken);
            HttpResponseMessage? response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Task<string>? content = response.Content.ReadAsStringAsync();
                return content.Result;
            }
            else
            {
                Debug.WriteLine("Request failed:");
                Debug.WriteLine(request.Headers);
                Debug.WriteLine(response.Content);
                return "";
            }
        }
        catch (Exception e) 
        {
            Debug.WriteLine(e);
            Url.Action("/Error");
            return "";
        }
    }

    [HttpGet("refresh-projects")]
    public async Task<ActionResult> FetchProjects() {
        List<UriBuilder>? builders = GetURIBuilders();
        List<SonarQubeProject>? projects = [];

        // Call project search API for each SonarQube instance to get all chosen projects
        foreach (UriBuilder builder in builders)
        {
            builder.Path = "/api/projects/search";
            Uri? uri = builder.Uri;
            HttpRequestMessage request = new(HttpMethod.Get, uri);
            ProjectSearchResponse? response = JsonConvert.DeserializeObject<ProjectSearchResponse>(await MakeRequest(request));
            if (response != null && response.Components != null)
            {
                projects.AddRange(response.Components);
            }
        }

        if (projects == null) {
            Debug.WriteLine("No projects found.");
            return NotFound();
        }
        else {
            // Store new projects in the database
            foreach(SonarQubeProject project in projects)
            {
                var projectExists = await _dbContext.SonarQubeProjects.AnyAsync(p => p.Key == project.Key);
                if (!projectExists)
                {
                    _dbContext.SonarQubeProjects.Add(project);
                    _dbContext.SaveChanges();
                }
            }
            return Ok();
        }
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
    public async Task<ActionResult<List<List<Measure>>>> GetAllMeasures() {
        // Get URI builders for all SonarQube instances
        List<UriBuilder> builders = GetURIBuilders();
        // Fetch all project keys from the database
        List<string> projectKeys = _dbContext.SonarQubeProjects.Select(p => p.Key).ToList();
        List<List<Measure>> listOfMeasures = [];

        // for each SonarQube instance, and for each project in the instance, fetch measures
        foreach(UriBuilder builder in builders) {
            builder.Path = "/api/measures/component";
            foreach(string key in projectKeys) {
                // for each project key, fetch measures
                List<Measure> measures = await GetMeasures(builder, key);
                listOfMeasures.Add(measures);
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

    protected async Task<List<Measure>> GetMeasures(UriBuilder uriBuilder, string projectKey) {
        List<MeasureSearchResponse?> measureSearchResponses = [];
        List<string> healthScoreMetrics = ["security_rating", "reliability_rating", "sqale_rating", "security_review_rating"];

        // TODO: extract metric keys into its own class for user customization
        uriBuilder.Path = "/api/measures/component";
        uriBuilder.Query = $"metricKeys={string.Join(",",healthScoreMetrics.ToArray())}&component={projectKey}";
        Uri? uri = uriBuilder.Uri;
        Debug.WriteLine($"Fetching measures for project {projectKey} from {uri}");

        HttpRequestMessage? request = new(HttpMethod.Get, uri);
        string response = await MakeRequest(request);
        if (response != "")
        {
            measureSearchResponses.AddRange(JsonConvert.DeserializeObject<MeasureSearchResponse>(response));
        }

        // Combine measures from API response into a single list
        if (measureSearchResponses != null)
        {
            List<Measure> measures = [];

            foreach (MeasureSearchResponse? measureResponse in measureSearchResponses)
            {
                if (measureResponse != null && measureResponse.component.measures != null)
                {
                    measures.AddRange(measureResponse.component.measures);
                }
            }

            return measures;
        }
        else
        {
            Debug.WriteLine("GetMeasures(): No measures found. Returning empty list.");
            return [];
        }
    }

    [HttpGet("issues")]
    public async Task<ActionResult<List<Issue>>> GetIssues() {
        int pageNumber = 1;
        IssueSearchResponse response = new();
        List<UriBuilder> builders = GetURIBuilders();

        //TODO: Handle multiple instances properly
        foreach (UriBuilder builder in builders)
        {
            builder.Path = "/api/issues/search";

            do
            {
                builder.Query = $"p={pageNumber}&ps=500";
                Uri? url = builder.Uri;
                HttpRequestMessage? request = new(HttpMethod.Get, url);
                IssueSearchResponse? currentResponse = JsonConvert.DeserializeObject<IssueSearchResponse>(await MakeRequest(request));
                if (currentResponse != null)
                {
                    Debug.WriteLine($"Page {pageNumber} of issues retrieved. Total issues so far: {response.issues.Count}");
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

    public async Task GetQualityGates(string projectKey) {
        List<UriBuilder> builders = GetURIBuilders();

        //TODO: Handle multiple instances properly
        foreach (UriBuilder builder in builders)
        {
            builder.Path = "/api/qualitygates/project_status";
            builder.Query = $"projectKey={projectKey}";
            Uri? url = builder.Uri;

            HttpRequestMessage? request = new HttpRequestMessage(HttpMethod.Get, url);

            object? response = JsonConvert.DeserializeObject(await MakeRequest(request));

            if (response != null) 
            {
                Debug.WriteLine(response.ToString());
            }
        }
    }
}
