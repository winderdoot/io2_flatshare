using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Utils;
using Org.BouncyCastle.Asn1.Cmp;
using flatshare_server.Infrastructure.Services.Emails;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : Controller
{

    private UserService _userService;
    private IEmailService _emailService;

    public AuthController(UserService userService, IEmailService emailService)
    {
        _userService = userService;
        _emailService = emailService;
    }

    [HttpPost("password-reset/request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
    {
        UserDTO? user = await _userService.GetByEmail(request.Email);

        if (user != null)
        {
            string subject = "Reset your password";
            string code = CodeGenerator.GenerateResetCode(10);

            await _userService.CreatePasswordResetEntry(user.Id, code);

            string body = EmailGenerator.GeneratePasswordResetEmailHTML(user, code);
            await _emailService.SendEmailHtmlAsync(user, subject, body);
        }

        return Accepted();
    }

    [HttpPost("password-reset/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request)
    {
        var result = await _userService.ResetPassword(request);

        if (result)
            return Ok();
        else
            return BadRequest();
    }
}

