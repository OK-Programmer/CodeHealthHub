using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeHealthHub.Models;

public class SonarQubeInstance()
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Scheme { get; set; } = "";

    public string Host { get; set; } = "";

    public int Port { get; set; } = 0;
    
    public List<SonarQubeProject> Projects { get; set; } = new();
}
