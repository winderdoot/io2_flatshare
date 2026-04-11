using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
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
        public const string RoleClaim = "role";

        public const string TenantRole = "TENANT";
        public const string LandlordRole = "LANDLORD";

        public const string LandlordPolicy = "LANDLORD_ONLY";

        private IUserRepository _userRepo;
        private ISessionRepository _sessionRepo;
        private JwtOptions _jwtOptions;

        public AuthService
        (
            IUserRepository userRepo,
            ISessionRepository sessionRepo,
            IOptions<JwtOptions> jwtOptions
        )
        {
            _userRepo = userRepo;
            _sessionRepo = sessionRepo;
            _jwtOptions = jwtOptions.Value;
        }
        private (string jwtToken, Guid sessionId, int expiresInSec, string role) MakeNewSession(User user)
        {
            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Secret));

            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            string jti_value = Guid.NewGuid().ToString();
            Guid session_id = Guid.NewGuid();

            string role;
            try
            {
                role = user.Role.ToString();
            }
            catch
            {
                role = "EMPTY";
            }

            List<Claim> claims = new()
            {
                new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti_value),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(SessionClaim, session_id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, $"{user.FirstName} {user.LastName}"),
                new Claim(RoleClaim, role)
            };

            var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationTimeInMinutes);
            var expiresIn = _jwtOptions.ExpirationTimeInMinutes * 60;

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (jwtToken, session_id, expiresIn, role);
        }

        public async Task<(string, Guid, int, string)> Authenticate(string email, string password)
        {
            User? user = await _userRepo.GetByEmail(email);

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
            var resp = MakeNewSession(user);

            await _sessionRepo.SaveNew(resp.sessionId, user.Id);
            return resp;
        }

        public async Task<(string, Guid, int, string)> Refresh(Guid sessionId)
        {
            UserSession session = await _sessionRepo.GetBySessionId(sessionId);
            User? user = await _userRepo.GetById(session.UserId);
            var resp = MakeNewSession(user);

            await _sessionRepo.SaveNew(resp.sessionId, user.Id);

            return resp;
        }

        public async Task<Guid> GetUserFromSession(Guid sessionId)
        {
            UserSession session = await _sessionRepo.GetBySessionId(sessionId);
            return session.UserId;
        }

        public Guid GetUserId(ClaimsPrincipal? principal)
        {
            var claim = principal?.FindFirst(JwtRegisteredClaimNames.Sub);
            if (claim is null)
            {
                throw ErrorResponse.Generate("Missing UserId Claim", StatusCodes.Status401Unauthorized);
            }
            Guid.TryParse(claim?.Value, out Guid id);
            return id;
        }

        public void AssertUserIs(ClaimsPrincipal? principal, Guid userId)
        {
            var actualId = GetUserId(principal);
            if (actualId != userId)
            {
                throw ErrorResponse.Generate("Forbidden", StatusCodes.Status403Forbidden);
            }
        }
    }
}
