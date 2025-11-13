using System.Threading.Tasks;

public interface IClassReportService
{
    Task<double> GetPassingRate(int classId);
    Task<double> GetFailingRate(int classId);
    Task<double> GetClassAverage(int classId);
    Task<double> GetIncompleteRate(int classId);
}