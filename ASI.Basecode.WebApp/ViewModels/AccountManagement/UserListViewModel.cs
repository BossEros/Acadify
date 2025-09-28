using ASI.Basecode.Services.DTOs;

namespace ASI.Basecode.WebApp.ViewModels.AccountManagement
{
    public class UserListViewModel
    {
        public IEnumerable<UserManagementDto> Users { get; set; } = new List<UserManagementDto>();
        public string? Message { get; set; }
        public string? MessageType { get; set; }
    }
}
