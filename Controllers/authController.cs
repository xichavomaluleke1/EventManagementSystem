using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;
using event_management_system.Models;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Handles user authentication: register, login, logout, and current-user lookup.
    /// </summary>
    /// <remarks>
    /// Passwords are hashed with BCrypt before storage. Sessions persist the
    /// user ID and role, which are checked by every other controller for
    /// role-based access control.
    /// </remarks>
    [Route("api/authenticator")]
    [ApiController]
    public class authController : ControllerBase
    {
        /// <summary>Role options for registration.</summary>
        public enum roles { Organiser, Participant }

        /// <summary>Gender options for registration.</summary>
        public enum genders { Male, Female, Other }

        private readonly auth_dbconnect _auth_db;

        public authController(auth_dbconnect auth_db)
        {
            _auth_db = auth_db;
        }

        /// <summary>
        /// Registers a new user account (Organiser or Participant).
        /// </summary>
        /// <remarks>
        /// Checks for duplicate email, hashes the password with BCrypt, and
        /// stores the new user in the users table.
        /// </remarks>
        /// <param name="email">Unique email address.</param>
        /// <param name="password">Plain text password (hashed before storage).</param>
        /// <param name="firstname">User's first name.</param>
        /// <param name="surname">User's last name.</param>
        /// <param name="role">Organiser or Participant.</param>
        /// <param name="date_of_birth">Date of birth (yyyy-MM-dd).</param>
        /// <param name="gender">Male, Female, or Other.</param>
        /// <param name="phone_number">Optional phone number.</param>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="409">Email already exists.</response>
        /// <response code="500">Database error.</response>
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

        /// <summary>
        /// Logs in an existing user and starts a session.
        /// </summary>
        /// <param name="email">Registered email address.</param>
        /// <param name="password">Plain text password.</param>
        /// <response code="200">Login successful; session created.</response>
        /// <response code="401">Invalid credentials.</response>
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
                firstName = user.FirstName
            });
        }

        /// <summary>
        /// Ends the current user's session (logout).
        /// </summary>
        /// <response code="200">Logged out successfully.</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok("Logged out successfully.");
        }

        /// <summary>
        /// Returns details of the currently authenticated user.
        /// </summary>
        /// <response code="200">Returns user details.</response>
        /// <response code="401">Not authenticated.</response>
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