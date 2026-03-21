using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.User;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace flatshare_server.Infrastructure.Services
{
    public class AuthService
    {
        public const string SessionClaim = "session_id";

        private IUserRepository _repo;
        private JwtOptions _jwtOptions;

        public AuthService
        (
            IUserRepository userRepo,
            IOptions<JwtOptions> jwtOptions
        )
        {
            _repo = userRepo;
            _jwtOptions = jwtOptions.Value;
        }
        private (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user)
        {
            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Secret));

            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            string jti_value = Guid.NewGuid().ToString();
            string session_id = Guid.NewGuid().ToString();
            List<Claim> claims =
            [
                new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti_value),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(SessionClaim, session_id),
                new Claim(ClaimTypes.NameIdentifier, $"{user.FirstName} {user.LastName}"),
            ];

            var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationTimeInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (jwtToken, expires);
        }

        public async Task<string> Authenticate(string email, string password)
        {
            User? user = await _repo.GetByEmail(email);

            if (user is null)
                throw ErrorResponse.Generate(
                    $"Wrong email or password",
                    StatusCodes.Status401Unauthorized
                    );

            if (!PasswordEncoder.Matches(password, user.PassHash, user.Id))
                throw ErrorResponse.Generate(
                    $"Wrong email or password",
                    StatusCodes.Status401Unauthorized
                    );

            throw new NotImplementedException();
        }
    }
}
