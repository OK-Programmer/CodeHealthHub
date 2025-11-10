using System;

namespace CodeHealthHub.Models;

public class SonarQubeProject()
{
    public required string Key { get; set; }
    public required string Name { get; set; }
}
