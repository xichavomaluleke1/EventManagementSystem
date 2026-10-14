using event_management_system.Database;
using event_management_system.Database.Interfaces;
using event_management_system.Models;
using Microsoft.AspNetCore.Mvc;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Manages events: list, view, create, update, delete, publish.
    /// </summary>
    /// <remarks>
    /// Read operations are public. Write operations require the Organiser role.
    /// </remarks>
    [Route("api/events")]
    [ApiController]
    public class eventController : ControllerBase
    {
        /// <summary>Event type options.</summary>
        public enum event_types { Run, Walk, Cycle }

        private readonly Ievent_dbconnect _event_db;
        private readonly Icategory_dbconnect _category_db;

        public eventController(Ievent_dbconnect event_db, Icategory_dbconnect category_db)
        {
            _event_db = event_db;
            _category_db = category_db;
        }

        /// <summary>Lists all events.</summary>
        /// <response code="200">Returns list of events.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            return Ok(_event_db.get_all_events());
        }

        /// <summary>Lists upcoming events (from today onwards).</summary>
        /// <response code="200">Returns upcoming events.</response>
        [HttpGet("upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUpcoming()
        {
            var all = _event_db.get_all_events();
            var upcoming = all.Where(e => e.EventDate >= DateTime.Today).ToList();
            return Ok(upcoming);
        }

        /// <summary>Gets a single event by ID.</summary>
        /// <param name="id">Event ID.</param>
        /// <response code="200">Returns event details.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var ev = _event_db.get_event_by_id(id);
            if (ev == null) return NotFound("Event not found.");
            return Ok(ev);
        }

        /// <summary>Lists all categories for a given event.</summary>
        /// <param name="id">Event ID.</param>
        /// <response code="200">Returns categories.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("{id}/categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCategories(int id)
        {
            var ev = _event_db.get_event_by_id(id);
            if (ev == null) return NotFound("Event not found.");
            return Ok(_category_db.get_categories_by_event(id));
        }

        /// <summary>Creates a new event (Organiser only).</summary>
        /// <remarks>New events always start with Status = "Upcoming".</remarks>
        /// <response code="201">Event created.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not an organiser.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Create(
            string event_name,
            string? description,
            DateTime event_date,
            string location,
            decimal distance,
            event_types event_type,
            int? max_participants,
            decimal? entry_fee)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can create events.");

            string status = "Upcoming";

            string result = _event_db.store_event_info(
                user_id.Value, event_name, description, event_date, location,
                distance, event_type.ToString(), max_participants, entry_fee, status);

            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Created("", result);
        }

        /// <summary>Updates an existing event (Organiser only).</summary>
        /// <param name="id">Event ID.</param>
        /// <param name="status">Must be Upcoming, Ongoing, Completed, or Cancelled.</param>
        /// <response code="200">Event updated.</response>
        /// <response code="400">Invalid status.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(
            int id,
            string event_name,
            string? description,
            DateTime event_date,
            string location,
            decimal distance,
            event_types event_type,
            int? max_participants,
            decimal? entry_fee,
            string status)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can update events.");

            var existing = _event_db.get_event_by_id(id);
            if (existing == null) return NotFound("Event not found.");
            if (existing.OrganiserID != user_id.Value) return StatusCode(403, "Not your event.");

            string[] allowed_status = { "Upcoming", "Ongoing", "Completed", "Cancelled" };
            if (!allowed_status.Contains(status))
                return BadRequest($"Invalid status. Must be one of: {string.Join(", ", allowed_status)}");

            string result = _event_db.update_event_info(
                id, event_name, description, event_date, location,
                distance, event_type.ToString(), max_participants, entry_fee, status);

            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }

        /// <summary>Deletes an event (Organiser only).</summary>
        /// <param name="id">Event ID.</param>
        /// <response code="200">Event deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can delete events.");

            var existing = _event_db.get_event_by_id(id);
            if (existing == null) return NotFound("Event not found.");
            if (existing.OrganiserID != user_id.Value) return StatusCode(403, "Not your event.");

            string result = _event_db.delete_event(id);
            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }

        /// <summary>Publishes or unpublishes an event (Organiser only).</summary>
        /// <param name="id">Event ID.</param>
        /// <param name="is_published">True to publish, false to unpublish.</param>
        /// <response code="200">Publish status updated.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpPut("{id}/publish")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Publish(int id, bool is_published)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can publish events.");

            var existing = _event_db.get_event_by_id(id);
            if (existing == null) return NotFound("Event not found.");
            if (existing.OrganiserID != user_id.Value) return StatusCode(403, "Not your event.");

            string result = _event_db.publish_event(id, is_published);
            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }
    }
}