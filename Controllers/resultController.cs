using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;
using event_management_system.Models;

namespace event_management_system.Controllers
{//start of namespace

    [Route("api")]
    [ApiController]
    public class resultController : ControllerBase
    {//start of class

        // enum for result status (matches DB CHECK constraint)
        public enum result_statuses
        {
            Pending,
            Confirmed,
            DNS,
            DNF,
            Disqualified
        }

        private readonly result_dbconnect _result_db;
        private readonly enrolment_dbconnect _enrolment_db;
        private readonly event_dbconnect _event_db;

        public resultController(
            result_dbconnect result_db,
            enrolment_dbconnect enrolment_db,
            event_dbconnect event_db)
        {
            _result_db = result_db;
            _enrolment_db = enrolment_db;
            _event_db = event_db;
        }

        // ==================================================
        // GET /api/results
        // Role: Any (logged in)
        // Optional: eventId filter
        // ==================================================
        [HttpGet("results")]
        public IActionResult GetAll(int? eventId = null)
        {//start of GetAll()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            return Ok(_result_db.get_all_results(eventId));

        }//end of GetAll()

        // ==================================================
        // GET /api/results/my
        // Role: Participant
        // ==================================================
        [HttpGet("results/my")]
        public IActionResult MyResults()
        {//start of MyResults()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Participant")
                return StatusCode(403, "Only participants can view their results.");

            return Ok(_result_db.get_results_by_participant(user_id.Value));

        }//end of MyResults()

        // ==================================================
        // GET /api/results/{id}
        // Role: Any (logged in)
        // ==================================================
        [HttpGet("results/{id}")]
        public IActionResult GetById(int id)
        {//start of GetById()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            var res = _result_db.get_result_by_id(id);
            if (res == null)
                return NotFound("Result not found.");

            return Ok(res);

        }//end of GetById()

        // ==================================================
        // POST /api/events/{eventId}/results
        // Role: Organiser
        // ==================================================
        [HttpPost("events/{eventId}/results")]
        public IActionResult Capture(
            int eventId,
            int enrolment_id,
            TimeSpan? finish_time,
            int? finish_position,
            int? total_participants,
            int? category_position,
            int? total_category_participants,
            result_statuses status,
            string? notes)
        {//start of Capture()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can capture results.");

            // verify event exists and belongs to this organiser
            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            // verify enrolment exists and belongs to this event
            var enr = _enrolment_db.get_enrolment_by_id(enrolment_id);
            if (enr == null)
                return NotFound("Enrolment not found.");

            if (enr.EventID != eventId)
                return BadRequest("Enrolment is not for this event.");

            string result = _result_db.store_result_info(
                enrolment_id,
                finish_time,
                finish_position,
                total_participants,
                category_position,
                total_category_participants,
                status.ToString(),
                user_id.Value,
                notes);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Created("", result);

        }//end of Capture()

        // ==================================================
        // PUT /api/results/{id}
        // Role: Organiser
        // ==================================================
        [HttpPut("results/{id}")]
        public IActionResult Update(
            int id,
            TimeSpan? finish_time,
            int? finish_position,
            int? total_participants,
            int? category_position,
            int? total_category_participants,
            result_statuses status,
            string? notes)
        {//start of Update()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can update results.");

            var existing = _result_db.get_result_by_id(id);
            if (existing == null)
                return NotFound("Result not found.");

            // verify ownership through the enrolment → event chain
            var enr = _enrolment_db.get_enrolment_by_id(existing.EnrolmentID);
            if (enr == null)
                return NotFound("Enrolment not found.");

            var ev = _event_db.get_event_by_id(enr.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _result_db.update_result_info(
                id,
                finish_time,
                finish_position,
                total_participants,
                category_position,
                total_category_participants,
                status.ToString(),
                notes);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);

        }//end of Update()

        // ==================================================
        // DELETE /api/results/{id}
        // Role: Organiser
        // ==================================================
        [HttpDelete("results/{id}")]
        public IActionResult Delete(int id)
        {//start of Delete()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can delete results.");

            var existing = _result_db.get_result_by_id(id);
            if (existing == null)
                return NotFound("Result not found.");

            var enr = _enrolment_db.get_enrolment_by_id(existing.EnrolmentID);
            if (enr == null)
                return NotFound("Enrolment not found.");

            var ev = _event_db.get_event_by_id(enr.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _result_db.delete_result(id);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);

        }//end of Delete()

        // ==================================================
        // GET /api/events/{eventId}/results
        // Role: Public
        // ==================================================
        [HttpGet("events/{eventId}/results")]
        public IActionResult GetByEvent(int eventId)
        {//start of GetByEvent()

            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            return Ok(_result_db.get_results_by_event(eventId));

        }//end of GetByEvent()

    }//end of class

}//end of namespace