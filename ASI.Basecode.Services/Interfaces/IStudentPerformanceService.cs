using System.Threading.Tasks;

public interface IStudentPerformanceService
{
    Task<int> GetTotalEnrolledUnitsForSemester(int studentId, short semester, short yearLevel);
    Task<int> GetCompletedSubjectCount(int studentId);
    Task<double> GetGpaForSemester(int studentId, short semester, short yearLevel);
}