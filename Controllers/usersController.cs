using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;

namespace event_management_system.Controllers
{//start of namespace

    [Route("api/users")]
    [ApiController]
    public class usersController : ControllerBase
    {//start of class

        // enum for gender options (unique name to avoid Swagger conflict)
        public enum user_genders
        {//start of enum

            Male,
            Female,
            Other

        }//end of enum

        private readonly users_dbconnect _users_db;

        public usersController(users_dbconnect users_db)
        {
            _users_db = users_db;
        }

        // ==================================================
        // GET /api/users/profile
        // ==================================================
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            var user = _users_db.get_user_by_id(user_id.Value);
            if (user == null)
                return NotFound("User not found.");

            user.PasswordHash = "";
            return Ok(user);
        }

        // ==================================================
        // PUT /api/users/profile
        // ==================================================
        [HttpPut("profile")]
        public IActionResult UpdateProfile(
            string firstname,
            string surname,
            string? phone_number,
            DateTime? date_of_birth,
            user_genders gender)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            string result = _users_db.update_user_profile(
                user_id.Value,
                firstname,
                surname,
                phone_number,
                date_of_birth,
                gender.ToString());

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            if (result == "User not found.")
                return NotFound(result);

            return Ok(result);
        }

        // ==================================================
        // GET /api/users/profile-picture
        // ==================================================
        [HttpGet("profile-picture")]
        public IActionResult GetProfilePicture()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            string? url = _users_db.get_profile_picture(user_id.Value);

            if (string.IsNullOrEmpty(url))
                return NotFound("No profile picture set.");

            return Ok(new { profile_picture_url = url });
        }

        // ==================================================
        // POST /api/users/profile-picture
        // ==================================================
        [HttpPost("profile-picture")]
        public IActionResult UploadProfilePicture(string image_url)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            if (string.IsNullOrWhiteSpace(image_url))
                return BadRequest("Invalid file or URL.");

            string result = _users_db.update_profile_picture(user_id.Value, image_url);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);
        }

        // ==================================================
        // PUT /api/users/change-password
        // ==================================================
        [HttpPut("change-password")]
        public IActionResult ChangePassword(string current_password, string new_password)
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");
            if (user_id == null)
                return Unauthorized("Not authenticated.");

            var user = _users_db.get_user_by_id(user_id.Value);
            if (user == null)
                return NotFound("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(current_password, user.PasswordHash))
                return BadRequest("Invalid current password.");

            string new_hash = BCrypt.Net.BCrypt.HashPassword(new_password);
            string result = _users_db.update_password(user_id.Value, new_hash);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Ok(result);
        }

    }//end of class

}//end of namespace