using Microsoft.AspNetCore.Http;

namespace event_management_system.Services.Interfaces
{
    public interface Iblob_storage_service
    {
        Task<string> upload_image_async(IFormFile file, string folder);
        Task delete_image_async(string image_url);
    }
}