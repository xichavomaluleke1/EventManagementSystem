namespace RaceDayMVC.Services
{
    public interface IApiService
    {
        Task<HttpResponseMessage> GetAsync(string endpoint, string? token = null);
        Task<HttpResponseMessage> PostAsync(string endpoint, object data, string? token = null);
        Task<HttpResponseMessage> PutAsync(string endpoint, object data, string? token = null);
        Task<HttpResponseMessage> DeleteAsync(string endpoint, string? token = null);
        Task<HttpResponseMessage> PostMultipartAsync(string endpoint, MultipartFormDataContent content, string? token = null);
    }
}