using System;
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

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    List<string> metricKeys = 
        [
            "security_rating", 
            "reliability_rating", 
            "sqale_rating", 
            "security_review_rating", 
            "sqale_index", // maintainability debt
            "reliability_remediation_effort", // reliability debt
            "security_remediation_effort" // security debt
        ];

    [HttpGet("all")]
    public async Task<ActionResult<List<SonarQubeProject>>> GetAllProjects() 
    {
        List<SonarQubeProject> projects = await _dbContext.SonarQubeProjects.ToListAsync();
        if (projects == null) { return NotFound(); }
        else { return Ok(projects); }
    }

    [HttpPut("update-weights")]
    public async Task<ActionResult> UpdateProjectWeights([FromBody] Dictionary<int, double> newWeights) 
    {
        if (!ValidateWeightUpdate(newWeights)) {
            Debug.WriteLine("UpdateProjectWeights(): Invalid weight update parameter.");
            return BadRequest("UpdateProjectWeights(): Invalid weight update parameter.");
        }

        foreach(var entry in newWeights) 
        {
            int projectId = entry.Key;
            double newWeight = entry.Value;

            SonarQubeProject? project = await _dbContext.SonarQubeProjects.FindAsync(projectId);
            if (project == null) 
            {
                Debug.WriteLine($"UpdateProjectWeight(): Project with ID {projectId} not found.");
                continue;
            }
            else 
            {
                if (project.Weight == newWeight) 
                {
                    continue;
                }
                else 
                {
                    project.Weight = newWeight;
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("update-developer-count")]
    public async Task<ActionResult> UpdateDeveloperCount([FromBody] Dictionary<int, int> newDevCounts) 
    {
        if (!ValidateDevCountUpdate(newDevCounts)) {
            Debug.WriteLine("UpdateDeveloperCount(): Invalid developer count update parameter.");
            return BadRequest("UpdateDeveloperCount(): Invalid developer count update parameter.");
        }
        
        foreach(var entry in newDevCounts) 
        {
            int projectId = entry.Key;
            int newDevCount = entry.Value;

            SonarQubeProject? project = await _dbContext.SonarQubeProjects.FindAsync(projectId);
            if (project == null) 
            {
                Debug.WriteLine($"UpdateDeveloperCount(): Project with ID {projectId} not found.");
                continue;
            }
            else 
            {
                if (project.NumOfDevelopers == newDevCount) 
                {
                    continue;
                }
                else 
                {
                    project.NumOfDevelopers = newDevCount;
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("update-developer-cost")]
    public async Task<ActionResult> UpdateDeveloperCost([FromBody] Dictionary<int, double> newDevCosts) 
    {
        if (!ValidateDevCostUpdate(newDevCosts)) {
            Debug.WriteLine("UpdateDeveloperCost(): Invalid developer cost update parameter.");
            return BadRequest("UpdateDeveloperCost(): Invalid developer cost update parameter.");
        }
        
        foreach(var entry in newDevCosts) 
        {
            int projectId = entry.Key;
            double newDevCost = entry.Value;

            SonarQubeProject? project = await _dbContext.SonarQubeProjects.FindAsync(projectId);
            if (project == null) 
            {
                Debug.WriteLine($"UpdateDeveloperCost(): Project with ID {projectId} not found.");
                continue;
            }
            else 
            {
                if (project.DeveloperCostPerHour == newDevCost) 
                {
                    continue;
                }
                else 
                {
                    project.DeveloperCostPerHour = newDevCost;
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("refresh")]
    public async Task<ActionResult> FetchAndUpdateProjects() 
    {        
        Dictionary<int, UriBuilder>? instanceBuilders = Utility.GetAllInstancesURIBuilders(_dbContext);
        if (instanceBuilders == null || instanceBuilders.Count == 0)
        {
            Debug.WriteLine("FetchAndUpdateProjects() could not find any SonarQubeInstances");
            return NotFound("FetchAndUpdateProjects() could not find any SonarQubeInstances");
        }
        
        List<ProjectComponent>? components = [];
        List<SonarQubeProject>? projects = [];

        // Fetch: Call project search API for each SonarQube instance to get all projects from sonarqube
        foreach (int Id in instanceBuilders.Keys)
        {
            string authToken = Utility.GetInstanceAuthTokenWithInstId(_dbContext, Id);
            UriBuilder builder = instanceBuilders[Id];
            builder.Path = "/api/projects/search";
            Uri? uri = builder.Uri;
            HttpRequestMessage request = new(HttpMethod.Get, uri);
            string? response = await Utility.MakeRequest(request, authToken);
            if (response == null) 
            {
                Debug.WriteLine("FetchAndUpdateProjects(): No response from project search API");
                return NotFound("FetchAndUpdateProjects(): No response from project search API");
            }
            else
            {
                ProjectSearchResponse? projSearchRes = JsonConvert.DeserializeObject<ProjectSearchResponse>(response);
                if (projSearchRes == null || projSearchRes.Components == null || projSearchRes.Components.Count == 0) 
                {
                    Debug.WriteLine("FetchAndUpdateProjects(): deserialized project search response is null or empty");
                    return NotFound("FetchAndUpdateProjects(): deserialized project search response is null or empty");
                }
                else
                {
                    components = projSearchRes.Components;
                    foreach (ProjectComponent component in components)
                    {
                        SonarQubeProject project = new()
                        {
                            Key = component.key,
                            Name = component.name,
                            LastAnalysisDate = DateTime.Parse(component.lastAnalysisDate)
                        };
                        projects.Add(project);
                    }

                    // Add SonarQubeInstance navigation reference to all projects fetched for this sonarqube instance
                    foreach (SonarQubeProject project in projects)
                    {
                        // Instance existence already confirmed by earlier call to GetInstanceURIBuilders
                        project.SonarQubeInstance = _dbContext.SonarQubeInstances.Find(Id)!; 
                    }
                }
            }
        }
 
        // Update: Store new projects in database, update existing ones if last analysis date is different
        if (projects == null || projects.Count == 0) {
            Debug.WriteLine("FetchAndUpdateProjects(): No projects found.");
            return NotFound();
        }
        else {
            bool newProjectsAdded = false;
            DateTime to = DateTime.Today;
            DateTime from = DateTime.Today.AddYears(-1);
            foreach(SonarQubeProject project in projects)
            {
                // Store new projects in database and fetch project measures afterwards,
                bool projectExists = await _dbContext.SonarQubeProjects.AnyAsync(p => p.Key == project.Key);
                if (!projectExists)
                {
                    // Add SonarQubeInstance navigation reference
                    _dbContext.SonarQubeProjects.Add(project);
                    newProjectsAdded = true;
                    await _dbContext.SaveChangesAsync();

                    // Trigger fetch for new project measures
                    await FetchAndUpdateMeasuresHistory(project, from, to);
                }
                // update existing project measures if last analysis date is different
                else 
                {
                    SonarQubeProject? existingProject = await _dbContext.SonarQubeProjects.FirstOrDefaultAsync(p => p.Key == project.Key);
                    if (existingProject != null && existingProject.LastAnalysisDate != project.LastAnalysisDate)
                    {
                        // Update existing project
                        existingProject.LastAnalysisDate = project.LastAnalysisDate;
                        _dbContext.SonarQubeProjects.Update(existingProject);
                        await _dbContext.SaveChangesAsync();

                        // Trigger fetch for new analysis measures
                        await FetchAndUpdateMeasuresHistory(existingProject, from, to);
                    }
                }
            }

            // Call recalculate weight to redistribute weight equally to all projects because new project added
            if (newProjectsAdded) RecalculateWeights();
        }

        return Ok();
    }

    [HttpDelete("{Id}")]
    public async Task<ActionResult> DeleteProject(int Id)
    {
        SonarQubeProject? project = await _dbContext.SonarQubeProjects.FindAsync(Id);
        if (project == null)
        {
            return NotFound();
        }

        _dbContext.SonarQubeProjects.Remove(project);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    // protected async Task FetchAndUpdateMeasures(SonarQubeProject project)                                                                                                                                     
    // {
    //     UriBuilder builder = Utility.GetInstanceUriBuilder(_dbContext, project.Id);

    //     // Initialize new ProjectScan instance and get measures for this scan
    //     ProjectScan? projectScan = await GetMeasures(builder, project);

    //     if (projectScan == null) 
    //     { 
    //         Debug.WriteLine($"FetchAndUpdateMeasures(): No measures found for project {project.Key}"); 
    //         return;
    //     }
    //     else 
    //     {
    //         // Add ProjectScan record if it does not already exist
    //         bool existingPS = await _dbContext.ProjectScans.AnyAsync(p => p.Id == projectScan.Id);
    //         if (!existingPS)
    //         {
    //             _dbContext.ProjectScans.Add(projectScan); 
    //         }
    //         await _dbContext.SaveChangesAsync();
    //         return;
    //     }
    // }

    // protected async Task<ProjectScan?> GetMeasures(UriBuilder uriBuilder, SonarQubeProject project) 
    // {
    //     uriBuilder.Path = "/api/measures/component";
    //     uriBuilder.Query = $"metricKeys={string.Join(",",metricKeys.ToArray())}&component={project.Key}";
    //     Uri? uri = uriBuilder.Uri;

    //     HttpRequestMessage? request = new(HttpMethod.Get, uri);
    //     string? response = await Utility.MakeRequest(request);
        
    //     ProjectScan? projScan;
    //     if (response == null)
    //     {
    //         Debug.WriteLine("GetMeasures(): Null response from request");
    //         return null;
    //     }
    //     else
    //     {
    //         // This deserialization will only populate the Measures property of ProjectScan variable
    //         projScan = JsonConvert.DeserializeObject<MeasureSearchResponse>(response)!.Component;
    //     }

    //     if (projScan == null)
    //     {
    //         Debug.WriteLine("GetMeasures(): No measures found in response");
    //         return null;
    //     }
    //     else
    //     {
    //         // Set rest of ProjectScan variable's properties
    //         projScan.SonarQubeProjectId = project.Id;
    //         projScan.AnalysisDate = project.LastAnalysisDate;
    //         return projScan;
    //     }
    // }

    protected async Task FetchAndUpdateMeasuresHistory(SonarQubeProject project, DateTime from, DateTime to)
    {
        UriBuilder builder = Utility.GetInstanceUriBuilder(_dbContext, project.Id);
        string authToken = Utility.GetInstanceAuthTokenWithProjId(_dbContext, project.Id);

        List<MeasureHistory>? measureHistList = await GetMeasuresHistory(builder, authToken, project, from, to);

        if (measureHistList != null)
        {
            foreach (MeasureHistory measureHist in measureHistList)
            {
                await ParseMeasureHistory(measureHist, project.Id);
                await ParseHistoryValue(measureHist);
            }
        }
    }

    protected async Task<List<MeasureHistory>?> GetMeasuresHistory(UriBuilder uriBuilder, string authToken, SonarQubeProject project, DateTime from, DateTime to)
    {
        string format = "yyyy-MM-dd";
        uriBuilder.Path = "/api/measures/search_history";
        uriBuilder.Query = $"metrics={string.Join(",",metricKeys.ToArray())}&component={project.Key}&from={from.ToString(format)}&to={to.ToString(format)}";
        Uri? uri = uriBuilder.Uri;

        Debug.WriteLine($"GetMeasuresHistory() URI: {uri}");

        HttpRequestMessage? request = new(HttpMethod.Get, uri);
        string? response = await Utility.MakeRequest(request, authToken);

        List<MeasureHistory>? measureHistList;
        if (response == null)
        {
            Debug.WriteLine("GetMeasuresHistory(): Null response from request");
            return null;
        }
        else
        {
            measureHistList = JsonConvert.DeserializeObject<HistoricalMeasuresSearchResponse>(response)!.Measures;
        }

        if (measureHistList == null || measureHistList.Count == 0)
        {
            Debug.WriteLine("GetMeasuresHistory(): No measures history found in response");
            return null;
        }
        else
        {
            return measureHistList;
        }
    }

    protected async Task ParseMeasureHistory(MeasureHistory measureHist, int projectId)
    {
        foreach(History hist in measureHist.History!)
        {
            ProjectScan projectScan = new()
            {
                SonarQubeProjectId = projectId,
                AnalysisDate = DateTime.Parse(hist.Date)
            };

            bool scanExists = await _dbContext.ProjectScans.AnyAsync(p => 
                p.AnalysisDate == projectScan.AnalysisDate &&
                p.SonarQubeProjectId == projectScan.SonarQubeProjectId
            );

            if(!scanExists)
            {
                await _dbContext.ProjectScans.AddAsync(projectScan);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    protected async Task ParseHistoryValue(MeasureHistory metricHistory)
    {
        foreach(History hist in metricHistory.History!)
        {
            Measure newMeasure = new()
            {
                Metric = metricHistory.Metric,
                Value = hist.Value,
                ProjectScanId = _dbContext.ProjectScans
                    .Where(p => p.AnalysisDate == DateTime.Parse(hist.Date))
                    .Select(p => p.Id)
                    .SingleOrDefault()
            };

            bool measureExist = await _dbContext.Measures.AnyAsync(m => 
                m.Metric == newMeasure.Metric &&
                m.Value == newMeasure.Value &&
                m.ProjectScanId == newMeasure.ProjectScanId
            );

            if (!measureExist)
            {
                await _dbContext.Measures.AddAsync(newMeasure);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    protected void RecalculateWeights() 
    {
        List<SonarQubeProject> projects = _dbContext.SonarQubeProjects.ToList();
        double weight = 1.0 / projects.Count;

        foreach (SonarQubeProject project in projects) {
            project.Weight = weight;
            _dbContext.SonarQubeProjects.Update(project);
        }

        _dbContext.SaveChanges();
    }

    protected bool ValidateWeightUpdate(Dictionary<int, double> newWeights) 
    {
        double totalWeight = 0.0;
        foreach(var entry in newWeights) 
        {
            totalWeight += entry.Value;
        }
        // Check if total weight is approximately 1.0 (allowing for floating point precision issues)
        return Math.Abs(totalWeight - 1.0) < 0.0001;
    }

    protected bool ValidateDevCountUpdate(Dictionary<int, int> newDevCounts) 
    {
        foreach(var entry in newDevCounts) 
        {
            if (entry.Value < 0) 
            {
                return false;
            }
        }
        return true;
    }

    protected bool ValidateDevCostUpdate(Dictionary<int, double> newDevCosts) 
    {
        foreach(var entry in newDevCosts) 
        {
            if (entry.Value < 0.0) 
            {
                return false;
            }
        }
        return true;
    }
}