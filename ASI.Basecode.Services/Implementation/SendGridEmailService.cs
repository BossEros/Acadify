namespace ASI.Basecode.Services.Implementation;

using ASI.Basecode.Services.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using ASI.Basecode.Resources.Templates;

public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SendGridEmailService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task SendPasswordResetEmailAsync(string email, string userName, string resetToken)
    {
        var apiKey = _configuration["EmailSettings:SendGridApiKey"];
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromName = _configuration["EmailSettings:FromName"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
        {
            throw new InvalidOperationException("SendGrid configuration is missing. Please check EmailSettings in appsettings.json");
        }

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var baseUrl = _configuration["EmailSettings:BaseUrl"] ?? "https://localhost:5001";
        var resetUrl = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";

        var emailData = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = email, name = userName } },
                    subject = "Reset Your ACADIFY Password"
                }
            },
            from = new { email = fromEmail, name = fromName },
            content = new[]
            {
                new
                {
                    type = "text/html",
                    value = EmailTemplates.GetPasswordResetEmailTemplate(userName, resetUrl)
                }
            }
        };

        var json = JsonSerializer.Serialize(emailData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to send email via SendGrid: {response.StatusCode} - {errorContent}");
        }
    }

    public async Task SendEmailVerificationAsync(string email, string userName, string verificationToken)
    {
        var apiKey = _configuration["EmailSettings:SendGridApiKey"];
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromName = _configuration["EmailSettings:FromName"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
        {
            throw new InvalidOperationException("SendGrid configuration is missing. Please check EmailSettings in appsettings.json");
        }

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var baseUrl = _configuration["EmailSettings:BaseUrl"] ?? "https://localhost:5001";
        var verificationUrl = $"{baseUrl}/Account/ConfirmEmail?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(verificationToken)}";

        var emailData = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = email, name = userName } },
                    subject = "Verify Your ACADIFY Email Address"
                }
            },
            from = new { email = fromEmail, name = fromName },
            content = new[]
            {
                new
                {
                    type = "text/html",
                    value = EmailTemplates.GetEmailVerificationTemplate(userName, verificationUrl)
                }
            }
        };

        var json = JsonSerializer.Serialize(emailData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to send email via SendGrid: {response.StatusCode} - {errorContent}");
        }
    }
}


