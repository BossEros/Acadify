using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.ViewModels
{
    public class SettingsViewModel
    {
        public UserDetailsViewModel? UserDetails { get; set; }
        public ChangePasswordViewModel? ChangePasswordModel { get; set; }
    }

    public class UserDetailsViewModel
    {
        public string? Name { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}