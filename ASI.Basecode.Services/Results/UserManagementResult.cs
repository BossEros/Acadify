namespace ASI.Basecode.Services.Results;

public class UserManagementResult
{
    public bool Succeeded { get; private set; }
    public IEnumerable<string> Errors { get; private set; } = new List<string>();
    public string? Message { get; private set; }

    private UserManagementResult(bool succeeded, IEnumerable<string> errors, string? message = null)
    {
        Succeeded = succeeded;
        Errors = errors;
        Message = message;
    }

    public static UserManagementResult Success(string? message = null)
    {
        return new UserManagementResult(true, new List<string>(), message);
    }

    public static UserManagementResult Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        var message = errorList.Any() ? string.Join(", ", errorList) : null;
        return new UserManagementResult(false, errorList, message);
    }

    public static UserManagementResult Failure(string error)
    {
        return new UserManagementResult(false, new[] { error }, error);
    }
}
