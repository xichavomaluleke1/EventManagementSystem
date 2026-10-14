using event_management_system.Controllers;
using event_management_system.Database.Interfaces;
using event_management_system.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace event_management_system.Tests
{
    /// <summary>
    /// Unit tests for enrolmentController — participant enrolment and role enforcement.
    /// </summary>
    public class EnrolmentControllerTests
    {
        private enrolmentController create_controller(
            Ienrolment_dbconnect enrolment_db,
            Ievent_dbconnect event_db,
            string? role = null,
            int? user_id = null)
        {
            var controller = new enrolmentController(enrolment_db, event_db);

            var http_context = new DefaultHttpContext();
            http_context.Features.Set<ISessionFeature>(new FakeSessionFeature());
            if (role != null) http_context.Session.SetString("Role", role);
            if (user_id.HasValue) http_context.Session.SetInt32("UserID", user_id.Value);

            controller.ControllerContext = new ControllerContext { HttpContext = http_context };
            return controller;
        }

        [Fact]
        public void Enrol_AsUnauthenticated_Returns401()
        {
            var mock_enr = new Mock<Ienrolment_dbconnect>();
            var mock_ev = new Mock<Ievent_dbconnect>();
            var controller = create_controller(mock_enr.Object, mock_ev.Object, null, null);

            var result = controller.Enrol(1, 1);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorized.StatusCode);
        }

        [Fact]
        public void Enrol_AsOrganiser_Returns403()
        {
            var mock_enr = new Mock<Ienrolment_dbconnect>();
            var mock_ev = new Mock<Ievent_dbconnect>();
            var controller = create_controller(mock_enr.Object, mock_ev.Object, "Organiser", 1);

            var result = controller.Enrol(1, 1);

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }

        [Fact]
        public void Enrol_AsParticipant_CreatesEnrolment()
        {
            var mock_ev = new Mock<Ievent_dbconnect>();
            mock_ev.Setup(x => x.get_event_by_id(1)).Returns(new Events { EventID = 1 });

            var mock_enr = new Mock<Ienrolment_dbconnect>();
            mock_enr.Setup(x => x.store_enrolment_info(1, 1, 5))
                .Returns("Enrolment created successfully.");

            var controller = create_controller(mock_enr.Object, mock_ev.Object, "Participant", 1);

            var result = controller.Enrol(1, 5);

            var created = Assert.IsType<CreatedResult>(result);
            Assert.Equal("Enrolment created successfully.", created.Value);

            // verify the enrolment was recorded in the database
            mock_enr.Verify(x => x.store_enrolment_info(1, 1, 5), Times.Once);
        }

        [Fact]
        public void Enrol_WhenAlreadyEnrolled_Returns400()
        {
            var mock_ev = new Mock<Ievent_dbconnect>();
            mock_ev.Setup(x => x.get_event_by_id(1)).Returns(new Events { EventID = 1 });

            var mock_enr = new Mock<Ienrolment_dbconnect>();
            mock_enr.Setup(x => x.store_enrolment_info(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns("You are already enrolled in this event.");

            var controller = create_controller(mock_enr.Object, mock_ev.Object, "Participant", 1);

            var result = controller.Enrol(1, 1);

            var bad_request = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad_request.StatusCode);
        }

        [Fact]
        public void Enrol_WhenEventNotFound_Returns404()
        {
            var mock_ev = new Mock<Ievent_dbconnect>();
            mock_ev.Setup(x => x.get_event_by_id(99)).Returns((Events?)null);

            var mock_enr = new Mock<Ienrolment_dbconnect>();
            var controller = create_controller(mock_enr.Object, mock_ev.Object, "Participant", 1);

            var result = controller.Enrol(99, 1);

            var not_found = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, not_found.StatusCode);
        }
    }
}