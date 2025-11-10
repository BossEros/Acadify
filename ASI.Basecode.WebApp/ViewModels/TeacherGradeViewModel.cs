namespace ASI.Basecode.WebApp.ViewModels
{
    public class TeacherGradeViewModel
    {
        public int ClassId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string Schedule { get; set; } = null!;
        public int Units { get; set; }
        public List<StudentGradeViewModel> StudentGrades { get; set; } = new();
    }

    public class StudentGradeViewModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool IsPassed { get; set; }
    }
}