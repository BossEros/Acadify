namespace ASI.Basecode.Services.DTOs;

public class UserManagementDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class UpdateUserRequest
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ChangePasswordRequest
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateUserEmailAndStatusRequest
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class EnrolledClassDto
{
    public string EdpCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
}

public class AssignedClassDto
{
    public string EdpCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
}
