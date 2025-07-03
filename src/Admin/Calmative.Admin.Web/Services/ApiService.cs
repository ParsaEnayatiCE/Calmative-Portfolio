using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Calmative.Admin.Web.Services
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
    }

    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, string? adminToken);
        Task<HttpResponseMessage> PostAsync(string endpoint, object data, string? adminToken = null);
        Task<HttpResponseMessage> PutAsync(string endpoint, object data, string? adminToken = null);
        Task<HttpResponseMessage> DeleteAsync(string endpoint, string? adminToken = null);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IOptions<ApiSettings> options)
        {
            _httpClient = httpClient;
            _baseUrl = options.Value.BaseUrl;
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? adminToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}{endpoint}");
            
            if (!string.IsNullOrEmpty(adminToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<HttpResponseMessage> PostAsync(string endpoint, object data, string? adminToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{endpoint}");
            
            if (!string.IsNullOrEmpty(adminToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            }

            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PutAsync(string endpoint, object data, string? adminToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}{endpoint}");
            
            if (!string.IsNullOrEmpty(adminToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            }

            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, string? adminToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}{endpoint}");
            
            if (!string.IsNullOrEmpty(adminToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            }

            return await _httpClient.SendAsync(request);
        }
    }
} 