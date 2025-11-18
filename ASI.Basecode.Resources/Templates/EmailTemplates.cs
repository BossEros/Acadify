namespace ASI.Basecode.Resources.Templates;

public static class EmailTemplates
{
    public static string GetPasswordResetEmailTemplate(string userName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>ACADIFY - Password Reset Request</title>
    <!--[if mso]>
    <style>
        * {{ font-family: Arial, sans-serif !important; }}
    </style>
    <![endif]-->
</head>
<body style=""margin: 0; padding: 0; background-color: #f3f4f6; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;"">
    <center style=""width: 100%; table-layout: fixed; background-color: #f3f4f6; padding: 40px 0;"">
        <div style=""max-width: 600px; background-color: #ffffff; margin: 0 auto; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);"">

            <!-- Header -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); padding: 48px 32px; text-align: center;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"">
                                    <div style=""width: 72px; height: 72px; background-color: rgba(255, 255, 255, 0.15); border-radius: 16px; margin: 0 auto 20px; display: inline-block; line-height: 72px;"">
                                        <span style=""color: #ffffff; font-size: 36px; font-weight: 700;"">A</span>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td align=""center"">
                                    <h1 style=""color: #ffffff; font-size: 36px; font-weight: 700; margin: 0 0 8px 0; letter-spacing: -0.5px;"">ACADIFY</h1>
                                    <p style=""color: rgba(255, 255, 255, 0.95); font-size: 15px; margin: 0; font-weight: 400;"">Student Performance Tracker</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <!-- Main Content -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""padding: 48px 32px;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td>
                                    <h2 style=""color: #111827; font-size: 24px; font-weight: 700; margin: 0 0 24px 0; text-align: center;"">Password Reset Request</h2>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">
                                        Hello <strong style=""color: #2563eb; font-weight: 600;"">{userName}</strong>,
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""color: #6b7280; font-size: 15px; line-height: 1.7; margin: 0 0 32px 0;"">
                                        We received a request to reset your ACADIFY account password. Click the button below to create a new password and regain access to your account.
                                    </p>
                                </td>
                            </tr>

                            <!-- CTA Button -->
                            <tr>
                                <td align=""center"" style=""padding: 16px 0 32px 0;"">
                                    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                        <tr>
                                            <td style=""border-radius: 8px; background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);"">
                                                <a href=""{resetUrl}"" target=""_blank"" style=""display: inline-block; padding: 16px 40px; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600; letter-spacing: 0.3px; border-radius: 8px;"">Reset Password</a>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Security Notice -->
                            <tr>
                                <td>
                                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; border-radius: 8px; padding: 20px;"">
                                        <tr>
                                            <td>
                                                <p style=""color: #92400e; font-size: 15px; font-weight: 600; margin: 0 0 12px 0;"">🔒 Security Notice</p>
                                                <p style=""color: #78350f; font-size: 14px; line-height: 1.6; margin: 0 0 8px 0;"">• This link expires in <strong>30 minutes</strong> for your security</p>
                                                <p style=""color: #78350f; font-size: 14px; line-height: 1.6; margin: 0 0 8px 0;"">• Didn't request this? You can safely ignore this email</p>
                                                <p style=""color: #78350f; font-size: 14px; line-height: 1.6; margin: 0;"">• Never share this link with anyone</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Alternative Link -->
                            <tr>
                                <td style=""padding-top: 32px;"">
                                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f9fafb; border-radius: 8px; padding: 16px;"">
                                        <tr>
                                            <td>
                                                <p style=""color: #6b7280; font-size: 13px; margin: 0 0 8px 0;"">Button not working? Copy and paste this link:</p>
                                                <a href=""{resetUrl}"" target=""_blank"" style=""color: #2563eb; word-break: break-all; font-size: 13px; text-decoration: none;"">{resetUrl}</a>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Signature -->
                            <tr>
                                <td style=""padding-top: 32px; border-top: 1px solid #e5e7eb; margin-top: 32px;"">
                                    <p style=""color: #6b7280; font-size: 15px; line-height: 1.6; margin: 0;"">
                                        Best regards,<br>
                                        <span style=""color: #2563eb; font-weight: 600;"">The ACADIFY Team</span>
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <!-- Footer -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""background-color: #f9fafb; padding: 32px; text-align: center; border-top: 1px solid #e5e7eb;"">
                        <p style=""color: #9ca3af; font-size: 13px; line-height: 1.6; margin: 0 0 8px 0;"">
                            This is an automated message. Please do not reply to this email.
                        </p>
                        <p style=""color: #6b7280; font-size: 12px; margin: 0;"">
                            © 2024 ACADIFY. All rights reserved.
                        </p>
                    </td>
                </tr>
            </table>

        </div>
    </center>
