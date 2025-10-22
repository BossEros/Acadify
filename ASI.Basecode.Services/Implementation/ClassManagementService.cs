using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Implementation
{
    public class ClassManagementService : IClassManagementService
    {
        private readonly IClassManagementRepository _classManagementRepository;

        public ClassManagementService(IClassManagementRepository classManagementRepository)
        {
            _classManagementRepository = classManagementRepository;
        }

        public async Task<IEnumerable<Class>> GetAllClassesAsync()
        {
            return await _classManagementRepository.GetAllAsync();
        }



        public async Task<Class?> GetClassByIdAsync(int id)
        {
            return await _classManagementRepository.GetByIdAsync(id);
        }
        
        public async Task<int> GetNextClassIdAsync()
        {
            return await _classManagementRepository.GetNextClassIdAsync();
        }


        public async Task CreateClassAsync(Class classEntity)
        {
            classEntity.JoinCode = "TEMP";
            classEntity.JoinCodeGeneratedAt = DateTime.UtcNow;
            await _classManagementRepository.AddAsync(classEntity);

            if (classEntity.Course != null)
            {
                classEntity.JoinCode = $"{classEntity.Course.CourseCode}-{classEntity.Id}";
            }
            else
            {
                classEntity.JoinCode = $"CLASS{classEntity.Id}";
            }

            classEntity.JoinCodeGeneratedAt = DateTime.UtcNow;

            await _classManagementRepository.UpdateAsync(classEntity);
        }


        public async Task UpdateClassAsync(Class classEntity)
        {
            await _classManagementRepository.UpdateAsync(classEntity);
        }

        public async Task DeleteClassAsync(int id)
        {
            await _classManagementRepository.DeleteAsync(id);
        }
    }
}
