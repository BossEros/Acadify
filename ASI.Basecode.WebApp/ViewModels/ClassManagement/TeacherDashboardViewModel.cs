using System.Collections.Generic;

namespace ASI.Basecode.WebApp.ViewModels.ClassManagement
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; } = string.Empty;
        public List<ClassCard> Classes { get; set; } = new();

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
