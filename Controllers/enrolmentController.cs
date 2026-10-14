using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;
using event_management_system.Models;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Manages event enrolments: participants enrol and cancel; organisers view and update status.
    /// </summary>
    [Route("api")]
    [ApiController]
    public class enrolmentController : ControllerBase
    {
        /// <summary>Enrolment status options.</summary>
        public enum enrolment_statuses { Pending, Confirmed, Cancelled, Withdrawn, Completed }

        /// <summary>Payment status options.</summary>
        public enum payment_statuses { Pending, Paid, Refunded, Cancelled }

        private readonly enrolment_dbconnect _enrolment_db;
        private readonly event_dbconnect _event_db;

        public enrolmentController(enrolment_dbconnect enrolment_db, event_dbconnect event_db)
        {
            _enrolment_db = enrolment_db;
            _event_db = event_db;
        }

        /// <summary>Lists all enrolments, optionally filtered by event.</summary>
        /// <param name="eventId">Optional event filter.</param>
        /// <response code="200">Returns list of enrolments.</response>
        /// <response code="401">Not authenticated.</response>
        [HttpGet("enrolments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetAll(int? eventId = null)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");
            return Ok(_enrolment_db.get_all_enrolments(eventId));
        }

        /// <summary>Lists the current participant's own enrolments.</summary>
        /// <response code="200">Returns the participant's enrolments.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a participant.</response>
        [HttpGet("enrolments/my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult MyEnrolments()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Participant") return StatusCode(403, "Only participants have enrolments.");
            return Ok(_enrolment_db.get_enrolments_by_participant(user_id.Value));
        }

        /// <summary>Gets a single enrolment by ID.</summary>
        /// <param name="id">Enrolment ID.</param>
        /// <response code="200">Returns enrolment details.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">Enrolment not found.</response>
        [HttpGet("enrolments/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            var enr = _enrolment_db.get_enrolment_by_id(id);
            if (enr == null) return NotFound("Enrolment not found.");
            return Ok(enr);
        }

        /// <summary>Enrols the current participant in an event (Participant only).</summary>
        /// <param name="eventId">Event ID to enrol into.</param>
        /// <param name="category_id">Category ID within the event.</param>
        /// <response code="201">Enrolment created.</response>
        /// <response code="400">Already enrolled.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a participant.</response>
        /// <response code="404">Event not found.</response>
        [HttpPost("events/{eventId}/enrol")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Enrol(int eventId, int category_id)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Participant") return StatusCode(403, "Only participants can enrol.");

            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null) return NotFound("Event not found.");

            string result = _enrolment_db.store_enrolment_info(user_id.Value, eventId, category_id);

            if (result.Contains("already enrolled")) return BadRequest(result);
            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Created("", result);
        }

        /// <summary>Cancels the current participant's enrolment (Participant only).</summary>
        /// <param name="id">Enrolment ID.</param>
        /// <response code="200">Enrolment cancelled.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not your enrolment.</response>
        /// <response code="404">Enrolment not found.</response>
        [HttpPut("enrolments/{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Cancel(int id)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Participant") return StatusCode(403, "Only participants can cancel enrolments.");

            var enr = _enrolment_db.get_enrolment_by_id(id);
            if (enr == null) return NotFound("Enrolment not found.");
            if (enr.ParticipantID != user_id.Value) return StatusCode(403, "Not your enrolment.");

            return Ok(_enrolment_db.cancel_enrolment(id));
        }

        /// <summary>Updates the enrolment and payment status (Organiser only).</summary>
        /// <param name="id">Enrolment ID.</param>
        /// <param name="status">Enrolment status.</param>
        /// <param name="payment_status">Payment status.</param>
        /// <response code="200">Status updated.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Enrolment not found.</response>
        [HttpPut("enrolments/{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateStatus(
            int id,
            enrolment_statuses status,
            payment_statuses payment_status)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can update enrolment status.");

            var enr = _enrolment_db.get_enrolment_by_id(id);
            if (enr == null) return NotFound("Enrolment not found.");

            var ev = _event_db.get_event_by_id(enr.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _enrolment_db.update_enrolment_status(
                id, status.ToString(), payment_status.ToString());

            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }

        /// <summary>Lists all enrolments for a specific event (Organiser only).</summary>
        /// <param name="eventId">Event ID.</param>
        /// <response code="200">Returns list of enrolments.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("events/{eventId}/enrolments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetByEvent(int eventId)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can view event enrolments.");

            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserID != user_id.Value) return StatusCode(403, "Not your event.");

            return Ok(_enrolment_db.get_enrolments_by_event(eventId));
        }

        /// <summary>Returns enrolment statistics for an event (Organiser only).</summary>
        /// <param name="eventId">Event ID.</param>
        /// <response code="200">Returns stats.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("events/{eventId}/enrolments/stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetStats(int eventId)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can view stats.");

            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserID != user_id.Value) return StatusCode(403, "Not your event.");

            var enrolments = _enrolment_db.get_enrolments_by_event(eventId);

            return Ok(new
            {
                total = enrolments.Count,
                pending = enrolments.Count(e => e.Status == "Pending"),
                confirmed = enrolments.Count(e => e.Status == "Confirmed"),
                cancelled = enrolments.Count(e => e.Status == "Cancelled"),
                withdrawn = enrolments.Count(e => e.Status == "Withdrawn"),
                completed = enrolments.Count(e => e.Status == "Completed"),
                paid = enrolments.Count(e => e.PaymentStatus == "Paid"),
                payment_pending = enrolments.Count(e => e.PaymentStatus == "Pending")
            });
        }
    }
}