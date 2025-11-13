namespace ASI.Basecode.WebApp.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = "";
        public List<ClassCard> Classes { get; set; } = new();

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
