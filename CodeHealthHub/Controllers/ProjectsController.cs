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
        return Ok(projects);
    }
}