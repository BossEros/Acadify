namespace ASI.Basecode.Data.Repositories;

using System.Threading.Tasks;
using ASI.Basecode.Data.Data;
using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;

public class ClassManagementRepository : IClassManagementRepository
{
    private readonly AppDbContext _dbContext;

    public ClassManagementRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Class>> GetAllAsync()
    {
        return await _dbContext.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .ToListAsync();
    }

    public async Task<Class?> GetByIdAsync(int id)
    {
        return await _dbContext.Classes
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<int> GetNextClassIdAsync()
    {
        var lastId = await _dbContext.Classes
            .OrderByDescending(c => c.Id)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        return lastId + 1;
    }


    public async Task AddAsync(Class classEntity)
    {
        await _dbContext.Classes.AddAsync(classEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Class classEntity)
    {
        _dbContext.Classes.Update(classEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbContext.Classes.FindAsync(id);
        if (entity != null)
        {
            _dbContext.Classes.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}



