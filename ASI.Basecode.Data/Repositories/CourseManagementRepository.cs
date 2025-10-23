namespace ASI.Basecode.Data.Repositories;

using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ASI.Basecode.Data.Data;

public class CourseManagementRepository : ICourseManagementRepository
{
    private readonly AppDbContext _dbContext;

    public CourseManagementRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _dbContext.Courses.ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int? id)
    {
        return await _dbContext.Courses.FindAsync(id);
    }
}



