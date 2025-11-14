using System.Threading.Tasks;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.DTOs;

namespace ASI.Basecode.Services.Implementation
{
    public class TeacherGradeService : ITeacherGradeService
    {
        private readonly IClassManagementRepository _repo;

        public TeacherGradeService(IClassManagementRepository repo)
        {
            _repo = repo;
        }

        public async Task<GradeUpdateResult> UpdateGradeAsync(int enrollmentId, decimal? midtermGrade, decimal? finalGrade, bool midtermProvided, bool finalProvided)
        {
            var grade = await _repo.UpsertGradeAsync(enrollmentId, midtermGrade, finalGrade, midtermProvided, finalProvided);

            if (grade == null)
                return new GradeUpdateResult(false, "Enrollment not found.", null, null, null);

            return new GradeUpdateResult(true, null, grade.Remarks, grade.MidtermGrade, grade.FinalGrade);
        }
    }
}