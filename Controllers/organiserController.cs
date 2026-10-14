using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;

namespace event_management_system.Controllers
{//start of namespace

    [Route("api/organiser")]
    [ApiController]
    public class organiserController : ControllerBase
    {//start of class

        private readonly event_dbconnect _event_db;
        private readonly enrolment_dbconnect _enrolment_db;
        private readonly result_dbconnect _result_db;

        public organiserController(
            event_dbconnect event_db,
            enrolment_dbconnect enrolment_db,
            result_dbconnect result_db)
        {
            _event_db = event_db;
            _enrolment_db = enrolment_db;
            _result_db = result_db;
        }

        // ==================================================
        // GET /api/organiser/dashboard
        // Role: Organiser
        // Summary of total events, enrolments, upcoming events
        // ==================================================
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {//start of Dashboard()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can access this dashboard.");

            // fetch all events for this organiser
            var my_events = _event_db.get_events_by_organiser(user_id.Value);

            // count total enrolments across all my events
            int total_enrolments = 0;
            int total_results = 0;

            foreach (var ev in my_events)
            {
                var enrolments = _enrolment_db.get_enrolments_by_event(ev.EventID);
                total_enrolments += enrolments.Count;

                var results = _result_db.get_results_by_event(ev.EventID);
                total_results += results.Count;
            }

            // upcoming events (from today onwards)
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

        }//end of Dashboard()

        // ==================================================
        // GET /api/organiser/events
        // Role: Organiser
        // All events created by the current organiser
        // ==================================================
        [HttpGet("events")]
        public IActionResult GetMyEvents()
        {//start of GetMyEvents()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can access this endpoint.");

            return Ok(_event_db.get_events_by_organiser(user_id.Value));

        }//end of GetMyEvents()

        // ==================================================
        // GET /api/organiser/events/{id}
        // Role: Organiser
        // A specific event created by the current organiser
        // ==================================================
        [HttpGet("events/{id}")]
        public IActionResult GetMyEventById(int id)
        {//start of GetMyEventById()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can access this endpoint.");

            var ev = _event_db.get_event_by_id(id);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            return Ok(ev);

        }//end of GetMyEventById()

        // ==================================================
        // GET /api/organiser/events/stats
        // Role: Organiser
        // Stats for all events created by the current organiser
        // ==================================================
        [HttpGet("events/stats")]
        public IActionResult GetMyEventsStats()
        {//start of GetMyEventsStats()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can access this endpoint.");

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

        }//end of GetMyEventsStats()

    }//end of class

}//end of namespace