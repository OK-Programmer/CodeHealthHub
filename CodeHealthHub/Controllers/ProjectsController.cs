using System;
using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using CodeHealthHub.Data;

namespace CodeHealthHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    [HttpGet("all")]
    public async Task<ActionResult<List<SonarQubeProject>>> GetAllProjects() {
        List<SonarQubeProject> projects = await _dbContext.SonarQubeProjects.ToListAsync();
        if (projects == null) { return NotFound(); }
        else { return Ok(projects); }
    }

    [HttpPut("update-weights")]
    public async Task<ActionResult> UpdateProjectWeights([FromBody] Dictionary<int, double> newWeights) {
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
    public async Task<ActionResult> UpdateDeveloperCount([FromBody] Dictionary<int, int> newDevCounts) {
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
    public async Task<ActionResult> UpdateDeveloperCost([FromBody] Dictionary<int, double> newDevCosts) {
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
    public async Task<ActionResult> FetchAndUpdateProjects() {
        List<UriBuilder>? builders = Utility.GetInstancesURIBuilders(_dbContext);
        List<SonarQubeProject>? projects = [];

        // Fetch: Call project search API for each SonarQube instance to get all projects from sonarqube
        foreach (UriBuilder builder in builders)
        {
            builder.Path = "/api/projects/search";
            Uri? uri = builder.Uri;
            HttpRequestMessage request = new(HttpMethod.Get, uri);
            string? response = await Utility.MakeRequest(request);
            if (response == null) 
            {
                Debug.WriteLine("FetchAndUpdateProjects(): No response from project search API");
                return NotFound("FetchAndUpdateProjects(): No response from project search API");
            }
            else
            {
                ProjectSearchResponse? projSearchRes = JsonConvert.DeserializeObject<ProjectSearchResponse>(response);
                if (projSearchRes == null || projSearchRes.Components == null) 
                {
                    Debug.WriteLine("FetchAndUpdateProjects(): Null deserialized project search response");
                    return NotFound("FetchAndUpdateProjects(): Null deserialized project search response");
                }
                else
                {
                    projects.AddRange(projSearchRes.Components);
                }
            }
        }
 
        // Update: Store new projects in database, update existing ones if last analysis date is different
        if (projects == null) {
            Debug.WriteLine("No projects found.");
            return NotFound();
        }
        else {
            bool newProjectsAdded = false;
            foreach(SonarQubeProject project in projects)
            {
                // Store new projects in database, else update existing ones if last analysis date is different
                bool projectExists = await _dbContext.SonarQubeProjects.AnyAsync(p => p.Key == project.Key);
                if (!projectExists)
                {
                    _dbContext.SonarQubeProjects.Add(project);
                    newProjectsAdded = true;
                    await _dbContext.SaveChangesAsync();

                    // Trigger fetch for new project measures
                    await FetchAndUpdateMeasures(project);
                }
                else
                {
                    SonarQubeProject? existingProject = await _dbContext.SonarQubeProjects.FirstOrDefaultAsync(p => p.Key == project.Key);
                    if (existingProject != null && existingProject.LastAnalysisDate != project.LastAnalysisDate)
                    {
                        // Update existing project
                        existingProject.LastAnalysisDate = project.LastAnalysisDate;
                        _dbContext.SonarQubeProjects.Update(existingProject);

                        // Trigger fetch for new analysis measures
                        await FetchAndUpdateMeasures(existingProject);
                    }
                }
            }

            if (newProjectsAdded) RecalculateWeights();

            return Ok();
        }
    }

    /*
    Fetches measures for a given SonarQube project in case of new scan and updates the database.
    */
    public async Task FetchAndUpdateMeasures(SonarQubeProject project)                                                                                                                                     
    {
        UriBuilder builder = _dbContext.SonarQubeInstances
            .Where(instance => instance.Projects!.Any(p => p.Id == project.Id))
            .Select(instance => new UriBuilder()
            {
                Scheme = instance.Scheme,
                Host = instance.Host,
                Port = instance.Port
            })
            .FirstOrDefault()!;

        // Initialize new ProjectScan instance and get measures for this scan
        ProjectScan? projectScan = await GetMeasures(builder, project);

        if (projectScan == null) 
        { 
            Debug.WriteLine($"FetchAndUpdateMeasures(): No measures found for project {project.Key}"); 
            return;
        }
        else 
        {
            // Add ProjectScan record if it does not already exist
            bool existingPS = await _dbContext.ProjectScans.AnyAsync(p => p.Id == projectScan.Id);
            if (!existingPS)
            {
                _dbContext.ProjectScans.Add(projectScan); 
            }
            await _dbContext.SaveChangesAsync();
            return;
        }
    }

    protected void RecalculateWeights() {
        List<SonarQubeProject> projects = _dbContext.SonarQubeProjects.ToList();
        double weight = 1.0 / projects.Count;

        foreach (SonarQubeProject project in projects) {
            project.Weight = weight;
            _dbContext.SonarQubeProjects.Update(project);
        }

        _dbContext.SaveChanges();
    }

    protected async Task<ProjectScan?> GetMeasures(UriBuilder uriBuilder, SonarQubeProject project) {
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

        uriBuilder.Path = "/api/measures/component";
        uriBuilder.Query = $"metricKeys={string.Join(",",metricKeys.ToArray())}&component={project.Key}";
        Uri? uri = uriBuilder.Uri;

        HttpRequestMessage? request = new(HttpMethod.Get, uri);
        string? response = await Utility.MakeRequest(request);
        
        ProjectScan? projScan;
        if (response == null)
        {
            Debug.WriteLine("GetMeasures(): Null response from request");
            return null;
        }
        else
        {
            // This deserialization will only populate the Measures property of ProjectScan variable
            projScan = JsonConvert.DeserializeObject<MeasureSearchResponse>(response)!.Component;
        }

        if (projScan == null)
        {
            Debug.WriteLine("GetMeasures(): No measures found in response");
            return null;
        }
        else
        {
            projScan.SonarQubeProjectId = project.Id;
            projScan.AnalysisDate = project.LastAnalysisDate;
            projScan.CreatedAt = DateTime.UtcNow.ToString();
            return projScan;
        }
    }

    protected bool ValidateWeightUpdate(Dictionary<int, double> newWeights) {
        double totalWeight = 0.0;
        foreach(var entry in newWeights) 
        {
            totalWeight += entry.Value;
        }
        // Check if total weight is approximately 1.0 (allowing for floating point precision issues)
        return Math.Abs(totalWeight - 1.0) < 0.0001;
    }

    protected bool ValidateDevCountUpdate(Dictionary<int, int> newDevCounts) {
        foreach(var entry in newDevCounts) 
        {
            if (entry.Value < 0) 
            {
                return false;
            }
        }
        return true;
    }

    protected bool ValidateDevCostUpdate(Dictionary<int, double> newDevCosts) {
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