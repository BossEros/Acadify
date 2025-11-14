using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Services.DTOs;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IGradeManagementService
    {
        Task<List<SemesterGradesDto>> GetSemesterGradesForStudentAsync(int studentId);
    }
}
