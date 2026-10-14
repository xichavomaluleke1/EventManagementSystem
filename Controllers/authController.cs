using Microsoft.AspNetCore.Mvc;
using event_management_system.Database;
using event_management_system.Models;

namespace event_management_system.Controllers
{//start of namespace

    [Route("api/authenticator")]
    [ApiController]
    public class authController : ControllerBase
    {//start of class

        // creating an enum to hold role options
        public enum roles
        {//start of enum

            Organiser,
            Participant

        }//end of enum

        // creating an enum to hold gender options
        public enum genders
        {//start of enum

            Male,
            Female,
            Other

        }//end of enum

        private readonly auth_dbconnect _auth_db;

        public authController(auth_dbconnect auth_db)
        {
            _auth_db = auth_db;
        }

        //POST /api/auth/register
        [HttpPost("register")]
        public IActionResult Register(
            string email,
            string password,
            string firstname,
            string surname,
            roles role,
            DateTime date_of_birth,
            genders gender,
            string? phone_number)
        {//start of Register()

            // check email duplicate
            if (_auth_db.email_exists(email))
                return Conflict("Email already exists.");

            // hash password
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            // store user (pass enums as strings)
            string result = _auth_db.store_user_info(
                email, hash, firstname, surname, role.ToString(),
                date_of_birth, gender.ToString(), phone_number);

            if (result.Contains("Database Error"))
                return StatusCode(500, result);

            return Created("", result);

        }//end of Register()

        //POST /api/auth/login
        [HttpPost("login")]
        public IActionResult Login(string email, string password)
        {//start of Login()

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

        }//end of Login()

        //POST /api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok("Logged out successfully.");
        }

        //GET /api/auth/current-user
        [HttpGet("current-user")]
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

    }//end of class

}//end of namespace