using event_management_system.Database;
using event_management_system.Database.Interfaces;
using event_management_system.Models;
using Microsoft.AspNetCore.Mvc;
using ResultModel = event_management_system.Models.Results;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Manages race results: capture, view, update, delete.
    /// </summary>
    [Route("api")]
    [ApiController]
    public class resultController : ControllerBase
    {
        /// <summary>Result status options (matches DB CHECK constraint).</summary>
        public enum result_statuses { Pending, Confirmed, DNS, DNF, Disqualified }

        private readonly Iresult_dbconnect _result_db;
        private readonly Ienrolment_dbconnect _enrolment_db;
        private readonly Ievent_dbconnect _event_db;

        public resultController(
            Iresult_dbconnect result_db,
            Ienrolment_dbconnect enrolment_db,
            Ievent_dbconnect event_db)
        {
            _result_db = result_db;
            _enrolment_db = enrolment_db;
            _event_db = event_db;
        }

        /// <summary>Lists all results, optionally filtered by event.</summary>
        /// <param name="eventId">Optional event filter.</param>
        /// <response code="200">Returns list of results.</response>
        /// <response code="401">Not authenticated.</response>
        [HttpGet("results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetAll(int? eventId = null)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");
            return Ok(_result_db.get_all_results(eventId));
        }

        /// <summary>Lists the current participant's own results.</summary>
        /// <response code="200">Returns the participant's results.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a participant.</response>
        [HttpGet("results/my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult MyResults()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Participant") return StatusCode(403, "Only participants can view their results.");
            return Ok(_result_db.get_results_by_participant(user_id.Value));
        }

        /// <summary>Gets a single result by ID.</summary>
        /// <param name="id">Result ID.</param>
        /// <response code="200">Returns result details.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">Result not found.</response>
        [HttpGet("results/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            var res = _result_db.get_result_by_id(id);
            if (res == null) return NotFound("Result not found.");
            return Ok(res);
        }

        /// <summary>Captures a result for an enrolment (Organiser only).</summary>
        /// <param name="eventId">Event ID.</param>
        /// <response code="201">Result recorded.</response>
        /// <response code="400">Enrolment does not belong to this event.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event or enrolment not found.</response>
        [HttpPost("events/{eventId}/results")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can capture results.");

            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserID != user_id.Value) return StatusCode(403, "Not your event.");

            var enr = _enrolment_db.get_enrolment_by_id(enrolment_id);
            if (enr == null) return NotFound("Enrolment not found.");
            if (enr.EventID != eventId) return BadRequest("Enrolment is not for this event.");

            string result = _result_db.store_result_info(
                enrolment_id, finish_time, finish_position, total_participants,
                category_position, total_category_participants,
                status.ToString(), user_id.Value, notes);

            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Created("", result);
        }

        /// <summary>Updates an existing result (Organiser only).</summary>
        /// <param name="id">Result ID.</param>
        /// <response code="200">Result updated.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Result not found.</response>
        [HttpPut("results/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(
            int id,
            TimeSpan? finish_time,
            int? finish_position,
            int? total_participants,
            int? category_position,
            int? total_category_participants,
            result_statuses status,
            string? notes)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can update results.");

            var existing = _result_db.get_result_by_id(id);
            if (existing == null) return NotFound("Result not found.");

            var enr = _enrolment_db.get_enrolment_by_id(existing.EnrolmentID);
            if (enr == null) return NotFound("Enrolment not found.");

            var ev = _event_db.get_event_by_id(enr.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _result_db.update_result_info(
                id, finish_time, finish_position, total_participants,
                category_position, total_category_participants,
                status.ToString(), notes);

            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }

        /// <summary>Deletes a result (Organiser only).</summary>
        /// <param name="id">Result ID.</param>
        /// <response code="200">Result deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Result not found.</response>
        [HttpDelete("results/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Only organisers can delete results.");

            var existing = _result_db.get_result_by_id(id);
            if (existing == null) return NotFound("Result not found.");

            var enr = _enrolment_db.get_enrolment_by_id(existing.EnrolmentID);
            if (enr == null) return NotFound("Enrolment not found.");

            var ev = _event_db.get_event_by_id(enr.EventID);
            if (ev == null || ev.OrganiserID != user_id.Value)
                return StatusCode(403, "Not your event.");

            string result = _result_db.delete_result(id);
            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }

        /// <summary>Lists all results for a specific event (Public).</summary>
        /// <param name="eventId">Event ID.</param>
        /// <response code="200">Returns list of results.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("events/{eventId}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetByEvent(int eventId)
        {
            var ev = _event_db.get_event_by_id(eventId);
            if (ev == null) return NotFound("Event not found.");
            return Ok(_result_db.get_results_by_event(eventId));
        }
    }
}