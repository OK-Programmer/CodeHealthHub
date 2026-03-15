using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using System.Diagnostics;
using CodeHealthHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace CodeHealthHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    [HttpGet("projects")]
    public async Task<ActionResult<IEnumerable<SonarQubeProject>>> GetProjects() 
    {
        // Fetch all projects from the database
        try
        {
            List<SonarQubeProject>? projects = await _dbContext.SonarQubeProjects.ToListAsync(); 
            return Ok(projects);
        }
        catch (SqliteException sqlExcep)
        {
            Debug.WriteLine($"SqliteException while getting projects: {sqlExcep}");
            return Ok(new List<SonarQubeProject>());
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception while getting projects: {e}");
            return Ok(new List<SonarQubeProject>());
        }
    }

    [HttpGet("measures")]
    public async Task<ActionResult> GetLatestMeasures() 
    {
        try
        {
            // Step 1: get latest scan Id per SonarQubeProject
            var latestScanIds = await _dbContext.ProjectScans
                .GroupBy(ps => ps.SonarQubeProjectId)
                .Select(g => g
                    .OrderByDescending(ps => ps.AnalysisDate)
                    .Select(ps => ps.Id)
                    .First())
                .ToListAsync();

            // Step 2: load scans + measures
            var latestScans = await _dbContext.ProjectScans
                .Where(ps => latestScanIds.Contains(ps.Id))
                .Include(ps => ps.Measures)
                .ToListAsync();

            if (latestScans.Count == 0)
            {
                return NotFound();
            }
            else
            {
                return Ok(latestScans);
            }
        }
        catch (SqliteException sqlExcep)
        {
            Debug.WriteLine($"SqliteException while getting projects: {sqlExcep}");
            return NotFound();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception while getting projects: {e}");
            return NotFound();
        }
    }

    [HttpGet("measures-history/{Id}")]
    public async Task<ActionResult> GetMeasuresHistory(int Id)
    {
        try
        {
            List<ProjectScan> projectScans = await _dbContext.ProjectScans.Where(p => p.SonarQubeProjectId == Id).Include(p => p.Measures).ToListAsync();

            if (projectScans.Count == 0)
            {
                return NotFound();
            }
            else
            {
                return Ok(projectScans);
            }
        }
        catch (SqliteException sqlExcep)
        {
            Debug.WriteLine($"SqliteException while getting projects: {sqlExcep}");
            return NotFound();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception while getting projects: {e}");
            return NotFound();
        }
    }

    [HttpGet("issues")]
    public async Task<ActionResult<List<ProjectIssue>>> GetIssues()
    {
        try
        {
            List<ProjectIssue> projectIssues = await _dbContext.ProjectIssues.Where(i => i.Status == "OPEN").ToListAsync();
            return Ok(projectIssues);
        }
        catch (SqliteException sqlExcep)
        {
            Debug.WriteLine($"SqliteException while getting projects: {sqlExcep}");
            return Ok(new List<ProjectIssue>());
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception while getting projects: {e}");
            return Ok(new List<ProjectIssue>());
        }
    }

    [HttpGet("piechart-colours")]
    public async Task<ActionResult<List<PieChartColour>>> GetPiechartColours()
    {
        try
        {
            List<PieChartColour> piechartColours = _dbContext.PieChartColours.ToList();

            if (piechartColours.Count != 0)
                return Ok(piechartColours);
            else
                return NotFound();
        }
        catch (SqliteException sqlExcep)
        {
            Debug.WriteLine($"SqliteException while getting projects: {sqlExcep}");
            return NotFound();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception while getting projects: {e}");
            return NotFound();
        }
    }
}