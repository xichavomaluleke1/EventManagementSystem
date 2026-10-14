using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using event_management_system.Controllers;
using event_management_system.Database.Interfaces;
using event_management_system.Models;

namespace event_management_system.Tests
{
    /// <summary>
    /// Unit tests for authController — registration and login.
    /// </summary>
    public class AuthControllerTests
    {
        private authController create_controller(Iauth_dbconnect mocked_db, string? role = null, int? user_id = null)
        {
            var controller = new authController(mocked_db);

            var http_context = new DefaultHttpContext();
            http_context.Features.Set<ISessionFeature>(new FakeSessionFeature());
            if (role != null) http_context.Session.SetString("Role", role);
            if (user_id.HasValue) http_context.Session.SetInt32("UserID", user_id.Value);

            controller.ControllerContext = new ControllerContext { HttpContext = http_context };
            return controller;
        }

        // REGISTER tests
        [Fact]
        public void Register_WithNewEmail_Returns201()
        {
            var mock = new Mock<Iauth_dbconnect>();
            mock.Setup(x => x.email_exists(It.IsAny<string>())).Returns(false);
            mock.Setup(x => x.store_user_info(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(),
                It.IsAny<string>(), It.IsAny<string?>()))
                .Returns("User registered successfully.");

            var controller = create_controller(mock.Object);

            var result = controller.Register(
                "test@test.com", "Pass123", "Test", "User",
                authController.roles.Participant, DateTime.Today,
                authController.genders.Male, "0821234567");

            var created = Assert.IsType<CreatedResult>(result);
            Assert.Equal("User registered successfully.", created.Value);
        }

        [Fact]
        public void Register_WithExistingEmail_Returns409()
        {
            var mock = new Mock<Iauth_dbconnect>();
            mock.Setup(x => x.email_exists(It.IsAny<string>())).Returns(true);

            var controller = create_controller(mock.Object);

            var result = controller.Register(
                "existing@test.com", "Pass123", "Test", "User",
                authController.roles.Participant, DateTime.Today,
                authController.genders.Male, null);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflict.StatusCode);
        }

        [Fact]
        public void Register_WhenDatabaseFails_Returns500()
        {
            var mock = new Mock<Iauth_dbconnect>();
            mock.Setup(x => x.email_exists(It.IsAny<string>())).Returns(false);
            mock.Setup(x => x.store_user_info(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(),
                It.IsAny<string>(), It.IsAny<string?>()))
                .Returns("Database Error: something broke");

            var controller = create_controller(mock.Object);

            var result = controller.Register(
                "test@test.com", "Pass123", "Test", "User",
                authController.roles.Participant, DateTime.Today,
                authController.genders.Male, null);

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
        }

        // LOGIN tests
        [Fact]
        public void Login_WithValidCredentials_Returns200()
        {
            string plain = "Test123";
            string hash = BCrypt.Net.BCrypt.HashPassword(plain);

            var user = new Users
            {
                UserID = 1,
                Email = "user@test.com",
                PasswordHash = hash,
                Role = "Participant",
                FirstName = "Test"
            };

            var mock = new Mock<Iauth_dbconnect>();
            mock.Setup(x => x.get_user_by_email("user@test.com")).Returns(user);

            var controller = create_controller(mock.Object);

            var result = controller.Login("user@test.com", plain);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public void Login_WithInvalidPassword_Returns401()
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");

            var user = new Users
            {
                UserID = 1,
                Email = "user@test.com",
                PasswordHash = hash,
                Role = "Participant"
            };

            var mock = new Mock<Iauth_dbconnect>();
            mock.Setup(x => x.get_user_by_email("user@test.com")).Returns(user);

            var controller = create_controller(mock.Object);

            var result = controller.Login("user@test.com", "WrongPassword");

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorized.StatusCode);
        }

        [Fact]
        public void Login_WithNonExistentUser_Returns401()
        {
            var mock = new Mock<Iauth_dbconnect>();
            mock.Setup(x => x.get_user_by_email(It.IsAny<string>())).Returns((Users?)null);

            var controller = create_controller(mock.Object);

            var result = controller.Login("notfound@test.com", "AnyPass");

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorized.StatusCode);
        }
    }
}