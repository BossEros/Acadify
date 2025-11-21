namespace ASI.Basecode.Services.Interfaces;

using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public interface IClassManagementService
{
    Task<IEnumerable<ClassIndexDTO>> GetAllClassesAsync();
    Task<ClassDetailsDTO?> GetClassByIdAsync(int id);
    Task<ClassEditDTO> GetClassEditModelAsync(int id);
    Task<Class?> GetClassByIdIncludeInactiveAsync(int id);
    Task<OperationResultDTO> CreateClassAsync(ClassCreateCommandDTO model);
    Task<ClassCreateDTO> GetClassCreateModelAsync();
    Task<OperationResultDTO> UpdateClassAsync(ClassEditCommandDTO model);
    Task<OperationResultDTO> DeleteClassAsync(int id);
    Task<ClassDeleteDTO?> GetClassDeleteModelAsync(int id);
    Task<bool> ActivateClassAsync(int classId, int teacherId);
}



