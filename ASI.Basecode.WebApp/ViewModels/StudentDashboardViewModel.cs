using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = "";
        public List<ClassCard> Classes { get; set; } = new();

        [Required(ErrorMessage = "EDP code is required.")]
        [Display(Name = "EDP Code")]
        [RegularExpression(@"^\d+$", ErrorMessage = "EDP code must be a valid number.")]
        public string EdpCode { get; set; } = string.Empty;

        public class ClassCard
        {
            public int Id { get; set; }
            public string CourseCode { get; set; } = "";
            public string CourseName { get; set; } = "";
            public int Units { get; set; }
            public string TeacherName { get; set; } = "";
            public string Schedule { get; set; } = "";
            public string Room { get; set; } = "";
        }
    }
}
