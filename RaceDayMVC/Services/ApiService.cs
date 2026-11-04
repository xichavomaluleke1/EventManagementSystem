using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint, string? token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PostAsync(string endpoint, object data, string? token = null)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(endpoint, content);
        }

        public async Task<HttpResponseMessage> PutAsync(string endpoint, object data, string? token = null)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync(endpoint, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, string? token = null)
        {
            return await _httpClient.DeleteAsync(endpoint);
        }

        public async Task<HttpResponseMessage> PostMultipartAsync(string endpoint, MultipartFormDataContent content, string? token = null)
        {
            return await _httpClient.PostAsync(endpoint, content);
        }
    }
}