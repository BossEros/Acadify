using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.ViewModels.ClassManagement
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; } = string.Empty;
        public List<ClassCard> Classes { get; set; } = new();

        [Required(ErrorMessage = "EDP code is required.")]
        [Display(Name = "EDP Code")]
        [RegularExpression(@"^\d+$", ErrorMessage = "EDP code must be a valid number.")]
        public string EdpCode { get; set; } = string.Empty;

        public class ClassCard
        {
            public int Id { get; set; }
            public string CourseCode { get; set; } = string.Empty;
            public string CourseName { get; set; } = string.Empty;
            public string Schedule { get; set; } = string.Empty;
            public string Room { get; set; } = string.Empty;
            public short Semester { get; set; }
            public short YearLevel { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
