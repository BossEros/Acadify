using ASI.Basecode.Data.Data;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ASI.Basecode.Services.Implementation
{
    public class StudentPerformanceService : IStudentPerformanceService
    {
        private readonly AppDbContext _dbContext;


        private static (short Semester, int SchoolYearStart) GetTermParameters(DateTime enrolledAt)
        {
            var month = enrolledAt.Month;


            var semester = (short)((month >= 6 && month <= 12) ? 1 : 2);
            var schoolYearStart = (month >= 6 && month <= 12) ? enrolledAt.Year : enrolledAt.Year - 1;

            return (semester, schoolYearStart);
        }

        public StudentPerformanceService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetTotalEnrolledUnitsForSemester(int studentId, int schoolYearStart, short semester)
        {

            var enrollments = await _dbContext.Enrollments
                .Include(e => e.Class.Course)
                .Where(e => e.StudentId == studentId &&
                            e.Class != null &&
                            e.Class.IsActive &&
                            e.Class.Course != null)
                .ToListAsync();

            var filteredEnrollments = enrollments
                .Where(e =>
                {
                    var term = GetTermParameters(e.EnrolledAt);
                    return term.Semester == semester && term.SchoolYearStart == schoolYearStart;
                });

            var totalUnits = filteredEnrollments.Sum(e => e.Class.Course!.Units);

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

        public async Task<double> GetGpaForSemester(int studentId, int schoolYearStart, short semester)
        {

            var gradesForTerm = await _dbContext.Grades
                .Include(g => g.Enrollment.Class.Course)
                .Where(g => g.Enrollment.StudentId == studentId &&
                             g.FinalGrade != null &&
                             g.Enrollment.Class != null &&
                             g.Enrollment.Class.IsActive)
                .ToListAsync();


            var filteredGrades = gradesForTerm
                .Where(g =>
                {
                    var term = GetTermParameters(g.Enrollment.EnrolledAt);
                    return term.Semester == semester && term.SchoolYearStart == schoolYearStart;
                })
                .ToList();

            if (filteredGrades.Count == 0)
            {
                return 0.0;
            }

            double totalQualityPoints = filteredGrades
                .Where(g => g.Enrollment.Class.Course != null)
                .Sum(g => (double)g.FinalGrade!.Value * g.Enrollment.Class.Course!.Units);

            int totalUnits = filteredGrades
                .Where(g => g.Enrollment.Class.Course != null)
                .Sum(g => g.Enrollment.Class.Course!.Units);

            if (totalUnits == 0)
            {
                return 0.0;
            }

            double gpa = totalQualityPoints / totalUnits;

            return gpa;
        }


        public async Task<IEnumerable<Enrollment>> GetStudentGradesForSemester(int studentId, int schoolYearStart, short semester)
        {

            var enrollments = await _dbContext.Enrollments
                .Include(e => e.Grade)
                .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                .Where(e => e.StudentId == studentId &&
                            e.Class != null &&
                            e.Class.IsActive)
                .ToListAsync();

            var filteredEnrollments = enrollments
                .Where(e =>
                {
                    var term = GetTermParameters(e.EnrolledAt);
                    return term.Semester == semester && term.SchoolYearStart == schoolYearStart;
                })
                .ToList();

            return filteredEnrollments;
        }

        public async Task<int> GetPassedSubjectsCountForSemester(int studentId, int schoolYearStart, short semester)
        {
            const string passedRemark = "Passed";

            var grades = await _dbContext.Grades
                .Include(g => g.Enrollment.Class)
                .Where(g => g.Enrollment.StudentId == studentId &&
                             g.Remarks == passedRemark &&
                             g.Enrollment.Class != null &&
                             g.Enrollment.Class.IsActive)
                .ToListAsync();

            var count = grades
                .Count(g =>
                {
                    var term = GetTermParameters(g.Enrollment.EnrolledAt);
                    return term.Semester == semester && term.SchoolYearStart == schoolYearStart;
                });

            return count;
        }
    }
}