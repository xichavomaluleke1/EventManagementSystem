using event_management_system.Database;
using event_management_system.Database.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Manages the current user's profile: view, update, profile picture, and password change.
    /// </summary>
    /// <remarks>
    /// All endpoints require an active session. Users can only view or modify their own profile.
    /// </remarks>
    [Route("api/users")]
    [ApiController]
    public class usersController : ControllerBase
    {
        /// <summary>Gender options for profile updates.</summary>
        public enum user_genders { Male, Female, Other }

        private readonly Iusers_dbconnect _users_db;

        public usersController(Iusers_dbconnect users_db)
        {
            _users_db = users_db;
        }

        /// <summary>
        /// Gets the current authenticated user's profile.
        /// </summary>
        /// <response code="200">Profile returned (password hash hidden).</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProfile()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            var user = _users_db.get_user_by_id(user_id.Value);
            if (user == null) return NotFound("User not found.");

            user.PasswordHash = "";
            return Ok(user);
        }

        /// <summary>
        /// Updates the current user's profile information.
        /// </summary>
        /// <param name="firstname">Updated first name.</param>
        /// <param name="surname">Updated last name.</param>
        /// <param name="phone_number">Updated phone number (optional).</param>
        /// <param name="date_of_birth">Updated date of birth (optional).</param>
        /// <param name="gender">Updated gender.</param>
        /// <response code="200">Profile updated.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateProfile(
            string firstname,
            string surname,
            string? phone_number,
            DateTime? date_of_birth,
            user_genders gender)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            string result = _users_db.update_user_profile(
                user_id.Value, firstname, surname, phone_number,
                date_of_birth, gender.ToString());

            if (result.Contains("Database Error")) return StatusCode(500, result);
            if (result == "User not found.") return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Returns the URL of the current user's profile picture.
        /// </summary>
        /// <response code="200">Returns the URL.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">No profile picture set.</response>
        [HttpGet("profile-picture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProfilePicture()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            string? url = _users_db.get_profile_picture(user_id.Value);
            if (string.IsNullOrEmpty(url)) return NotFound("No profile picture set.");

            return Ok(new { profile_picture_url = url });
        }

        /// <summary>
        /// Saves a URL for the current user's profile picture.
        /// </summary>
        /// <param name="image_url">URL of the profile picture.</param>
        /// <response code="200">Profile picture updated.</response>
        /// <response code="400">Invalid URL.</response>
        /// <response code="401">Not authenticated.</response>
        [HttpPost("profile-picture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult UploadProfilePicture(string image_url)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            if (string.IsNullOrWhiteSpace(image_url))
                return BadRequest("Invalid file or URL.");

            string result = _users_db.update_profile_picture(user_id.Value, image_url);
            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        /// <param name="current_password">Current password for verification.</param>
        /// <param name="new_password">New password (hashed before storage).</param>
        /// <response code="200">Password changed.</response>
        /// <response code="400">Current password incorrect.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ChangePassword(string current_password, string new_password)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            var user = _users_db.get_user_by_id(user_id.Value);
            if (user == null) return NotFound("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(current_password, user.PasswordHash))
                return BadRequest("Invalid current password.");

            string new_hash = BCrypt.Net.BCrypt.HashPassword(new_password);
            string result = _users_db.update_password(user_id.Value, new_hash);

            if (result.Contains("Database Error")) return StatusCode(500, result);
            return Ok(result);
        }
    }
}