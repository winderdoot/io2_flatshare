using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Repositories;

namespace flatshare_server.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddAppOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var emailOptions = configuration
            .GetSection(EmailOptions.OptionsKey)
            .Get<EmailOptions>();

        if (emailOptions == null)
        {
            throw new InvalidOperationException("Email configuration is missing from user secrets.");
        }

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.OptionsKey));

        var jwtOptions = configuration
            .GetSection(JwtOptions.OptionsKey)
            .Get<JwtOptions>();

        if (jwtOptions == null || string.IsNullOrEmpty(jwtOptions.Secret))
        {
            throw new InvalidOperationException("JWT configuration is missing from appsettings.json.");
        }

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.OptionsKey));

        return services;
    }

    public static IServiceCollection AddAppJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        /* Disable the silly microslop claim mapping globally */
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        /* Pull JWT Options out of config */
        var jwtOptions = configuration
            .GetSection(JwtOptions.OptionsKey)
            .Get<JwtOptions>();

        if (jwtOptions == null || string.IsNullOrEmpty(jwtOptions.Secret))
        {
            throw new InvalidOperationException("JWT configuration is missing from appsettings.json.");
        }

        /* Configure Authentication */
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                /* Disable it again :) */
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)
                    ),
                    /* JWT uses claim type "role" (AuthService.RoleClaim), not ClaimTypes.Role */
                    RoleClaimType = AuthService.RoleClaim,
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var sessionIdClaim = context.Principal?.FindFirst(AuthService.SessionClaim)?.Value;

                        if (string.IsNullOrEmpty(sessionIdClaim) || !Guid.TryParse(sessionIdClaim, out var sessionId))
                        {
                            context.Fail("Unauthorized: Session claim is missing.");
                            return;
                        }

                        var sessionRepo = context.HttpContext.RequestServices.GetRequiredService<ISessionRepository>();

                        var valid = await sessionRepo.IsSessionValid(sessionId);
                        if (!valid)
                        {
                            context.Fail("Unauthorized: Session is invalid or has been revoked.");
                        }
                    }
                };
            });

        return services;
    }
}
