namespace ASI.Basecode.Resources.Templates;

public static class EmailTemplates
{
    public static string GetPasswordResetEmailTemplate(string userName, string resetUrl)
    {
        return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>ACADIFY - Password Reset Request</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 0; background-color: #f8fafc;'>
            <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background-color: #f8fafc;'>
                <tr>
                    <td style='padding: 40px 20px;'>
                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='600' style='margin: 0 auto; max-width: 600px; background-color: #ffffff; border-radius: 16px; box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);'>
                            
                            <!-- Header -->
                            <tr>
                                <td style='background-color: #3A4D75; padding: 40px 30px; text-align: center; border-radius: 16px 16px 0 0;'>
                                    <div style='width: 60px; height: 60px; background-color: rgba(255, 255, 255, 0.2); border-radius: 12px; display: inline-flex; align-items: center; justify-content: center; margin-bottom: 20px;'>
                                        <span style='color: #ffffff; font-size: 28px; font-weight: bold; line-height: 60px;'>A</span>
                                    </div>
                                    <h1 style='color: #ffffff; font-size: 32px; font-weight: bold; margin: 0 0 8px 0; letter-spacing: -0.5px;'>ACADIFY</h1>
                                    <p style='color: rgba(255, 255, 255, 0.9); font-size: 16px; margin: 0; font-weight: normal;'>Student Performance Tracker</p>
                                </td>
                            </tr>
                            
                            <!-- Content -->
                            <tr>
                                <td style='padding: 50px 40px;'>
                                    <h2 style='color: #1f2937; font-size: 28px; font-weight: bold; margin: 0 0 30px 0; text-align: center;'>Password Reset Request</h2>
                                    
                                    <p style='color: #4b5563; font-size: 16px; margin-bottom: 20px;'>
                                        Hello <strong style='color: #3A4D75; font-weight: 600;'>{userName}</strong>,
                                    </p>
                                    
                                    <p style='color: #6b7280; font-size: 16px; line-height: 1.7; margin-bottom: 35px;'>
                                        We received a request to reset the password for your ACADIFY account. 
                                        To create a new password, please click the button below. This action will 
                                        allow you to securely access your student performance tracking dashboard.
                                    </p>
                                    
                                    <!-- Reset Button -->
                                    <div style='text-align: center; margin: 40px 0;'>
                                        <a href='{resetUrl}' style='display: inline-block; background-color: #3A4D75; color: #ffffff; text-decoration: none; padding: 16px 32px; border-radius: 12px; font-weight: 600; font-size: 16px; letter-spacing: 0.3px;'>Reset My Password</a>
                                    </div>
                                    
                                    <!-- Warning Box -->
                                    <div style='background-color: #fef3c7; border: 1px solid #f59e0b; border-radius: 12px; padding: 20px; margin: 30px 0;'>
                                        <p style='color: #92400e; font-weight: 600; font-size: 16px; margin: 0 0 12px 0;'>Important Security Information</p>
                                        <ul style='color: #92400e; font-size: 14px; margin: 0; padding-left: 20px;'>
                                            <li style='margin: 8px 0;'>This password reset link will expire in <strong>24 hours</strong></li>
                                            <li style='margin: 8px 0;'>If you did not request this password reset, please ignore this email</li>
                                            <li style='margin: 8px 0;'>For security reasons, never share this link with anyone</li>
                                            <li style='margin: 8px 0;'>Contact our support team if you have any concerns</li>
                                        </ul>
                                    </div>
                                    
                                    <!-- Fallback Link -->
                                    <div style='background-color: #f9fafb; border-radius: 8px; padding: 20px; margin-top: 30px;'>
                                        <p style='color: #6b7280; font-size: 14px; margin: 0 0 10px 0;'>If the button above doesn't work, copy and paste this link into your browser:</p>
                                        <a href='{resetUrl}' style='color: #3A4D75; word-break: break-all; text-decoration: none; font-size: 14px;'>{resetUrl}</a>
                                    </div>
                                    
                                    <div style='color: #6b7280; font-size: 16px; margin-top: 40px;'>
                                        <p>Best regards,<br>
                                        <span style='color: #3A4D75; font-weight: 600;'>The ACADIFY Support Team</span></p>
                                    </div>
                                </td>
                            </tr>
                            
                            <!-- Footer -->
                            <tr>
                                <td style='background-color: #f8fafc; border-top: 1px solid #e5e7eb; padding: 30px; text-align: center; border-radius: 0 0 16px 16px;'>
                                    <p style='color: #9ca3af; font-size: 14px; margin: 0 0 10px 0;'>This is an automated message from ACADIFY. Please do not reply to this email.</p>
                                    <p style='color: #6b7280; font-size: 13px; margin: 0;'>Need help? Contact our support team for assistance with your account.</p>
                                </td>
                            </tr>
                            
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>";
    }
}