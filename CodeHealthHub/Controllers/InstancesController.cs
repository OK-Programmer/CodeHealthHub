using System;
using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using Microsoft.EntityFrameworkCore;
using CodeHealthHub.Data;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using CodeHealthHub.Components.Pages;

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

    [HttpPost("add")]
    public async Task<ActionResult> AddNewInstance([FromBody] string instanceJson)
    {
        try
        {
            SonarQubeInstance? instance = JsonSerializer.Deserialize<SonarQubeInstance>(instanceJson);
            if (instance != null)
            {
                Debug.WriteLine($"New instance: {instance.Id}, {instance.Name}, {instance.Scheme}://{instance.Host}:{instance.Port}");

                bool instanceExists = await _dbContext.SonarQubeInstances.AnyAsync(i => 
                    i.Scheme == instance.Scheme &&
                    i.Host == instance.Host && 
                    i.Port == instance.Port);
                if (!instanceExists)
                {
                    await _dbContext.SonarQubeInstances.AddAsync(instance);
                    await _dbContext.SaveChangesAsync();
                    return Created();
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Error: Failed to add new instance, " + e.Message);
            return BadRequest();
        }
    }

    [HttpDelete("{Id}")]
    public async Task<ActionResult> DeleteInstance(int Id)
    {
        try
        {
            SonarQubeInstance? edittingInstance = await _dbContext.SonarQubeInstances.FindAsync(Id);
            if (edittingInstance == null)
            {
                return NotFound();
            }

            _dbContext.SonarQubeInstances.Remove(edittingInstance);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Error: Deleting instance failed, " + e.Message);
            return NotFound();
        }
    }
}