using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASI.Basecode.Data.Data;
using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data.Repositories
{
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
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Grade)
                .ToListAsync();
        }

        public async Task<Class?> GetByIdAsync(int id)
        {
            return await _dbContext.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Grade)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Class?> GetByIdIncludeInactiveAsync(int id)
        {
            return await _dbContext.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Grade)
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

        public async Task<bool> HasEnrolledStudentsAsync(int classId)
        {
            return await _dbContext.Enrollments.AnyAsync(e => e.ClassId == classId);
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

        // Enrollment methods
        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await _dbContext.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                .ToListAsync();
        }

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int classId)
        {
            return await _dbContext.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.ClassId == classId);
        }

        public async Task EnrollStudentAsync(int studentId, int classId)
        {
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                ClassId = classId,
                EnrolledAt = DateTime.UtcNow
            };

            await _dbContext.Enrollments.AddAsync(enrollment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnenrollStudentAsync(int studentId, int classId)
        {
            var enrollment = await _dbContext.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.ClassId == classId);

            if (enrollment != null)
            {
                _dbContext.Enrollments.Remove(enrollment);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Teacher assignment methods
        public async Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId)
        {
            return await _dbContext.Classes
                .Where(c => c.TeacherId == teacherId && c.IsActive)
                .Include(c => c.Course)
                .ToListAsync();
        }

        public async Task UnassignTeacherFromClassAsync(int classId)
        {
            var classEntity = await _dbContext.Classes.FindAsync(classId);
            if (classEntity != null)
            {
                classEntity.TeacherId = null;
                classEntity.IsActive = false;
                _dbContext.Classes.Update(classEntity);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
