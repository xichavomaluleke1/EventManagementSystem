using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;
using event_management_system.Models;

namespace event_management_system.Controllers
{//start of namespace

    [Route("api/events")]
    [ApiController]
    public class eventController : ControllerBase
    {//start of class

        // creating an enum to hold event type options
        public enum event_types
        {//start of enum

            Run,
            Walk,
            Cycle

        }//end of enum

        private readonly event_dbconnect _event_db;
        private readonly category_dbconnect _category_db;

        public eventController(event_dbconnect event_db, category_dbconnect category_db)
        {
            _event_db = event_db;
            _category_db = category_db;
        }

        // ==================================================
        // GET /api/events
        // Role: Public
        // ==================================================
        [HttpGet]
        public IActionResult GetAll()
        {//start of GetAll()

            return Ok(_event_db.get_all_events());

        }//end of GetAll()

        // ==================================================
        // GET /api/events/upcoming
        // Role: Public
        // (declared before {id} to avoid route conflict)
        // ==================================================
        [HttpGet("upcoming")]
        public IActionResult GetUpcoming()
        {//start of GetUpcoming()

            var all = _event_db.get_all_events();
            var upcoming = all.Where(e => e.EventDate >= DateTime.Today).ToList();

            return Ok(upcoming);

        }//end of GetUpcoming()

        // ==================================================
        // GET /api/events/{id}
        // Role: Public
        // ==================================================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {//start of GetById()

            var ev = _event_db.get_event_by_id(id);
            if (ev == null)
                return NotFound("Event not found.");

            return Ok(ev);

        }//end of GetById()

        // ==================================================
        // GET /api/events/{id}/categories
        // Role: Public
        // ==================================================
        [HttpGet("{id}/categories")]
        public IActionResult GetCategories(int id)
        {//start of GetCategories()

            var ev = _event_db.get_event_by_id(id);
            if (ev == null)
                return NotFound("Event not found.");

            return Ok(_category_db.get_categories_by_event(id));

        }//end of GetCategories()

        // ==================================================
        // POST /api/events
        // Role: Organiser
        // New events always start as "Upcoming"
        // ==================================================
        [HttpPost]
        public IActionResult Create(
            string event_name,
            string? description,
            DateTime event_date,
            string location,
            decimal distance,
            event_types event_type,
            int? max_participants,
            decimal? entry_fee)
        {//start of Create()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can create events.");

            // new events ALWAYS start as Upcoming
            string status = "Upcoming";

            string result = _event_db.store_event_info(
                user_id.Value,
                event_name,
                description,
                event_date,
                location,
                distance,
                event_type.ToString(),
                max_participants,
                entry_fee,
                status);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Created("", result);

        }//end of Create()

        // ==================================================
        // PUT /api/events/{id}
        // Role: Organiser
        // ==================================================
        [HttpPut("{id}")]
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
        {//start of Update()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can update events.");

            var existing = _event_db.get_event_by_id(id);
            if (existing == null)
                return NotFound("Event not found.");

            if (existing.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            // validate status against the CHECK constraint
            string[] allowed_status = { "Upcoming", "Ongoing", "Completed", "Cancelled" };
            if (!allowed_status.Contains(status))
                return BadRequest($"Invalid status. Must be one of: {string.Join(", ", allowed_status)}");

            string result = _event_db.update_event_info(
                id,
                event_name,
                description,
                event_date,
                location,
                distance,
                event_type.ToString(),
                max_participants,
                entry_fee,
                status);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);

        }//end of Update()

        // ==================================================
        // DELETE /api/events/{id}
        // Role: Organiser
        // ==================================================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {//start of Delete()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can delete events.");

            var existing = _event_db.get_event_by_id(id);
            if (existing == null)
                return NotFound("Event not found.");

            if (existing.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _event_db.delete_event(id);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);

        }//end of Delete()

        // ==================================================
        // PUT /api/events/{id}/publish
        // Role: Organiser
        // ==================================================
        [HttpPut("{id}/publish")]
        public IActionResult Publish(int id, bool is_published)
        {//start of Publish()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can publish events.");

            var existing = _event_db.get_event_by_id(id);
            if (existing == null)
                return NotFound("Event not found.");

            if (existing.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _event_db.publish_event(id, is_published);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);

        }//end of Publish()

    }//end of class

}//end of namespace