using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace flatshare_server.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection InjectStubCIConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(options =>
        {
            options.AppName = "Flatshare";
            options.Host = "smtp.gmail.com";
            options.Port = 587;
            options.EmailAddress = "flatshare.app@gmail.com";
            options.AppPassword = "STUB_EMAIL_PASSWORD_IGNORE";
        });

        services.Configure<StripeOptions>(options =>
        {
            options.SecretKey = "sk_test_STUB_SECRET_KEY_IGNORE";
            options.PublishableKey = "pk_test_STUB_PUBLISHABLE_KEY_IGNORE";
            options.WebhookSecretInitKey = "whsec_STUB_WEBHOOK_SECRET_IGNORE";
        });

        return services;
    }
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

        var stripeOptions = configuration
            .GetSection(StripeOptions.OptionsKey)
            .Get<StripeOptions>();

        if (stripeOptions is null)
        {
            throw new InvalidOperationException("Stripe configuration is missing from secrets.");
        }

        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.OptionsKey));

        return services;
    }

    public static IServiceCollection AddStripeClient(this IServiceCollection services, IConfiguration configuration)
    {
        var stripeOptions = configuration
            .GetSection(StripeOptions.OptionsKey)
            .Get<StripeOptions>();

        if (stripeOptions is null)
        {
            throw new InvalidOperationException("Stripe configuration is missing from secrets.");
        }

        services.AddSingleton<IStripeClient>(new StripeClient(stripeOptions.SecretKey));

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
                    RoleClaimType = AuthService.RoleClaim, /* Ensure that role claims aren't remapped by microslop :) */ 
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)
                    ),
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
