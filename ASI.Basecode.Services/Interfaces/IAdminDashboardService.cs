using System.Threading.Tasks;

public interface IAdminDashboardService
{
    Task<int> GetTotalUsers();
    Task<int> GetStudentCount();
    Task<int> GetTeacherCount();
    Task<int> GetAdminCount();

    Task<int> GetTotalClasses();
    Task<int> GetActiveClassCount();
    Task<int> GetInactiveClassCount();
}