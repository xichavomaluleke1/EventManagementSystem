using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;
using event_management_system.Models;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Manages event categories: list, view, create, update, delete.
    /// </summary>
    [Route("api")]
    [ApiController]
    public class categoryController : ControllerBase
    {
        private readonly category_dbconnect _category_db;
        private readonly event_dbconnect _event_db;

        public categoryController(category_dbconnect category_db, event_dbconnect event_db)
        {
            _category_db = category_db;
            _event_db = event_db;
        }

        /// <summary>Lists all categories, optionally filtered by event.</summary>
        /// <param name="eventId">Optional event ID filter.</param>
        /// <response code="200">Returns list of categories.</response>
        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll(int? eventId = null)
        {
            return Ok(_category_db.get_all_categories(eventId));
        }

        /// <summary>Gets a single category by ID.</summary>
        /// <param name="id">Category ID.</param>
        /// <response code="200">Returns category details.</response>
        /// <response code="404">Category not found.</response>
        [HttpGet("categories/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var cat = _category_db.get_category_by_id(id);
            if (cat == null) return NotFound("Category not found.");
            return Ok(cat);
        }

        /// <summary>Adds a category to a specific event (Organiser only).</summary>
        /// <param name="eventId">Parent event ID.</param>
        /// <response code="201">Category created.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpPost("events/{eventId}/categories")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Create(
            int eventId,
            string category_name,
            string? description,
            int? min_age,
            int? max_age,
            decimal? min_distance,
            decimal? max_distance,
            decimal? entry_fee)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can create categories.");

            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserID != user_id.Value) return StatusCode(403, "Not your event.");

            string result = _category_db.store_category_info(
                eventId, category_name, description, min_age, max_age,
                min_distance, max_distance, entry_fee);

            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Created("", result);
        }

        /// <summary>Updates an existing category (Organiser only).</summary>
        /// <param name="id">Category ID.</param>
        /// <response code="200">Category updated.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Category not found.</response>
        [HttpPut("categories/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(
            int id,
            string category_name,
            string? description,
            int? min_age,
            int? max_age,
            decimal? min_distance,
            decimal? max_distance,
            decimal? entry_fee,
            bool? is_active = true)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can update categories.");

            var existing = _category_db.get_category_by_id(id);
            if (existing == null) return NotFound("Category not found.");

            var ev = _event_db.get_event_by_id(existing.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your category.");

            string result = _category_db.update_category_info(
                id, category_name, description, min_age, max_age,
                min_distance, max_distance, entry_fee, is_active);

            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }

        /// <summary>Deletes a category (Organiser only).</summary>
        /// <param name="id">Category ID.</param>
        /// <response code="200">Category deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Category not found.</response>
        [HttpDelete("categories/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can delete categories.");

            var existing = _category_db.get_category_by_id(id);
            if (existing == null) return NotFound("Category not found.");

            var ev = _event_db.get_event_by_id(existing.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your category.");

            string result = _category_db.delete_category(id);
            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }
    }
}