using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStudentPerformanceService
{
    Task<int> GetTotalEnrolledUnitsForSemester(int studentId, short semester, short yearLevel);
    Task<int> GetCompletedSubjectCount(int studentId);
    Task<double> GetGpaForSemester(int studentId, short semester, short yearLevel);

    Task<IEnumerable<Grade>> GetStudentGradesForSemester(int studentId, short semester, short yearLevel);
    Task<int> GetPassedSubjectsCountForSemester(int studentId, short semester, short yearLevel);
}