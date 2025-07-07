using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace be_lecas.Common
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

            public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = ExtractTokenFromHeader(context.Request.Headers["Authorization"].FirstOrDefault());

            if (!string.IsNullOrEmpty(token))
            {
                var jwtHelper = context.RequestServices.GetRequiredService<JwtHelper>();
                var principal = jwtHelper.ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                }
            }

            await _next(context);
        }

        private string? ExtractTokenFromHeader(string? authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
                return null;

            // Remove "Bearer " prefix if present
            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader.Substring("Bearer ".Length);
            }

            // If no "Bearer " prefix, assume the entire string is the token
            return authorizationHeader;
        }
    }
} 