</body>
</html>";
    }

    public static string GetEmailVerificationTemplate(string userName, string verificationUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>ACADIFY - Verify Your Email Address</title>
    <!--[if mso]>
    <style>
        * {{ font-family: Arial, sans-serif !important; }}
    </style>
    <![endif]-->
</head>
<body style=""margin: 0; padding: 0; background-color: #f3f4f6; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;"">
    <center style=""width: 100%; table-layout: fixed; background-color: #f3f4f6; padding: 40px 0;"">
        <div style=""max-width: 600px; background-color: #ffffff; margin: 0 auto; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);"">

            <!-- Header -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); padding: 48px 32px; text-align: center;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"">
                                    <div style=""width: 72px; height: 72px; background-color: rgba(255, 255, 255, 0.15); border-radius: 16px; margin: 0 auto 20px; display: inline-block; line-height: 72px;"">
                                        <span style=""color: #ffffff; font-size: 36px; font-weight: 700;"">A</span>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td align=""center"">
                                    <h1 style=""color: #ffffff; font-size: 36px; font-weight: 700; margin: 0 0 8px 0; letter-spacing: -0.5px;"">ACADIFY</h1>
                                    <p style=""color: rgba(255, 255, 255, 0.95); font-size: 15px; margin: 0; font-weight: 400;"">Student Performance Tracker</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <!-- Main Content -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""padding: 48px 32px;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td>
                                    <h2 style=""color: #111827; font-size: 24px; font-weight: 700; margin: 0 0 24px 0; text-align: center;"">Verify Your Email Address</h2>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">
                                        Welcome <strong style=""color: #2563eb; font-weight: 600;"">{userName}</strong>,
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""color: #6b7280; font-size: 15px; line-height: 1.7; margin: 0 0 32px 0;"">
                                        Thank you for registering with ACADIFY! To complete your registration and start using your account, please verify your email address by clicking the button below.
                                    </p>
                                </td>
                            </tr>

                            <!-- CTA Button -->
                            <tr>
                                <td align=""center"" style=""padding: 16px 0 32px 0;"">
                                    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                        <tr>
                                            <td style=""border-radius: 8px; background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);"">
                                                <a href=""{verificationUrl}"" target=""_blank"" style=""display: inline-block; padding: 16px 40px; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600; letter-spacing: 0.3px; border-radius: 8px;"">Verify Email Address</a>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Security Notice -->
                            <tr>
                                <td>
                                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #dbeafe; border-left: 4px solid #2563eb; border-radius: 8px; padding: 20px;"">
                                        <tr>
                                            <td>
                                                <p style=""color: #1e40af; font-size: 15px; font-weight: 600; margin: 0 0 12px 0;"">📧 Verification Details</p>
                                                <p style=""color: #1e3a8a; font-size: 14px; line-height: 1.6; margin: 0 0 8px 0;"">• This link expires in <strong>30 minutes</strong> for your security</p>
                                                <p style=""color: #1e3a8a; font-size: 14px; line-height: 1.6; margin: 0 0 8px 0;"">• Didn't create an account? You can safely ignore this email</p>
                                                <p style=""color: #1e3a8a; font-size: 14px; line-height: 1.6; margin: 0;"">• You must verify your email before logging in</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Alternative Link -->
                            <tr>
                                <td style=""padding-top: 32px;"">
                                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f9fafb; border-radius: 8px; padding: 16px;"">
                                        <tr>
                                            <td>
                                                <p style=""color: #6b7280; font-size: 13px; margin: 0 0 8px 0;"">Button not working? Copy and paste this link:</p>
                                                <a href=""{verificationUrl}"" target=""_blank"" style=""color: #2563eb; word-break: break-all; font-size: 13px; text-decoration: none;"">{verificationUrl}</a>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Signature -->
                            <tr>
                                <td style=""padding-top: 32px; border-top: 1px solid #e5e7eb; margin-top: 32px;"">
                                    <p style=""color: #6b7280; font-size: 15px; line-height: 1.6; margin: 0;"">
                                        Best regards,<br>
                                        <span style=""color: #2563eb; font-weight: 600;"">The ACADIFY Team</span>
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <!-- Footer -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""background-color: #f9fafb; padding: 32px; text-align: center; border-top: 1px solid #e5e7eb;"">
                        <p style=""color: #9ca3af; font-size: 13px; line-height: 1.6; margin: 0 0 8px 0;"">
                            This is an automated message. Please do not reply to this email.
                        </p>
                        <p style=""color: #6b7280; font-size: 12px; margin: 0;"">
                            © 2024 ACADIFY. All rights reserved.
                        </p>
                    </td>
                </tr>
            </table>

        </div>
    </center>
</body>
</html>";
    }

    public static string GetPasswordChangedConfirmationTemplate(string userName, string userEmail)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>ACADIFY - Password Changed Successfully</title>
    <!--[if mso]>
    <style>
        * {{ font-family: Arial, sans-serif !important; }}
    </style>
    <![endif]-->
