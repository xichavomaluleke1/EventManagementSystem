using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;

        public AuthController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync(
                $"api/authenticator/login?email={model.Email}&password={model.Password}",
                new { });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<JsonElement>(content);

                int userId = apiResponse.GetProperty("userID").GetInt32();
                string role = apiResponse.GetProperty("role").GetString();
                string firstName = apiResponse.GetProperty("firstName").GetString();

                // Read and store the profile picture
                string profilePic = apiResponse.TryGetProperty("profilePictureURL", out var p) ? p.GetString() : null;

                HttpContext.Session.SetInt32("UserID", userId);
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("FirstName", firstName);
                HttpContext.Session.SetString("ProfilePictureURL", profilePic ?? "");

                if (role == "Organiser")
                    return RedirectToAction("Dashboard", "Organiser");
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login credentials.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string url = $"api/authenticator/register?email={model.Email}&password={model.Password}" +
                         $"&firstname={model.FirstName}&surname={model.Surname}&role={model.Role}" +
                         $"&date_of_birth={model.DateOfBirth:yyyy-MM-dd}&gender={model.Gender}";

            if (!string.IsNullOrEmpty(model.PhoneNumber))
                url += $"&phone_number={model.PhoneNumber}";

            var response = await _apiService.PostAsync(url, new { });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Registration failed. Email may already exist.");
            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}