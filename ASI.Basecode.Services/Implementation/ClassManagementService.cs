using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Implementation
{
    public class ClassManagementService : IClassManagementService
    {
        private readonly IClassManagementRepository _classManagementRepository;
        private readonly ICourseManagementRepository _courseManagementRepository;

        public ClassManagementService(IClassManagementRepository classManagementRepository, ICourseManagementRepository courseManagementRepository)
        {
            _classManagementRepository = classManagementRepository;
            _courseManagementRepository = courseManagementRepository;
        }

        public async Task<IEnumerable<Class>> GetAllClassesAsync()
        {
            return await _classManagementRepository.GetAllAsync();
        }



        public async Task<Class?> GetClassByIdAsync(int id)
        {
            return await _classManagementRepository.GetByIdAsync(id);
        }

        public async Task<Class?> GetClassByIdIncludeInactiveAsync(int id)
        {
            return await _classManagementRepository.GetByIdIncludeInactiveAsync(id);
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

            Course? course = null;

            if (classEntity.Course != null)
            {
                course = classEntity.Course;
            }
            else if (classEntity.CourseId.HasValue)
            {
                course = await _courseManagementRepository.GetByIdAsync(classEntity.CourseId.Value);
            }

            if (course != null)
            {
                classEntity.JoinCode = $"{course.CourseCode}-{classEntity.Id}";
            }
            else
            {
                classEntity.JoinCode = $"CLASS{classEntity.Id}";
            }

            classEntity.JoinCode = Regex.Replace(classEntity.JoinCode, "[^a-zA-Z0-9]", "");

            classEntity.JoinCodeGeneratedAt = DateTime.UtcNow;
            await _classManagementRepository.UpdateAsync(classEntity); 
        }


        public async Task UpdateClassAsync(Class updatedClass)
        {
            var existingClass = await _classManagementRepository.GetByIdAsync(updatedClass.Id);
            if (existingClass == null)
                throw new Exception("Class not found.");

            existingClass.CourseId = updatedClass.CourseId;
            existingClass.TeacherId = updatedClass.TeacherId;
            existingClass.Semester = updatedClass.Semester;
            existingClass.YearLevel = updatedClass.YearLevel;
            existingClass.Schedule = updatedClass.Schedule;
            existingClass.Room = updatedClass.Room;
            existingClass.IsActive = updatedClass.IsActive;

            existingClass.JoinCode ??= "TEMP"; // just in case, safety line 

            await _classManagementRepository.UpdateAsync(existingClass);
        }


        public async Task<bool> DeleteClassAsync(int id)
        {
            var classEntity = await _classManagementRepository.GetByIdAsync(id);
            if (classEntity == null)
                throw new Exception("Class not found.");

            if (classEntity.IsActive)
                throw new InvalidOperationException("Cannot delete an active class.");

            var hasStudents = await _classManagementRepository.HasEnrolledStudentsAsync(id);
            if (hasStudents)
                throw new InvalidOperationException("Cannot delete a class with enrolled students.");

            await _classManagementRepository.DeleteAsync(classEntity.Id);
            return true;
        }

        public async Task<bool> ActivateClassAsync(int classId, int teacherId)
        {
            var classEntity = await _classManagementRepository.GetByIdIncludeInactiveAsync(classId);
            if (classEntity == null)
                throw new Exception("Class not found with the provided EDP code.");

            // Check if class is already active and assigned to a different teacher
            if (classEntity.IsActive && classEntity.TeacherId.HasValue && classEntity.TeacherId.Value != teacherId)
            {
                throw new InvalidOperationException("This class is already active and assigned to another teacher.");
            }

            // Assign teacher and activate the class
            classEntity.TeacherId = teacherId;
            classEntity.IsActive = true;

            await _classManagementRepository.UpdateAsync(classEntity);
            return true;
        }
    }
}
