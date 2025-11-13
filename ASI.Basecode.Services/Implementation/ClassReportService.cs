using ASI.Basecode.Data.Data; 
using ASI.Basecode.Services.Interfaces; 
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class ClassReportService : IClassReportService
{
    private readonly AppDbContext _dbContext;

    public ClassReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<double> GetPassingRate(int classId)
    {
        // Gets all grades for the class
        var gradesForClass = await _dbContext.Grades
            .Include(g => g.Enrollment)
            .Where(g => g.Enrollment.ClassId == classId)
            .ToListAsync();

        if (gradesForClass.Count == 0)
        {
            return 0.0;
        }

        int totalStudents = gradesForClass.Count;
        const string passedRemark = "Passed";
        int passingCount = gradesForClass.Count(g => g.Remarks == passedRemark);
        
        double passingRate = (double)passingCount / totalStudents * 100.0;
        return passingRate;
    }
    public async Task<double> GetFailingRate(int classId)
    {
        var gradesForClass = await _dbContext.Grades
            .Include(g => g.Enrollment)
            .Where(g => g.Enrollment.ClassId == classId)
            .ToListAsync();

        if (gradesForClass.Count == 0)
        {
            return 0.0;
        }

        int totalStudents = gradesForClass.Count;
        const string failedRemark = "Failed";
        int failingCount = gradesForClass.Count(g => g.Remarks == failedRemark);
        
        double failingRate = (double)failingCount / totalStudents * 100.0;
        return failingRate;
    }
    public async Task<double> GetClassAverage(int classId)
    {
        var gradesWithFinal = await _dbContext.Grades
            .Include(g => g.Enrollment)
            .Where(g => g.Enrollment.ClassId == classId &&
                         g.FinalGrade != null)
            .ToListAsync();

        if (gradesWithFinal.Count == 0)
        {
            return 0.0;
        }

        double classAverage = (double)gradesWithFinal
            .Where(g => g.FinalGrade.HasValue)
            .Average(g => g.FinalGrade!.Value);
        return classAverage;
    }

    public async Task<double> GetIncompleteRate(int classId)
    {
        //Gets all grades for the class
        var gradesForClass = await _dbContext.Grades
            .Include(g => g.Enrollment)
            .Where(g => g.Enrollment.ClassId == classId)
            .ToListAsync();

        if (gradesForClass.Count == 0)
        {
            return 0.0;
        }

        int totalStudents = gradesForClass.Count;
        const string incompleteRemark = "Incomplete";
        int incompleteCount = gradesForClass.Count(g => g.Remarks == incompleteRemark);
        
        double incompleteRate = (double)incompleteCount / totalStudents * 100.0;
        return incompleteRate;
    }
}