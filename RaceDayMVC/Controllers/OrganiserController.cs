using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IApiService _apiService;

        public OrganiserController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Helper method to check if the user is an Organiser
        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role") == "Organiser";
        }

        // ==========================================
        // DASHBOARD
        // ==========================================
        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            try
            {
                var response = await _apiService.GetAsync("api/organiser/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ViewBag.DashboardData = JsonSerializer.Deserialize<JsonElement>(content);
                }
            }
            catch
            {
                ViewBag.Error = "Could not connect to the API.";
            }

            return View();
        }

        // ==========================================
        // CREATE EVENT
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Safely encode string inputs for the URL
                string safeName = Uri.EscapeDataString(model.EventName);
                string safeLocation = Uri.EscapeDataString(model.Location);
                string safeDesc = string.IsNullOrEmpty(model.Description) ? "" : Uri.EscapeDataString(model.Description);

                // Build the URL exactly as the API expects it (Query parameters, not JSON body)
                string url = $"api/events?event_name={safeName}&event_date={model.EventDate:yyyy-MM-dd}" +
                             $"&location={safeLocation}&distance={model.Distance}&event_type={model.EventType}";

                if (!string.IsNullOrEmpty(safeDesc)) url += $"&description={safeDesc}";
                if (model.MaxParticipants.HasValue) url += $"&max_participants={model.MaxParticipants}";
                if (model.EntryFee.HasValue) url += $"&entry_fee={model.EntryFee}";

                var response = await _apiService.PostAsync(url, new { });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Failed to create event. API says: {errorContent}";
                    return View(model);
                }

                // Get the newly created event's ID
                var eventsResponse = await _apiService.GetAsync("api/organiser/events");
                int newEventId = 0;

                if (eventsResponse.IsSuccessStatusCode)
                {
                    var content = await eventsResponse.Content.ReadAsStringAsync();
                    var events = JsonSerializer.Deserialize<JsonElement[]>(content);
                    if (events != null && events.Length > 0) newEventId = events[0].GetProperty("eventID").GetInt32();
                }

                // Upload Banner to Azure
                if (model.BannerImage != null && model.BannerImage.Length > 0 && newEventId > 0)
                {
                    using var content = new MultipartFormDataContent();
                    using var stream = model.BannerImage.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.BannerImage.ContentType);
                    content.Add(fileContent, "banner_image", model.BannerImage.FileName);

                    await _apiService.PostMultipartAsync($"api/events/{newEventId}/banner", content);
                }

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View(model);
            }
        }

        // ==========================================
        // MANAGE CATEGORIES
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Categories(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            try
            {
                var evRes = await _apiService.GetAsync($"api/events/{id}");
                if (!evRes.IsSuccessStatusCode) return NotFound();

                var evContent = await evRes.Content.ReadAsStringAsync();
                var eventModel = JsonSerializer.Deserialize<EventViewModel>(evContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var catRes = await _apiService.GetAsync($"api/events/{id}/categories");
                if (catRes.IsSuccessStatusCode)
                {
                    var catContent = await catRes.Content.ReadAsStringAsync();
                    eventModel.Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CategoryViewModel>();
                }

                ViewBag.Event = eventModel;
                return View(new CategoryFormViewModel { EventId = id });
            }
            catch { return RedirectToAction("Dashboard"); }
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryFormViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            if (!ModelState.IsValid) return RedirectToAction("Categories", new { id = model.EventId });

            string safeName = Uri.EscapeDataString(model.CategoryName);
            string url = $"api/events/{model.EventId}/categories?category_name={safeName}";

            if (!string.IsNullOrEmpty(model.Description)) url += $"&description={Uri.EscapeDataString(model.Description)}";
            if (model.MinAge.HasValue) url += $"&min_age={model.MinAge}";
            if (model.MaxAge.HasValue) url += $"&max_age={model.MaxAge}";
            if (model.EntryFee.HasValue) url += $"&entry_fee={model.EntryFee}";

            await _apiService.PostAsync(url, new { });
            TempData["SuccessMessage"] = "Category added successfully!";
            return RedirectToAction("Categories", new { id = model.EventId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int categoryId, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            await _apiService.DeleteAsync($"api/categories/{categoryId}");
            TempData["SuccessMessage"] = "Category deleted.";
            return RedirectToAction("Categories", new { id = eventId });
        }

        // ==========================================
        // VIEW ENROLMENTS
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Enrolments(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            try
            {
                var evRes = await _apiService.GetAsync($"api/events/{id}");
                var evContent = await evRes.Content.ReadAsStringAsync();
                var eventModel = JsonSerializer.Deserialize<EventViewModel>(evContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (eventModel == null) return NotFound();

                var viewModel = new EventEnrolmentsViewModel
                {
                    EventID = eventModel.EventID,
                    EventName = eventModel.EventName
                };

                var enrRes = await _apiService.GetAsync($"api/events/{id}/enrolments");
                if (enrRes.IsSuccessStatusCode)
                {
                    var enrContent = await enrRes.Content.ReadAsStringAsync();
                    var enrolments = JsonSerializer.Deserialize<JsonElement[]>(enrContent);

                    if (enrolments != null)
                    {
                        foreach (var enr in enrolments)
                        {
                            viewModel.Enrolments.Add(new EnrolmentDetail
                            {
                                EnrolmentID = enr.GetProperty("enrolmentID").GetInt32(),
                                ParticipantID = enr.GetProperty("participantID").GetInt32(),
                                Status = enr.GetProperty("status").GetString() ?? "",
                                PaymentStatus = enr.GetProperty("paymentStatus").GetString() ?? "",
                                ParticipantName = $"Participant #{enr.GetProperty("participantID").GetInt32()}",
                                CategoryName = $"Category #{enr.GetProperty("categoryID").GetInt32()}"
                            });
                        }
                    }
                }

                return View(viewModel);
            }
            catch { return RedirectToAction("Dashboard"); }
        }

        // ==========================================
        // CAPTURE RESULTS
        // ==========================================
        [HttpGet]
        public IActionResult CaptureResult(int enrolmentId, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            return View(new ResultEntryViewModel { EnrolmentId = enrolmentId, EventId = eventId });
        }

        [HttpPost]
        public async Task<IActionResult> CaptureResult(ResultEntryViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            if (!ModelState.IsValid) return View(model);

            // API expects finish_time as HH:mm:ss
            string url = $"api/events/{model.EventId}/results?enrolment_id={model.EnrolmentId}&finish_time={model.FinishTime}&status={model.Status}";
            if (model.FinishPosition.HasValue) url += $"&finish_position={model.FinishPosition}";
            if (model.TotalParticipants.HasValue) url += $"&total_participants={model.TotalParticipants}";

            var response = await _apiService.PostAsync(url, new { });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Result recorded successfully!";
                return RedirectToAction("Enrolments", new { id = model.EventId });
            }

            var error = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Failed to save result: {error}";
            return View(model);
        }
    }
}