</head>
<body style=""margin: 0; padding: 0; background-color: #f3f4f6; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;"">
    <center style=""width: 100%; table-layout: fixed; background-color: #f3f4f6; padding: 40px 0;"">
        <div style=""max-width: 600px; background-color: #ffffff; margin: 0 auto; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);"">

            <!-- Header -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); padding: 48px 32px; text-align: center;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"">
                                    <div style=""width: 72px; height: 72px; background-color: rgba(255, 255, 255, 0.15); border-radius: 16px; margin: 0 auto 20px; display: inline-block; line-height: 72px;"">
                                        <span style=""color: #ffffff; font-size: 36px; font-weight: 700;"">A</span>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td align=""center"">
                                    <h1 style=""color: #ffffff; font-size: 36px; font-weight: 700; margin: 0 0 8px 0; letter-spacing: -0.5px;"">ACADIFY</h1>
                                    <p style=""color: rgba(255, 255, 255, 0.95); font-size: 15px; margin: 0; font-weight: 400;"">Student Performance Tracker</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <!-- Main Content -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""padding: 48px 32px;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td>
                                    <h2 style=""color: #111827; font-size: 24px; font-weight: 700; margin: 0 0 24px 0; text-align: center;"">Password Changed Successfully</h2>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""color: #374151; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">
                                        Hello <strong style=""color: #2563eb; font-weight: 600;"">{userName}</strong>,
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style=""color: #6b7280; font-size: 15px; line-height: 1.7; margin: 0 0 24px 0;"">
                                        This email confirms that your ACADIFY account password has been successfully changed.
                                    </p>
                                </td>
                            </tr>

                            <!-- Success Notice -->
                            <tr>
                                <td>
                                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #d1fae5; border-left: 4px solid #10b981; border-radius: 8px; padding: 20px; margin-bottom: 24px;"">
                                        <tr>
                                            <td>
                                                <p style=""color: #065f46; font-size: 15px; font-weight: 600; margin: 0 0 12px 0;"">✓ Password Update Confirmed</p>
                                                <p style=""color: #047857; font-size: 14px; line-height: 1.6; margin: 0 0 8px 0;"">• Account: <strong>{userEmail}</strong></p>
                                                <p style=""color: #047857; font-size: 14px; line-height: 1.6; margin: 0 0 8px 0;"">• Changed on: <strong>{DateTime.UtcNow:MMMM dd, yyyy} at {DateTime.UtcNow:hh:mm tt} UTC</strong></p>
                                                <p style=""color: #047857; font-size: 14px; line-height: 1.6; margin: 0;"">• Status: Active and secure</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Security Notice -->
                            <tr>
                                <td>
                                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #fee2e2; border-left: 4px solid #ef4444; border-radius: 8px; padding: 20px;"">
                                        <tr>
                                            <td>
                                                <p style=""color: #991b1b; font-size: 15px; font-weight: 600; margin: 0 0 12px 0;"">🔒 Didn't Make This Change?</p>
                                                <p style=""color: #7f1d1d; font-size: 14px; line-height: 1.6; margin: 0 0 12px 0;"">
                                                    If you did not change your password, your account may have been compromised. Please take immediate action:
                                                </p>
                                                <p style=""color: #7f1d1d; font-size: 14px; line-height: 1.6; margin: 0 0 8px 0;"">• Contact ACADIFY support immediately</p>
                                                <p style=""color: #7f1d1d; font-size: 14px; line-height: 1.6; margin: 0 0 8px 0;"">• Reset your password using the forgot password feature</p>
                                                <p style=""color: #7f1d1d; font-size: 14px; line-height: 1.6; margin: 0;"">• Review your recent account activity</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Security Tips -->
                            <tr>
                                <td style=""padding-top: 32px;"">
                                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f9fafb; border-radius: 8px; padding: 20px;"">
                                        <tr>
                                            <td>
                                                <p style=""color: #374151; font-size: 14px; font-weight: 600; margin: 0 0 12px 0;"">🛡️ Security Tips</p>
                                                <p style=""color: #6b7280; font-size: 13px; line-height: 1.6; margin: 0 0 6px 0;"">• Never share your password with anyone</p>
                                                <p style=""color: #6b7280; font-size: 13px; line-height: 1.6; margin: 0 0 6px 0;"">• Use a unique password for your ACADIFY account</p>
                                                <p style=""color: #6b7280; font-size: 13px; line-height: 1.6; margin: 0 0 6px 0;"">• Enable two-factor authentication when available</p>
                                                <p style=""color: #6b7280; font-size: 13px; line-height: 1.6; margin: 0;"">• Change your password regularly</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Signature -->
                            <tr>
                                <td style=""padding-top: 32px; border-top: 1px solid #e5e7eb; margin-top: 32px;"">
                                    <p style=""color: #6b7280; font-size: 15px; line-height: 1.6; margin: 0;"">
                                        Best regards,<br>
                                        <span style=""color: #2563eb; font-weight: 600;"">The ACADIFY Team</span>
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <!-- Footer -->
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""background-color: #f9fafb; padding: 32px; text-align: center; border-top: 1px solid #e5e7eb;"">
                        <p style=""color: #9ca3af; font-size: 13px; line-height: 1.6; margin: 0 0 8px 0;"">
                            This is an automated security notification. Please do not reply to this email.
                        </p>
                        <p style=""color: #6b7280; font-size: 12px; margin: 0;"">
                            © 2024 ACADIFY. All rights reserved.
                        </p>
                    </td>
                </tr>
            </table>

        </div>
    </center>
</body>
</html>";
    }
}