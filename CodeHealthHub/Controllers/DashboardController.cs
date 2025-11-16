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
    private readonly string authHeader = "Authorization";
    private readonly string bearerToken = "Bearer " + Environment.GetEnvironmentVariable("SonarUserToken");
    private readonly AppDbContext _dbContext = dbContext;
    readonly HttpClient client = new();
    readonly List<string> projectKeys = [];

    protected UriBuilder FormURIBuilder() {
        UriBuilder builder = new()
        {
            Scheme = "http",
            Host = "localhost",
            Port = 9000
        };
        return builder;
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
        UriBuilder? builder = FormURIBuilder();
        builder.Path = "/api/projects/search";
        Uri? url = builder.Uri;

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        ProjectSearchResponse? response = JsonConvert.DeserializeObject<ProjectSearchResponse>(await MakeRequest(request));
        List<SonarQubeProject>? projects = response?.Components;
        
        if (projects == null) {
            Debug.WriteLine("No projects found.");
            return NotFound();
        }
        else {
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
        List<List<Measure>> measures = [];

        foreach(string key in projectKeys) {
            List<Measure> measure = await GetMeasures(key);
            measures.Add(measure);
        }

        if (measures == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(measures);
        }
    }

    protected async Task<List<Measure>> GetMeasures(string projectKey) {
        List<Measure> measures = [];
        UriBuilder? builder = FormURIBuilder();
        builder.Path = "/api/measures/component";
        builder.Query = $"metricKeys=ncloc,coverage,bugs,vulnerabilities,code_smells&component={projectKey}";
        Uri? url = builder.Uri;

        HttpRequestMessage? request = new(HttpMethod.Get, url);
        string res = await MakeRequest(request);
        MeasureSearchResponse? measureSearchResponse = JsonConvert.DeserializeObject<MeasureSearchResponse>(res);

        if (measureSearchResponse != null) 
        {
            measures = measureSearchResponse.component.measures;
            foreach(Measure measure in measures)
            {
                Debug.WriteLine($"Metric: {measure.metric}    Value: {measure.value}\n");
            }
            return measures;
        }
        else
        {
            return measures;
        }
    }

    [HttpGet("issues")]
    public async Task<ActionResult<List<Issue>>> GetIssues() {
        int pageNumber = 1;
        IssueSearchResponse response = new();

        UriBuilder? builder = FormURIBuilder();
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
        UriBuilder? builder = FormURIBuilder();
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
