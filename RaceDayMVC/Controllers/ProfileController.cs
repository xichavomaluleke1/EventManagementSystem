using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IApiService _apiService;

        public ProfileController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Auth");

            try
            {
                var response = await _apiService.GetAsync("api/users/profile");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var profile = JsonSerializer.Deserialize<ProfileViewModel>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View(profile);
                }
            }
            catch { }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Profile/Update
        [HttpPost]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View("Index", model);

            try
            {
                string url = $"api/users/profile?firstname={Uri.EscapeDataString(model.FirstName)}" +
                             $"&surname={Uri.EscapeDataString(model.LastName)}" +
                             $"&gender=Other";

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                    url += $"&phone_number={Uri.EscapeDataString(model.PhoneNumber)}";

                var response = await _apiService.PutAsync(url, new { });

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to update profile: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: /Profile/UploadPicture
        [HttpPost]
        public async Task<IActionResult> UploadPicture(IFormFile picture)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (picture == null || picture.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image.";
                return RedirectToAction("Index");
            }

            try
            {
                using var content = new MultipartFormDataContent();
                using var stream = picture.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(picture.ContentType);

                // The key "profile_picture" must match the parameter name in the API
                content.Add(fileContent, "profile_picture", picture.FileName);

                var response = await _apiService.PostMultipartAsync("api/users/profile-picture", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Profile picture updated successfully!";
                }
                else
                {
                    // If it fails, show exactly what the API said
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"API Error ({response.StatusCode}): {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Network Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}