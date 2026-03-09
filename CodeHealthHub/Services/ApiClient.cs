using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace CodeHealthHub.Services
{
    public class ApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(IHttpClientFactory httpClientFactory, NavigationManager navigationManager, ILogger<ApiClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _navigationManager = navigationManager;
            _logger = logger;
        }

        private HttpClient GetConfiguredClient()
        {
            var client = _httpClientFactory.CreateClient("LocalApi");
            var httpBaseUri = _navigationManager.BaseUri;
            client.BaseAddress = new Uri(httpBaseUri);
            _logger.LogInformation($"API BaseAddress: {client.BaseAddress}");
            return client;
        }

        public async Task<T?> GetAsync<T>(string requestUri)
        {
            var client = GetConfiguredClient();
            _logger.LogInformation($"Calling: {client.BaseAddress}{requestUri}");
            return await client.GetFromJsonAsync<T>(requestUri);
        }

        public async Task<HttpResponseMessage> GetRawAsync(string requestUri)
        {
            var client = GetConfiguredClient();
            _logger.LogInformation($"Calling: {client.BaseAddress}{requestUri}");
            return await client.GetAsync(requestUri);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T content)
        {
            var client = GetConfiguredClient();
            _logger.LogInformation($"Calling: {client.BaseAddress}{requestUri}");
            return await client.PutAsJsonAsync(requestUri, content);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T content)
        {
            var client = GetConfiguredClient();
            _logger.LogInformation($"Calling: {client.BaseAddress}{requestUri}");
            return await client.PostAsJsonAsync(requestUri, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            var client = GetConfiguredClient();
            _logger.LogInformation($"Calling: {client.BaseAddress}{requestUri}");
            return await client.DeleteAsync(requestUri);
        }
    }
}