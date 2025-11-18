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

        // Helper to calculate the term parameters based on Enrollment date
        private static (short Semester, int SchoolYearStart) GetTermParameters(DateTime enrolledAt)
        {
            var month = enrolledAt.Month;

            // Example Logic: Semester 1 = June to December, Semester 2 = January to May
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
            // 1. Fetch all enrollments for the student from the database
            var enrollments = await _dbContext.Enrollments
                .Include(e => e.Class.Course)
                .Where(e => e.StudentId == studentId && e.Class.Course != null)
                .ToListAsync();

            // 2. Filter in-memory using the date-based logic (calculates term once per record)
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
            // This method remains historical/cumulative and doesn't require date filtering
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
            // 1. Fetch all relevant grades from the database
            var gradesForTerm = await _dbContext.Grades
                .Include(g => g.Enrollment.Class.Course)
                .Where(g => g.Enrollment.StudentId == studentId &&
                             g.FinalGrade != null)
                .ToListAsync();

            // 2. Filter in-memory to only include grades for the specified semester/year
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

        // CRITICAL FIX: Changed method to fetch Enrollments, ensuring all classes show up.
        public async Task<IEnumerable<Enrollment>> GetStudentGradesForSemester(int studentId, int schoolYearStart, short semester)
        {
            // 1. Fetch all ENROLLMENTS for the student from the database
            var enrollments = await _dbContext.Enrollments
                // Eagerly load Class, Course, and Grade to provide all necessary report data
                .Include(e => e.Grade)
                .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            // 2. Filter in-memory to only include enrollments for the specified semester/year
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

            // 1. Fetch all passing grades for the student from the database
            var grades = await _dbContext.Grades
                .Include(g => g.Enrollment.Class)
                .Where(g => g.Enrollment.StudentId == studentId &&
                             g.Remarks == passedRemark)
                .ToListAsync();

            // 2. Count in-memory, filtering by the date-based semester
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