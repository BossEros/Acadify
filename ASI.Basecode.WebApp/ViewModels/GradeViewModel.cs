using System.Collections.Generic;

namespace ASI.Basecode.WebApp.ViewModels
{
    public class GradeViewModel
    {
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int Units { get; set; }
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }

        // New: remark text and a simple flag to decide color in the view
        public string Remark { get; set; } = string.Empty;
        public bool IsPassed { get; set; }
    }

    public class SemesterGradesViewModel
    {
        public short YearLevel { get; set; }
        public short Semester { get; set; }
        public List<GradeViewModel> Grades { get; set; } = new();
    }

    public class SemesterReportViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public short Semester { get; set; }
        public short YearLevel { get; set; }

        public double GPA { get; set; }
        public int TotalUnits { get; set; }
        public int SubjectsPassed { get; set; }
        public int SubjectsFailed { get; set; }
        public int SubjectsIncomplete { get; set; }

        public List<GradeViewModel> Grades { get; set; } = new List<GradeViewModel>();
    }
}