using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;
using event_management_system.Models;

namespace event_management_system.Controllers
{//start of namespace

    [Route("api")]
    [ApiController]
    public class categoryController : ControllerBase
    {//start of class

        private readonly category_dbconnect _category_db;
        private readonly event_dbconnect _event_db;

        public categoryController(category_dbconnect category_db, event_dbconnect event_db)
        {
            _category_db = category_db;
            _event_db = event_db;
        }

        // ==================================================
        // GET /api/categories
        // Role: Public
        // Optional query: eventId
        // ==================================================
        [HttpGet("categories")]
        public IActionResult GetAll(int? eventId = null)
        {//start of GetAll()

            return Ok(_category_db.get_all_categories(eventId));

        }//end of GetAll()

        // ==================================================
        // GET /api/categories/{id}
        // Role: Public
        // ==================================================
        [HttpGet("categories/{id}")]
        public IActionResult GetById(int id)
        {//start of GetById()

            var cat = _category_db.get_category_by_id(id);
            if (cat == null)
                return NotFound("Category not found.");

            return Ok(cat);

        }//end of GetById()

        // ==================================================
        // POST /api/events/{eventId}/categories
        // Role: Organiser
        // ==================================================
        [HttpPost("events/{eventId}/categories")]
        public IActionResult Create(
            int eventId,
            string category_name,
            string? description,
            int? min_age,
            int? max_age,
            decimal? min_distance,
            decimal? max_distance,
            decimal? entry_fee)
        {//start of Create()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can create categories.");

            // verify event exists and belongs to this organiser
            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _category_db.store_category_info(
                eventId,
                category_name,
                description,
                min_age,
                max_age,
                min_distance,
                max_distance,
                entry_fee);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Created("", result);

        }//end of Create()

        // ==================================================
        // PUT /api/categories/{id}
        // Role: Organiser
        // ==================================================
        [HttpPut("categories/{id}")]
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
        {//start of Update()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can update categories.");

            var existing = _category_db.get_category_by_id(id);
            if (existing == null)
                return NotFound("Category not found.");

            // verify event belongs to this organiser
            var ev = _event_db.get_event_by_id(existing.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your category.");

            string result = _category_db.update_category_info(
                id,
                category_name,
                description,
                min_age,
                max_age,
                min_distance,
                max_distance,
                entry_fee,
                is_active);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);

        }//end of Update()

        // ==================================================
        // DELETE /api/categories/{id}
        // Role: Organiser
        // ==================================================
        [HttpDelete("categories/{id}")]
        public IActionResult Delete(int id)
        {//start of Delete()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can delete categories.");

            var existing = _category_db.get_category_by_id(id);
            if (existing == null)
                return NotFound("Category not found.");

            var ev = _event_db.get_event_by_id(existing.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your category.");

            string result = _category_db.delete_category(id);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);

        }//end of Delete()

    }//end of class

}//end of namespace