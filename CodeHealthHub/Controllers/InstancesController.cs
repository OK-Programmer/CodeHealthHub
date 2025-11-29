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
public class InstancesController(AppDbContext dbContext) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext;

    [HttpGet("all")]
    public async Task<ActionResult<List<SonarQubeProject>>> GetAllInstances() {
        List<SonarQubeInstance> instances = await _dbContext.SonarQubeInstances.ToListAsync();
        return Ok(instances);
    }
}