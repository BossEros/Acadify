using ASI.Basecode.Data.Data;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Data.Models; // Needed for Grade model
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Needed for IEnumerable/List
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Implementation
{
    public class StudentPerformanceService : IStudentPerformanceService
    {
        private readonly AppDbContext _dbContext;

        // Use constructor injection to get the database context
        public StudentPerformanceService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // --- EXISTING METHODS ---

        public async Task<int> GetTotalEnrolledUnitsForSemester(int studentId, short semester, short yearLevel)
        {
            var totalUnits = await _dbContext.Enrollments
                .Include(e => e.Class.Course)
                .Where(e => e.StudentId == studentId &&
                             e.Class.Semester == semester &&
                             e.Class.YearLevel == yearLevel &&
                             e.Class.Course != null)
                .SumAsync(e => e.Class.Course!.Units);

            return totalUnits;
        }

        public async Task<int> GetCompletedSubjectCount(int studentId)
        {
            const string passedRemark = "Passed";

            var completedCount = await _dbContext.Grades
                .Include(g => g.Enrollment)
                .Where(g => g.Enrollment.StudentId == studentId &&
                             g.Remarks == passedRemark)
                .CountAsync();

            return completedCount;
        }

        public async Task<double> GetGpaForSemester(int studentId, short semester, short yearLevel)
        {
            // Get all grades for the student in the specified term.
            var gradesForTerm = await _dbContext.Grades
                .Include(g => g.Enrollment.Class.Course)
                .Where(g => g.Enrollment.StudentId == studentId &&
                             g.Enrollment.Class.Semester == semester &&
                             g.Enrollment.Class.YearLevel == yearLevel &&
                             g.FinalGrade != null) // Only subjects that have a final grade
                .ToListAsync();

            if (gradesForTerm.Count == 0)
            {
                return 0.0;
            }

            // Calculate Total Quality Points and Total Units
            double totalQualityPoints = gradesForTerm
                .Where(g => g.Enrollment.Class.Course != null)
                .Sum(g => (double)g.FinalGrade!.Value * g.Enrollment.Class.Course!.Units);

            int totalUnits = gradesForTerm
                .Where(g => g.Enrollment.Class.Course != null)
                .Sum(g => g.Enrollment.Class.Course!.Units);

            if (totalUnits == 0)
            {
                return 0.0; // Avoid dividing by zero
            }

            double gpa = totalQualityPoints / totalUnits;

            return gpa;
        }

        // --- NEW METHODS FOR SEMESTER REPORT ---

        /// <summary>
        /// Retrieves the list of grades/subjects for a specific semester to display in the report table.
        /// </summary>
        public async Task<IEnumerable<Grade>> GetStudentGradesForSemester(int studentId, short semester, short yearLevel)
        {
            var grades = await _dbContext.Grades
                .Include(g => g.Enrollment.Class.Course)
                .Include(g => g.Enrollment.Class.Teacher) // Optional: Include teacher if needed
                .Where(g => g.Enrollment.StudentId == studentId &&
                             g.Enrollment.Class.Semester == semester &&
                             g.Enrollment.Class.YearLevel == yearLevel)
                .ToListAsync();

            return grades;
        }

        /// <summary>
        /// Counts how many subjects were passed specifically in this semester.
        /// </summary>
        public async Task<int> GetPassedSubjectsCountForSemester(int studentId, short semester, short yearLevel)
        {
            const string passedRemark = "Passed";

            var count = await _dbContext.Grades
                .Include(g => g.Enrollment.Class)
                .Where(g => g.Enrollment.StudentId == studentId &&
                             g.Enrollment.Class.Semester == semester &&
                             g.Enrollment.Class.YearLevel == yearLevel &&
                             g.Remarks == passedRemark)
                .CountAsync();

            return count;
        }
    }
}