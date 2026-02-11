using System.Diagnostics;
using CodeHealthHub.Data;
using CodeHealthHub.Models;

namespace CodeHealthHub;

public class Utility() 
{
    private static readonly HttpClient client = new();
    private const string authHeader = "Authorization";
    
    public static async Task<string?> MakeRequest(HttpRequestMessage request, string authToken) {
        try 
        {
            string bearerToken = "Bearer " + authToken;
            request.Headers.Add(authHeader, bearerToken);
            HttpResponseMessage? response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Task<string>? content = response.Content.ReadAsStringAsync();
                return content.Result;
            }
            else
            {
                Debug.WriteLine("Request failed:");
                Debug.WriteLine(request.Headers);
                Debug.WriteLine(response.Content);
                return null;
            }
        }
        catch (Exception e) 
        {
            Debug.WriteLine(e);
            return null;
        }
    }

    public static Dictionary<int, UriBuilder>? GetAllInstancesURIBuilders(AppDbContext _dbContext) {
        // Fetch SonarQube instances from DB
        List<SonarQubeInstance>? instances = [.. _dbContext.SonarQubeInstances];
        if (instances == null)
        {
            Debug.WriteLine("GetInstancesURIBuilders() could not find any SonarQubeInstances");
            return null;
        }
        
        // List<UriBuilder> builders = [];
        Dictionary<int, UriBuilder> instanceBuilders = [];

        // Create URI builders for each instance
        foreach (SonarQubeInstance instance in instances) {
            UriBuilder builder = new()
            {
                Scheme = instance.Scheme,
                Host = instance.Host,
                Port = instance.Port
            };
            instanceBuilders[instance.Id] = builder;
        }

        // Return dictionary of uri builders for all SonarQubeInstance 
        return instanceBuilders;
    }

    public static UriBuilder GetInstanceUriBuilder(AppDbContext _dbContext, int projId)
    {
        UriBuilder builder = _dbContext.SonarQubeInstances
            .Where(instance => instance.Projects!.Any(p => p.Id == projId))
            .Select(instance => new UriBuilder()
            {
                Scheme = instance.Scheme,
                Host = instance.Host,
                Port = instance.Port
            })
            .FirstOrDefault()!;

        return builder;
    }

    public static string GetInstanceAuthTokenWithProjId(AppDbContext _dbContext, int projId)
    {
        SonarQubeInstance instance =  _dbContext.SonarQubeInstances.Where(i => i.Projects!.Any(p => p.Id == projId)).First();
        return instance.AuthToken;
    }

    public static string GetInstanceAuthTokenWithInstId(AppDbContext _dbContext, int instId)
    {
        SonarQubeInstance instance =  _dbContext.SonarQubeInstances.Where(i => i.Id == instId).First();
        return instance.AuthToken;
    }

    public static double CalculateUTD(ProjectScan projScan)
    {
        double totalDebtMinutes = 0.0;

        foreach (Measure measure in projScan.Measures?? new List<Measure>())
        {
            if (measure.Metric == "sqale_index" || 
                measure.Metric == "reliability_remediation_effort" || 
                measure.Metric == "security_remediation_effort")
            {
                if (double.TryParse(measure.Value, out double debtMinutes))
                {
                    totalDebtMinutes += debtMinutes;
                }
            }
        }

        return totalDebtMinutes / 60.0; // convert to hours
    }

    public static double CalculateUTDCost(double utd, SonarQubeProject project)
    {
        try
        {
            double hourlyRate = project.DeveloperCostPerHour;
            double nEmployees = project.NumOfDevelopers;
            return utd * hourlyRate * nEmployees ;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating UTD Cost: {ex.Message}");
            return 0.0;
        }
    }

    public static double CalculateAverageProjectHealthScore(ProjectScan projScan)
    {
        List<Measure>? measureList = projScan.Measures;

        if (measureList == null)
        {
            return 0.0;
        }
        else
        {
            var phs = new ProjectHealthScore()
            {
                Vulnerability = (int)float.Parse(measureList.Find(m => m.Metric == "security_rating")?.Value ?? "0.0"),
                Bugs = (int)float.Parse(measureList.Find(m => m.Metric == "reliability_rating")?.Value ?? "0.0"),
                CodeSmells = (int)float.Parse(measureList.Find(m => m.Metric == "sqale_rating")?.Value ?? "0.0"),
                SecHotspots = (int)float.Parse(measureList.Find(m => m.Metric == "security_review_rating")?.Value ?? "0.0"),
            };

            double avgPHS = Math.Round((double)(
                phs.Vulnerability + 
                phs.Bugs + 
                phs.CodeSmells + 
                phs.SecHotspots
            ) / 4, MidpointRounding.AwayFromZero);

            return avgPHS;
        }
    }

    public static string TranslateHealthScoreToGrade(double score)
    {
        var grade = score switch
        {
            5.0 => "E",
            4.0 => "D",
            3.0 => "C",
            2.0 => "B",
            1.0 => "A",
            _ => "N/A",
        };
        return grade;
    }
}