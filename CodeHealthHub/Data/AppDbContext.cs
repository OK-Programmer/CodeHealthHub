using System;
using CodeHealthHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeHealthHub.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<SonarQubeProject> SonarQubeProjects { get; set; }

    public DbSet<SonarQubeInstance> SonarQubeInstances { get; set; }
}
