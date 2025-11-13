using ASI.Basecode.Data.Data;
using ASI.Basecode.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Implementation
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<int> GetTotalUsers()
        {
            return await _context.Users.CountAsync();
        }

        private async Task<int> GetCountByRoleName(string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return 0;

            return await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        }

        public async Task<int> GetStudentCount()
        {
            return await GetCountByRoleName("Student");
        }

        public async Task<int> GetTeacherCount()
        {
            return await GetCountByRoleName("Teacher");
        }

        public async Task<int> GetAdminCount()
        {
            return await GetCountByRoleName("Admin");
        }


        public async Task<int> GetTotalClasses()
        {
            return await _context.Classes.CountAsync();
        }

        public async Task<int> GetActiveClassCount()
        {
            return await _context.Classes.CountAsync(c => c.IsActive);
        }

        public async Task<int> GetInactiveClassCount()
        {
            return await _context.Classes.CountAsync(c => !c.IsActive);
        }
    }
}