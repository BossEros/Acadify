namespace ASI.Basecode.Services.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string userName, string resetToken);
    Task SendEmailVerificationAsync(string email, string userName, string verificationToken);
}


