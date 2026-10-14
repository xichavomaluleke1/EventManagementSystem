using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;
using event_management_system.Models;

namespace event_management_system.Controllers
{//start of namespace

    [Route("api")]
    [ApiController]
    public class enrolmentController : ControllerBase
    {//start of class

        // enum for enrolment status
        public enum enrolment_statuses
        {
            Pending,
            Confirmed,
            Cancelled,
            Withdrawn,
            Completed
        }

        // enum for payment status
        public enum payment_statuses
        {
            Pending,
            Paid,
            Refunded,
            Cancelled
        }

        private readonly enrolment_dbconnect _enrolment_db;
        private readonly event_dbconnect _event_db;

        public enrolmentController(enrolment_dbconnect enrolment_db, event_dbconnect event_db)
        {
            _enrolment_db = enrolment_db;
            _event_db = event_db;
        }

        // ==================================================
        // GET /api/enrolments
        // Role: Any (logged in)
        // Optional: eventId filter
        // ==================================================
        [HttpGet("enrolments")]
        public IActionResult GetAll(int? eventId = null)
        {//start of GetAll()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            return Ok(_enrolment_db.get_all_enrolments(eventId));

        }//end of GetAll()

        // ==================================================
        // GET /api/enrolments/my
        // Role: Participant
        // ==================================================
        [HttpGet("enrolments/my")]
        public IActionResult MyEnrolments()
        {//start of MyEnrolments()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Participant")
                return StatusCode(403, "Only participants have enrolments.");

            return Ok(_enrolment_db.get_enrolments_by_participant(user_id.Value));

        }//end of MyEnrolments()

        // ==================================================
        // GET /api/enrolments/{id}
        // Role: Any (logged in)
        // ==================================================
        [HttpGet("enrolments/{id}")]
        public IActionResult GetById(int id)
        {//start of GetById()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            var enr = _enrolment_db.get_enrolment_by_id(id);
            if (enr == null)
                return NotFound("Enrolment not found.");

            return Ok(enr);

        }//end of GetById()

        // ==================================================
        // POST /api/events/{eventId}/enrol
        // Role: Participant
        // ==================================================
        [HttpPost("events/{eventId}/enrol")]
        public IActionResult Enrol(int eventId, int category_id)
        {//start of Enrol()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Participant")
                return StatusCode(403, "Only participants can enrol.");

            // verify the event exists
            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            string result = _enrolment_db.store_enrolment_info(
                user_id.Value,
                eventId,
                category_id);

            if (result.Contains("already enrolled"))
                return BadRequest(result);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Created("", result);

        }//end of Enrol()

        // ==================================================
        // PUT /api/enrolments/{id}/cancel
        // Role: Participant
        // ==================================================
        [HttpPut("enrolments/{id}/cancel")]
        public IActionResult Cancel(int id)
        {//start of Cancel()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Participant")
                return StatusCode(403, "Only participants can cancel enrolments.");

            var enr = _enrolment_db.get_enrolment_by_id(id);
            if (enr == null)
                return NotFound("Enrolment not found.");

            if (enr.ParticipantID != user_id.Value)
                return StatusCode(403, "Not your enrolment.");

            return Ok(_enrolment_db.cancel_enrolment(id));

        }//end of Cancel()

        // ==================================================
        // PUT /api/enrolments/{id}/status
        // Role: Organiser
        // ==================================================
        [HttpPut("enrolments/{id}/status")]
        public IActionResult UpdateStatus(
            int id,
            enrolment_statuses status,
            payment_statuses payment_status)
        {//start of UpdateStatus()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can update enrolment status.");

            var enr = _enrolment_db.get_enrolment_by_id(id);
            if (enr == null)
                return NotFound("Enrolment not found.");

            // verify event belongs to this organiser
            var ev = _event_db.get_event_by_id(enr.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _enrolment_db.update_enrolment_status(
                id,
                status.ToString(),
                payment_status.ToString());

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);

        }//end of UpdateStatus()

        // ==================================================
        // GET /api/events/{eventId}/enrolments
        // Role: Organiser
        // ==================================================
        [HttpGet("events/{eventId}/enrolments")]
        public IActionResult GetByEvent(int eventId)
        {//start of GetByEvent()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can view event enrolments.");

            // verify event belongs to this organiser
            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            return Ok(_enrolment_db.get_enrolments_by_event(eventId));

        }//end of GetByEvent()

        // ==================================================
        // GET /api/events/{eventId}/enrolments/stats
        // Role: Organiser
        // ==================================================
        [HttpGet("events/{eventId}/enrolments/stats")]
        public IActionResult GetStats(int eventId)
        {//start of GetStats()

            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (role != "Organiser")
                return StatusCode(403, "Only organisers can view stats.");

            // verify event belongs to this organiser
            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            // compute stats from enrolments
            var enrolments = _enrolment_db.get_enrolments_by_event(eventId);

            var stats = new
            {
                total = enrolments.Count,
                pending = enrolments.Count(e => e.Status == "Pending"),
                confirmed = enrolments.Count(e => e.Status == "Confirmed"),
                cancelled = enrolments.Count(e => e.Status == "Cancelled"),
                withdrawn = enrolments.Count(e => e.Status == "Withdrawn"),
                completed = enrolments.Count(e => e.Status == "Completed"),
                paid = enrolments.Count(e => e.PaymentStatus == "Paid"),
                payment_pending = enrolments.Count(e => e.PaymentStatus == "Pending")
            };

            return Ok(stats);

        }//end of GetStats()

    }//end of class

}//end of namespace