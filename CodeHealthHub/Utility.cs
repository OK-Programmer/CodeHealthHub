using System.Diagnostics;
using CodeHealthHub.Data;
using CodeHealthHub.Models;

namespace CodeHealthHub;

public class Utility() 
{
    private const string authHeader = "Authorization";
    private readonly static string bearerToken = "Bearer " + Environment.GetEnvironmentVariable("SonarUserToken");
    private static readonly HttpClient client = new();
    
    public static async Task<string?> MakeRequest(HttpRequestMessage request) {
        try 
        {
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

    public static UriBuilder GetInstanceUriBuilder(AppDbContext _dbContext, int Id)
    {
        UriBuilder builder = _dbContext.SonarQubeInstances
            .Where(instance => instance.Projects!.Any(p => p.Id == Id))
            .Select(instance => new UriBuilder()
            {
                Scheme = instance.Scheme,
                Host = instance.Host,
                Port = instance.Port
            })
            .FirstOrDefault()!;

        return builder;
    }

    public static double UTD(ProjectScan projScan)
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
}