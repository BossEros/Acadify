using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.ViewModels.ClassManagement
{
    public class CreateClassViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course is required")]
        [Display(Name = "Course")]
        public int? CourseId { get; set; }

        [Display(Name = "Teacher")]
        public int? TeacherId { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [Display(Name = "Semester")]
        public short Semester { get; set; }

        [Required(ErrorMessage = "Year Level is required")]
        [Display(Name = "Year Level")]
        public short YearLevel { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Display(Name = "Capacity")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "At least one day must be selected")]
        [MinLength(1, ErrorMessage = "At least one day must be selected")]
        public string[] Days { get; set; } = Array.Empty<string>();

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public string EndTime { get; set; } = string.Empty;
    }
}
