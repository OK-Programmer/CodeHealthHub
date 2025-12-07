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
        bool anyUpdates = false;
        
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
                    anyUpdates = true;
                }
            }
        }

        if (!anyUpdates) 
        {
            return BadRequest("No project weights were updated because project weights did not change.");
        }
        else
        {
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }

    [HttpGet("refresh")]
    public async Task<ActionResult> FetchAndUpdateProjects() {
        List<UriBuilder>? builders = Utility.GetInstancesURIBuilders(_dbContext);
        List<SonarQubeProject>? projects = [];

        // Call project search API for each SonarQube instance to get all projects from sonarqube
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

        if (projects == null) {
            Debug.WriteLine("No projects found.");
            return NotFound();
        }
        else {
            // Store new projects in database
            foreach(SonarQubeProject project in projects)
            {
                bool projectExists = await _dbContext.SonarQubeProjects.AnyAsync(p => p.Key == project.Key);
                if (!projectExists)
                {
                    _dbContext.SonarQubeProjects.Add(project);
                }
            }
            await _dbContext.SaveChangesAsync();
            RecalculateWeights();
            return Ok();
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
}