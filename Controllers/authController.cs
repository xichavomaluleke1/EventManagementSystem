using event_management_system.Database;
using event_management_system.Database.Interfaces;
using event_management_system.Models;
using Microsoft.AspNetCore.Mvc;

namespace event_management_system.Controllers
{
    [Route("api/authenticator")]
    [ApiController]
    public class authController : ControllerBase
    {
        public enum roles { Organiser, Participant }
        public enum genders { Male, Female, Other }

        private readonly Iauth_dbconnect _auth_db;

        public authController(Iauth_dbconnect auth_db)
        {
            _auth_db = auth_db;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Register(
            string email,
            string password,
            string firstname,
            string surname,
            roles role,
            DateTime date_of_birth,
            genders gender,
            string? phone_number)
        {
            if (_auth_db.email_exists(email))
                return Conflict("Email already exists.");

            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            string result = _auth_db.store_user_info(
                email, hash, firstname, surname, role.ToString(),
                date_of_birth, gender.ToString(), phone_number);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Created("", result);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login(string email, string password)
        {
            var user = _auth_db.get_user_by_email(email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);

            _auth_db.update_last_login(user.UserID);

            return Ok(new
            {
                message = "Login successful.",
                userID = user.UserID,
                role = user.Role,
                firstName = user.FirstName,
                profilePictureURL = user.ProfilePictureURL ?? "" // 🆕 Added
            });
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok("Logged out successfully.");
        }

        [HttpGet("current-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CurrentUser()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            var user = _auth_db.get_user_by_id(user_id.Value);
            if (user == null) return Unauthorized("Not authenticated.");

            return Ok(new
            {
                userID = user.UserID,
                email = user.Email,
                role = user.Role,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }
    }
}