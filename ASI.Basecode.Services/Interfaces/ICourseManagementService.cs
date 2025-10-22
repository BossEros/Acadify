namespace ASI.Basecode.Services.Interfaces;

using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public interface ICourseManagementService
{
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<Course?> GetCourseByIdAsync(int id);
}




