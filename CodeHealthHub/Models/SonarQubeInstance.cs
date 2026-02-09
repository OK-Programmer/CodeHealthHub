namespace CodeHealthHub.Models;

public class SonarQubeInstance()
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Scheme { get; set; } = "";

    public string Host { get; set; } = "";

    public int Port { get; set; } = 0;

    public string AuthToken { get; set; } = "";
    
    public List<SonarQubeProject> Projects { get; set; } = new();
}
