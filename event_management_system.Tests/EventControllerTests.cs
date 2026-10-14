using event_management_system.Controllers;
using event_management_system.Database.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using event_management_system.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace event_management_system.Tests
{
    /// <summary>
    /// Unit tests for eventController — role enforcement and ownership.
    /// </summary>
    public class EventControllerTests
    {
        private eventController create_controller(Ievent_dbconnect event_db, string? role = null, int? user_id = null)
        {
            var category_mock = new Mock<Icategory_dbconnect>();
            var controller = new eventController(event_db, category_mock.Object);

            var http_context = new DefaultHttpContext();
            http_context.Features.Set<ISessionFeature>(new FakeSessionFeature());
            if (role != null) http_context.Session.SetString("Role", role);
            if (user_id.HasValue) http_context.Session.SetInt32("UserID", user_id.Value);

            controller.ControllerContext = new ControllerContext { HttpContext = http_context };
            return controller;
        }

        // ============================================================
        // CREATE EVENT
        // ============================================================

        [Fact]
        public void CreateEvent_AsUnauthenticated_Returns401()
        {
            var mock = new Mock<Ievent_dbconnect>();
            var controller = create_controller(mock.Object, null, null);

            var result = controller.Create(
                "Test", "Desc", DateTime.Today, "Cape Town", 10,
                eventController.event_types.Run, 100, 50);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorized.StatusCode);
        }

        [Fact]
        public void CreateEvent_AsParticipant_Returns403()
        {
            var mock = new Mock<Ievent_dbconnect>();
            var controller = create_controller(mock.Object, "Participant", 5);

            var result = controller.Create(
                "Test", "Desc", DateTime.Today, "Cape Town", 10,
                eventController.event_types.Run, 100, 50);

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }

        [Fact]
        public void CreateEvent_AsOrganiser_Returns201()
        {
            var mock = new Mock<Ievent_dbconnect>();
            mock.Setup(x => x.store_event_info(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<decimal?>(),
                It.IsAny<string>()))
                .Returns("Event created successfully.");

            var controller = create_controller(mock.Object, "Organiser", 1);

            var result = controller.Create(
                "Test", "Desc", DateTime.Today, "Cape Town", 10,
                eventController.event_types.Run, 100, 50);

            var created = Assert.IsType<CreatedResult>(result);
            Assert.Equal("Event created successfully.", created.Value);
        }

        // ============================================================
        // DELETE EVENT
        // ============================================================

        [Fact]
        public void DeleteEvent_ByNonOwner_Returns403()
        {
            var existing_event = new Events { EventID = 1, OrganiserID = 99 };
            var mock = new Mock<Ievent_dbconnect>();
            mock.Setup(x => x.get_event_by_id(1)).Returns(existing_event);

            var controller = create_controller(mock.Object, "Organiser", 1);

            var result = controller.Delete(1);

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }

        [Fact]
        public void DeleteEvent_ByOwner_Returns200()
        {
            var existing_event = new Events { EventID = 1, OrganiserID = 1 };
            var mock = new Mock<Ievent_dbconnect>();
            mock.Setup(x => x.get_event_by_id(1)).Returns(existing_event);
            mock.Setup(x => x.delete_event(1)).Returns("Event deleted successfully.");

            var controller = create_controller(mock.Object, "Organiser", 1);

            var result = controller.Delete(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Event deleted successfully.", ok.Value);
        }

        // ============================================================
        // PUBLISH EVENT
        // ============================================================

        [Fact]
        public void PublishEvent_ByNonOwner_Returns403()
        {
            var existing_event = new Events { EventID = 1, OrganiserID = 99 };
            var mock = new Mock<Ievent_dbconnect>();
            mock.Setup(x => x.get_event_by_id(1)).Returns(existing_event);

            var controller = create_controller(mock.Object, "Organiser", 1);

            var result = controller.Publish(1, true);

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }
    }
}