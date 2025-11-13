namespace ASI.Basecode.Services.Interfaces;

using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public interface IClassManagementService
{
    Task<IEnumerable<Class>> GetAllClassesAsync();
    Task<Class?> GetClassByIdAsync(int id);
    Task<Class?> GetClassByIdIncludeInactiveAsync(int id);
    Task<int> GetNextClassIdAsync();
    Task CreateClassAsync(Class classEntity);
    Task UpdateClassAsync(Class classEntity);
    Task<bool> DeleteClassAsync(int id);
    Task<bool> ActivateClassAsync(int classId, int teacherId);
}



