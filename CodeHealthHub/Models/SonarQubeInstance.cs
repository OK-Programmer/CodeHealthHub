using System;

namespace CodeHealthHub.Models;

public class SonarQubeInstance()
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Scheme { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
}
