using System;
using CodeHealthHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeHealthHub.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SonarQubeProject> SonarQubeProjects { get; set; }

    public DbSet<SonarQubeInstance> SonarQubeInstances { get; set; }

    public DbSet<ProjectMeasures> ProjectMeasures { get; set; }

    public DbSet<Measure> Measures { get; set; }
}
