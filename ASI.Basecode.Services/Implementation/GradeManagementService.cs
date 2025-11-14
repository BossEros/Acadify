using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.DTOs;

namespace ASI.Basecode.Services.Implementation
{
    public class GradeManagementService : IGradeManagementService
    {
        private readonly IClassManagementRepository _repo;

        public GradeManagementService(IClassManagementRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<SemesterGradesDto>> GetSemesterGradesForStudentAsync(int studentId)
        {
            var enrollments = await _repo.GetEnrollmentsByStudentIdAsync(studentId);

            var semesterGrades = enrollments
                .GroupBy(e => new { e.Class.YearLevel, e.Class.Semester })
                .OrderBy(g => g.Key.YearLevel)
                .ThenBy(g => g.Key.Semester)
                .Select(g => new SemesterGradesDto
                {
                    YearLevel = (short)g.Key.YearLevel,
                    Semester = (short)g.Key.Semester,
                    Grades = g.Select(e =>
                    {
                        var isPassed = e.Grade?.FinalGrade.HasValue == true || e.Grade?.MidtermGrade.HasValue == true;
                        var remarkText = !string.IsNullOrWhiteSpace(e.Grade?.Remarks)
                            ? e.Grade!.Remarks!
                            : (isPassed ? "Passed" : "Incomplete");

                        return new GradeDto
                        {
                            CourseCode = e.Class.Course!.CourseCode,
                            CourseName = e.Class.Course.CourseName,
                            Units = (int)e.Class.Course.Units,
                            MidtermGrade = e.Grade?.MidtermGrade,
                            FinalGrade = e.Grade?.FinalGrade,
                            Remark = remarkText,
                            IsPassed = isPassed
                        };
                    })
                    .OrderBy(x => x.CourseCode)
                    .ToList()
                })
                .ToList();

            return semesterGrades;
        }
    }
}