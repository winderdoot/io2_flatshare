using flatshare_server.Infrastructure.Model.Responses;

namespace flatshare_server.Infrastructure.Utils;
public static class EmailGenerator
{
    public static string GeneratePasswordResetEmailHTML(UserDTO user, string code)
    {
        return
        $"<div style=\"font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;\">\r\n" +
        $"    <div style=\"background-color: #4A90E2; padding: 20px; text-align: center; color: white;\">\r\n" +
        $"        <h1 style=\"margin: 0; font-size: 24px;\">Flatshare</h1>\r\n" +
        $"    </div>\r\n" +
        $"    <div style=\"padding: 30px; color: #333; line-height: 1.6; text-align: center;\">\r\n" +
        $"        <h2 style=\"color: #4A90E2;\">Password Reset Code</h2>\r\n" +
        $"        <p style=\"text-align: left;\">Hi {user.FirstName},</p>\r\n" +
        $"        <p style=\"text-align: left;\">Use the following code to reset your password. This code will expire in 24 hours.</p>\r\n" +
        $"        \r\n" +
        $"        <div style=\"margin: 40px 0; padding: 20px; background-color: #f4f7f9; border-radius: 10px; border: 2px dashed #4A90E2;\">\r\n" +
        $"            <span style=\"font-size: 48px; font-weight: bold; letter-spacing: 10px; color: #2C3E50; font-family: 'Courier New', Courier, monospace;\">{code}</span>\r\n" +
        $"        </div>\r\n" +
        $"        \r\n" +
        $"        <p style=\"text-align: left; font-size: 14px; color: #666;\">If you didn't request this change, you can safely ignore this email.</p>\r\n" +
        $"    </div>\r\n" +
        $"    <div style=\"background-color: #f9f9f9; padding: 15px; text-align: center; font-size: 12px; color: #999;\">\r\n" +
        $"        &copy; 2026 Flatshare App. All rights reserved.\r\n" +
        $"    </div>\r\n" +
        $"</div>";
    }
}
