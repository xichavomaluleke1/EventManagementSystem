using event_management_system.Database;
using event_management_system.Database.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Organiser dashboard: event summaries, own events, per-event statistics.
    /// </summary>
    [Route("api/organiser")]
    [ApiController]
    public class organiserController : ControllerBase
    {
        private readonly Ievent_dbconnect _event_db;
        private readonly Ienrolment_dbconnect _enrolment_db;
        private readonly Iresult_dbconnect _result_db;

        public organiserController(
            Ievent_dbconnect event_db,
            Ienrolment_dbconnect enrolment_db,
            Iresult_dbconnect result_db)
        {
            _event_db = event_db;
            _enrolment_db = enrolment_db;
            _result_db = result_db;
        }

        /// <summary>Returns dashboard summary: totals and upcoming events.</summary>
        /// <response code="200">Returns summary counts and upcoming list.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not an organiser.</response>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Dashboard()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can access this dashboard.");

            var my_events = _event_db.get_events_by_organiser(user_id.Value);

            int total_enrolments = 0;
            int total_results = 0;

            foreach (var ev in my_events)
            {
                total_enrolments += _enrolment_db.get_enrolments_by_event(ev.EventID).Count;
                total_results += _result_db.get_results_by_event(ev.EventID).Count;
            }

            var upcoming = my_events.Where(e => e.EventDate >= DateTime.Today).ToList();

            return Ok(new
            {
                total_events = my_events.Count,
                total_enrolments = total_enrolments,
                total_results = total_results,
                upcoming_events = upcoming.Count,
                upcoming_list = upcoming.Select(e => new
                {
                    eventID = e.EventID,
                    eventName = e.EventName,
                    eventDate = e.EventDate,
                    location = e.Location,
                    eventType = e.EventType
                })
            });
        }

        /// <summary>Lists all events created by the current organiser.</summary>
        /// <response code="200">Returns list of the organiser's events.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not an organiser.</response>
        [HttpGet("events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetMyEvents()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can access this endpoint.");

            return Ok(_event_db.get_events_by_organiser(user_id.Value));
        }

        /// <summary>Gets a single event created by the current organiser.</summary>
        /// <param name="id">Event ID.</param>
        /// <response code="200">Returns event details.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("events/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetMyEventById(int id)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can access this endpoint.");

            var ev = _event_db.get_event_by_id(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserID != user_id.Value) return StatusCode(403, "Not your event.");

            return Ok(ev);
        }

        /// <summary>Returns enrolment and result stats for all the organiser's events.</summary>
        /// <response code="200">Returns per-event stats.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not an organiser.</response>
        [HttpGet("events/stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetMyEventsStats()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can access this endpoint.");

            var my_events = _event_db.get_events_by_organiser(user_id.Value);
            var stats = new List<object>();

            foreach (var ev in my_events)
            {
                var enrolments = _enrolment_db.get_enrolments_by_event(ev.EventID);
                var results = _result_db.get_results_by_event(ev.EventID);

                stats.Add(new
                {
                    eventID = ev.EventID,
                    eventName = ev.EventName,
                    eventDate = ev.EventDate,
                    eventType = ev.EventType,
                    status = ev.Status,
                    isPublished = ev.IsPublished,
                    total_enrolments = enrolments.Count,
                    confirmed_enrolments = enrolments.Count(e => e.Status == "Confirmed"),
                    pending_enrolments = enrolments.Count(e => e.Status == "Pending"),
                    cancelled_enrolments = enrolments.Count(e => e.Status == "Cancelled"),
                    results_recorded = results.Count
                });
            }

            return Ok(stats);
        }
    }
}