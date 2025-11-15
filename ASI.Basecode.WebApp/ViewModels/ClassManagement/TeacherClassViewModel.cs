using System.Collections.Generic;

namespace ASI.Basecode.WebApp.ViewModels.ClassManagement
{
    public class TeacherClassViewModel
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public int Units { get; set; }
        public List<StudentGradeItem> Students { get; set; } = new();

        public class StudentGradeItem
        {
            public string StudentName { get; set; } = string.Empty;
            public double MidtermGrade { get; set; }
            public double FinalGrade { get; set; }
            public string Remarks { get; set; } = string.Empty;
        }
    }
}
