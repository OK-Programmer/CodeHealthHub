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

    public static List<UriBuilder> GetInstancesURIBuilders(AppDbContext _dbContext) {
        // Fetch SonarQube instances from DB
        List<SonarQubeInstance> instances = _dbContext.SonarQubeInstances.ToList();
        List<UriBuilder> builders = [];

        // Create URI builders for each instance
        foreach (SonarQubeInstance instance in instances) {
            UriBuilder builder = new()
            {
                Scheme = instance.Scheme,
                Host = instance.Host,
                Port = instance.Port
            };
            builders.Add(builder);
        }

        // Return list of URI builders/all SonarQube instances
        return builders;
    }
}