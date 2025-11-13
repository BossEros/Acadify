// In ASI.Basecode.Services/Implementation/
using ASI.Basecode.Data.Data; // Your AppDbContext
using ASI.Basecode.Services.Interfaces; // Your new interface
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class StudentPerformanceService : IStudentPerformanceService
{
    private readonly AppDbContext _dbContext;

    // Use constructor injection to get the database context
    public StudentPerformanceService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
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
        //Get all grades for the student in the specified term.
        var gradesForTerm = await _dbContext.Grades
            .Include(g => g.Enrollment.Class.Course)
            .Where(g => g.Enrollment.StudentId == studentId &&
                         g.Enrollment.Class.Semester == semester &&
                         g.Enrollment.Class.YearLevel == yearLevel &&
                         g.FinalGrade != null) //Only subjects that have a final grade
            .ToListAsync();

        if (gradesForTerm.Count == 0)
        {
            return 0.0;
        }

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
}
