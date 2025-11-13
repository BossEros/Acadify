using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.ViewModels.ClassManagement
{
    public class ActivateClassViewModel
    {
        [Required(ErrorMessage = "EDP code is required.")]
        [Display(Name = "EDP Code")]
        [RegularExpression(@"^\d+$", ErrorMessage = "EDP code must be a valid number.")]
        public string EdpCode { get; set; } = string.Empty;
    }
}

