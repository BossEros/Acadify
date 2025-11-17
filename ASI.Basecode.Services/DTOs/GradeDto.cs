using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.DTOs
{
    public class GradeDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Units { get; set; }
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool IsPassed { get; set; }
    }

    public class SemesterGradesDto
    {
        public int SchoolYearStart { get; set; }
        public short Semester { get; set; }
        public List<GradeDto> Grades { get; set; } = new();

        public string SchoolYearDisplay => $"{SchoolYearStart}-{SchoolYearStart + 1}";
    }

    public class GradeUpdateResult
    {
        public GradeUpdateResult(bool success, string? errorMessage, string? remark, decimal? midterm, decimal? final)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Remark = remark;
            Midterm = midterm;
            Final = final;
        }

        public bool Success { get; }
        public string? ErrorMessage { get; }
        public string? Remark { get; }
        public decimal? Midterm { get; }
        public decimal? Final { get; }
    }
}
