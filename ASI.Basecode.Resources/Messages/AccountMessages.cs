namespace ASI.Basecode.Resources.Messages;

public static class AccountMessages
{
    // Login Messages
    public const string InvalidLoginAttempt = "Invalid login attempt. Please try again.";
    public const string AccountLockedOut = "This account has been locked due to multiple failed login attempts. Please try again in 15 minutes.";
    public const string AccountNotFound = "No account found with this email address.";
    public const string IncorrectPassword = "Incorrect password. Please try again.";
    public const string IncorrectCredentials = "Incorrect email or password. Please try again.";
    public const string AccountDeactivated = "This account has been deactivated. Please contact support for assistance.";
    public const string AccountPendingApproval = "Your account is pending approval. Please contact an administrator.";
    public const string EmailNotVerified = "Please verify your email address before logging in. Check your inbox for the verification link.";
    
    // Registration Messages
    public const string RegistrationSuccessful = "Registration successful. Please log in.";
    
    // Password Reset Messages
    public const string PasswordResetEmailSent = "If an account with that email exists, a password reset link has been sent to your email address.";
    public const string PasswordResetSuccessful = "Your password has been reset successfully. You can now log in with your new password";
    public const string PasswordResetFailed = "Failed to send password reset email. Please try again later.";
    public const string InvalidPasswordResetRequest = "Invalid password reset request";
    
    // General Error Messages
    public const string UnexpectedError = "An unexpected error occurred. Please try again.";
}
