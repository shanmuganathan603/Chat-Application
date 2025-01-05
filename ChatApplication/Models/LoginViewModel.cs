using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Models
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; }
        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; }
    }
}
