using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;

        public HomeController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch upcoming events from the API
                var response = await _apiService.GetAsync("api/events/upcoming");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var events = JsonSerializer.Deserialize<List<EventViewModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View(events ?? new List<EventViewModel>());
                }
            }
            catch
            {
                // If API is down, show empty list
            }

            return View(new List<EventViewModel>());
        }

        // GET: /Home/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Get the event
                var eventResponse = await _apiService.GetAsync($"api/events/{id}");
                if (!eventResponse.IsSuccessStatusCode) return NotFound();

                var eventContent = await eventResponse.Content.ReadAsStringAsync();
                var eventModel = JsonSerializer.Deserialize<EventViewModel>(eventContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (eventModel == null) return NotFound();

                // Get the categories for this event
                var catResponse = await _apiService.GetAsync($"api/events/{id}/categories");
                if (catResponse.IsSuccessStatusCode)
                {
                    var catContent = await catResponse.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (categories != null)
                    {
                        eventModel.Categories = categories;
                    }
                }

                return View(eventModel);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        // POST: /Home/Enrol
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                TempData["ErrorMessage"] = "Only participants can enrol in events.";
                return RedirectToAction("Details", new { id = eventId });
            }

            // Call API to enrol
            var response = await _apiService.PostAsync($"api/events/{eventId}/enrol?category_id={categoryId}", new { });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully enrolled! Check your 'My Events' page.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Enrolment failed: {errorMsg}";
            }

            return RedirectToAction("Details", new { id = eventId });
        }
    }
}