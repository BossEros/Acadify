namespace ASI.Basecode.Data.Repositories;

using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public interface IClassManagementRepository
{
    Task<IEnumerable<Class>> GetAllAsync();
    Task<Class?> GetByIdAsync(int id);
    Task<int> GetNextClassIdAsync();
    Task AddAsync(Class classEntity);
    Task UpdateAsync(Class classEntity);
    Task<bool> HasEnrolledStudentsAsync(int classId);
    Task DeleteAsync(int id);
}



