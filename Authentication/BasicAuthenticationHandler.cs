using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace SkiServiceMongoDB.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

            if (authHeader.Scheme.ToLower() != "bearer")
                return AuthenticateResult.Fail("Invalid Authorization Scheme");

            var token = authHeader.Parameter;

            if (string.IsNullOrEmpty(token))
                return AuthenticateResult.Fail("Missing or empty Bearer token");

            var validationService = new TokenValidationService();
            var principal = validationService.ValidateToken(token);

            if (principal != null)
            {
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("Invalid Bearer token");
            }
        }
    }

    public class TokenValidationService
    {
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                // Token konnte nicht validiert werden
                return null;
            }
        }

        private TokenValidationParameters GetValidationParameters()
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key")),
                ValidateIssuer = true,
                ValidIssuer = "your_issuer",
                ValidateAudience = true,
                ValidAudience = "your_audience",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero 
            };

            return validationParameters;
        }
    }
}
