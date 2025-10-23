namespace ASI.Basecode.Data.Repositories;

using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public interface ICourseManagementRepository
{
    Task<IEnumerable<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int? id);
}



