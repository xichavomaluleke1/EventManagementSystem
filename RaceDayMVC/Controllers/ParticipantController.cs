using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IApiService _apiService;

        public ParticipantController(IApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetString("Role") == "Participant";
        }

        // GET: /Participant/MyEvents
        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            try
            {
                // API endpoint for participant enrolments
                var response = await _apiService.GetAsync("api/participant/enrolments");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var enrolments = JsonSerializer.Deserialize<List<MyEventViewModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View(enrolments ?? new List<MyEventViewModel>());
                }
            }
            catch { /* Handle API down */ }

            return View(new List<MyEventViewModel>());
        }

        // GET: /Participant/MyResults
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            try
            {
                // API endpoint for participant results
                var response = await _apiService.GetAsync("api/participant/results");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var results = JsonSerializer.Deserialize<List<MyResultViewModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View(results ?? new List<MyResultViewModel>());
                }
            }
            catch { /* Handle API down */ }

            return View(new List<MyResultViewModel>());
        }
    }
}