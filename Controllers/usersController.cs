using event_management_system.Database;
using event_management_system.Database.Interfaces;
using event_management_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace event_management_system.Controllers
{
    /// <summary>
    /// Manages the current user's profile: view, update, profile picture, and password change.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    public class usersController : ControllerBase
    {
        /// <summary>Gender options for profile updates.</summary>
        public enum user_genders { Male, Female, Other }

        private readonly Iusers_dbconnect _users_db;
        private readonly Iblob_storage_service _blob_service;

        public usersController(
            Iusers_dbconnect users_db,
            Iblob_storage_service blob_service)
        {
            _users_db = users_db;
            _blob_service = blob_service;
        }

        /// <summary>Gets the current authenticated user's profile.</summary>
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

        /// <summary>Updates the current user's profile information.</summary>
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

        /// <summary>Returns the URL of the current user's profile picture.</summary>
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

        /// <summary>Uploads a profile picture for the current user to Azure Blob Storage.</summary>
        /// <param name="profile_picture">The image file (multipart/form-data).</param>
        [HttpPost("profile-picture")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profile_picture)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null) return Unauthorized("Not authenticated.");

            if (profile_picture == null || profile_picture.Length == 0)
                return BadRequest("No file provided.");

            try
            {
                string url = await _blob_service.upload_image_async(profile_picture, "profiles");
                string result = _users_db.update_profile_picture(user_id.Value, url);

                if (result.Contains("Database Error"))
                    return StatusCode(500, result);

                return Ok(new
                {
                    message = "Profile picture updated successfully.",
                    profile_picture_url = url
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Changes the current user's password.</summary>
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