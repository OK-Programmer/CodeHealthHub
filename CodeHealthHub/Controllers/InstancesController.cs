using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using Microsoft.EntityFrameworkCore;
using CodeHealthHub.Data;
using System.Text.Json;
using System.Diagnostics;
using CodeHealthHub.Components.Pages;
using Microsoft.AspNetCore.Http.HttpResults;

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
                Debug.WriteLine($"New instance: {instance.Id}, {instance.Name}, {instance.Scheme}://{instance.Host}:{instance.Port}, {instance.AuthToken}");

                bool instanceExists = await _dbContext.SonarQubeInstances.AnyAsync(i =>
                    i.Name == instance.Name &&
                    i.Scheme == instance.Scheme &&
                    i.Host == instance.Host && 
                    i.Port == instance.Port &&
                    i.AuthToken == instance.AuthToken);
                    
                if (!instanceExists)
                {
                    if (await PingInstance(instance))
                    {
                        await _dbContext.SonarQubeInstances.AddAsync(instance);
                        await _dbContext.SaveChangesAsync();
                        return Created();
                    }
                    else
                    {
                        return BadRequest("Instance is not responding.");
                    }
                }
                else
                {
                    return BadRequest("Instance already exists.");
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

    [HttpPost("edit")]
    public async Task<ActionResult> EditInstance([FromBody] string instanceJson)
    {
        try
        {
            SonarQubeInstance? instance = JsonSerializer.Deserialize<SonarQubeInstance>(instanceJson);
            if (instance != null)
            {
                Debug.WriteLine($"Edit instance: {instance.Id}, {instance.Name}, {instance.Scheme}://{instance.Host}:{instance.Port}, {instance.AuthToken}");

                SonarQubeInstance? existingInstance  = await _dbContext.SonarQubeInstances.FindAsync(instance.Id);

                if (existingInstance != null)
                {
                    if (existingInstance.Name != instance.Name) existingInstance.Name = instance.Name;
                    if (existingInstance.Scheme != instance.Scheme) existingInstance.Scheme = instance.Scheme;
                    if (existingInstance.Host != instance.Host) existingInstance.Host = instance.Host;
                    if (existingInstance.Port != instance.Port) existingInstance.Port = instance.Port;
                    if (existingInstance.AuthToken != instance.AuthToken) existingInstance.AuthToken = instance.AuthToken;
                    _dbContext.Update(existingInstance);
                    await _dbContext.SaveChangesAsync();
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Error: Failed to add edit instance, " + e.Message);
            return UnprocessableEntity();
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

    //Ping the given instance address, should return 'pong' of the instance is up and running
    public async Task<bool> PingInstance(SonarQubeInstance instance)
    {
        UriBuilder builder = new()
        {
            Scheme = instance.Scheme,
            Host = instance.Host,
            Port = instance.Port,
            Path = "/api/system/ping"
        };
        Uri uri = builder.Uri;
        HttpRequestMessage request = new(HttpMethod.Get, uri);
        string? response = await Utility.MakeRequest(request, instance.AuthToken);
        if (response != null && response.Equals("pong"))
        {
            Debug.Print("Pinging new instance returned pong.");
            return true;
        }
        else
        {
            Debug.Print("Pinging new instance did not return response.");
            return false;
        }
    }
}