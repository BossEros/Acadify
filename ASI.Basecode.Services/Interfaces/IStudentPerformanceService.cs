using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStudentPerformanceService
{
    Task<int> GetTotalEnrolledUnitsForSemester(int studentId, int schoolYearStart, short semester);
    Task<int> GetCompletedSubjectCount(int studentId);
    Task<double> GetGpaForSemester(int studentId, int schoolYearStart, short semester);
    Task<IEnumerable<Enrollment>> GetStudentGradesForSemester(int studentId, int schoolYearStart, short semester);
    Task<int> GetPassedSubjectsCountForSemester(int studentId, int schoolYearStart, short semester);
}