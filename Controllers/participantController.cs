using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Participant dashboard: enrolments, results, and upcoming/past breakdowns.
    /// </summary>
    [Route("api/participant")]
    [ApiController]
    public class participantController : ControllerBase
    {
        private readonly enrolment_dbconnect _enrolment_db;
        private readonly result_dbconnect _result_db;
        private readonly event_dbconnect _event_db;

        public participantController(
            enrolment_dbconnect enrolment_db,
            result_dbconnect result_db,
            event_dbconnect event_db)
        {
            _enrolment_db = enrolment_db;
            _result_db = result_db;
            _event_db = event_db;
        }

        /// <summary>Returns dashboard summary with upcoming and past enrolments.</summary>
        /// <response code="200">Returns summary and enrolment breakdown.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a participant.</response>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Dashboard()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Participant") return StatusCode(403, "Only participants can access this dashboard.");

            var my_enrolments = _enrolment_db.get_enrolments_by_participant(user_id.Value);
            var my_results = _result_db.get_results_by_participant(user_id.Value);

            var upcoming = new List<object>();
            var past = new List<object>();

            foreach (var enr in my_enrolments)
            {
                var ev = _event_db.get_event_by_id(enr.EventID);
                if (ev == null) continue;

                var item = new
                {
                    enrolmentID = enr.EnrolmentID,
                    eventID = ev.EventID,
                    eventName = ev.EventName,
                    eventDate = ev.EventDate,
                    eventType = ev.EventType,
                    location = ev.Location,
                    status = enr.Status,
                    paymentStatus = enr.PaymentStatus
                };

                if (ev.EventDate >= DateTime.Today) upcoming.Add(item);
                else past.Add(item);
            }

            return Ok(new
            {
                total_enrolments = my_enrolments.Count,
                upcoming_count = upcoming.Count,
                past_count = past.Count,
                total_results = my_results.Count,
                upcoming_enrolments = upcoming,
                past_enrolments = past
            });
        }

        /// <summary>Lists all enrolments for the current participant with event details.</summary>
        /// <response code="200">Returns enriched enrolments.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a participant.</response>
        [HttpGet("enrolments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult MyEnrolments()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Participant") return StatusCode(403, "Only participants can access this endpoint.");

            var my_enrolments = _enrolment_db.get_enrolments_by_participant(user_id.Value);
            var enriched = new List<object>();

            foreach (var enr in my_enrolments)
            {
                var ev = _event_db.get_event_by_id(enr.EventID);

                enriched.Add(new
                {
                    enrolmentID = enr.EnrolmentID,
                    participantID = enr.ParticipantID,
                    eventID = enr.EventID,
                    categoryID = enr.CategoryID,
                    enrolmentDate = enr.EnrolmentDate,
                    status = enr.Status,
                    paymentStatus = enr.PaymentStatus,
                    paymentReference = enr.PaymentReference,
                    notes = enr.Notes,
                    eventName = ev?.EventName,
                    eventDate = ev?.EventDate,
                    eventType = ev?.EventType,
                    location = ev?.Location,
                    distance = ev?.Distance
                });
            }

            return Ok(enriched);
        }

        /// <summary>Lists all results for the current participant with event details.</summary>
        /// <response code="200">Returns enriched results.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a participant.</response>
        [HttpGet("results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult MyResults()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (user_id == null) return Unauthorized("Not authenticated.");
            if (role != "Participant") return StatusCode(403, "Only participants can access this endpoint.");

            var my_results = _result_db.get_results_by_participant(user_id.Value);
            var enriched = new List<object>();

            foreach (var res in my_results)
            {
                var enr = _enrolment_db.get_enrolment_by_id(res.EnrolmentID);
                var ev = enr != null ? _event_db.get_event_by_id(enr.EventID) : null;

                enriched.Add(new
                {
                    resultID = res.ResultID,
                    enrolmentID = res.EnrolmentID,
                    finishTime = res.FinishTime,
                    finishPosition = res.FinishPosition,
                    totalParticipants = res.TotalParticipants,
                    categoryPosition = res.CategoryPosition,
                    totalCategoryParticipants = res.TotalCategoryParticipants,
                    status = res.Status,
                    recordedAt = res.RecordedAt,
                    notes = res.Notes,
                    eventID = ev?.EventID,
                    eventName = ev?.EventName,
                    eventDate = ev?.EventDate,
                    eventType = ev?.EventType,
                    location = ev?.Location,
                    distance = ev?.Distance
                });
            }

            return Ok(enriched);
        }
    }
}