using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Models
{
    public class ProfileViewModel
    {
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? ProfilePictureURL { get; set; }

        // For the file upload
        public IFormFile? ProfilePictureFile { get; set; }
    }
}