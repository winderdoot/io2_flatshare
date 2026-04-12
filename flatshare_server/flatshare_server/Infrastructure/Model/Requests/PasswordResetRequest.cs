namespace flatshare_server.Infrastructure.Model.Requests;

public record PasswordResetRequest(string Email);

public record ConfirmPasswordResetRequest(string ResetToken, string Email, string NewPassword);
