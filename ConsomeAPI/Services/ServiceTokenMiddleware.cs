using ConsomeAPI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Xml.Linq;

namespace ConsomeAPI.Services
{
    public class ServiceTokenMiddleware : ControllerBase
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ServiceTokenMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Cookies["JWTApplication"] != null)
            {
                var application = context.Request.Cookies["JWTApplication"];
                var model = JsonConvert.DeserializeObject(application);
                JObject responseObject = JObject.Parse(model.ToString());

                var token = responseObject["token"].ToString();
                var refreshToken = responseObject["refreshToken"].ToString();
                var dateExpire = responseObject["date"].ToString();

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:key"]);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                try
                {
                    var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                    if (claimsPrincipal.Identity.IsAuthenticated)
                    {
                        var id = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;
                        var name = claimsPrincipal.FindFirst("name")?.Value;
                        var email = claimsPrincipal.FindFirst(ClaimTypes.Email).Value;
                        var roles = claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => new Claim(ClaimTypes.Role, c.Value));
                        await TokenManager.RolesManager(context, email, refreshToken, dateExpire);

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                            new Claim(ClaimTypes.Name, name.ToString()),
                            new Claim(ClaimTypes.Email, email.ToString()),
                        };

                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                        }

                        var identity = new ClaimsIdentity(claims, "cookie");
                        context.User = new ClaimsPrincipal(identity);
                        context.User.AddIdentity(new ClaimsIdentity(roles, "cookie"));
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error");
                }
            }

            await _next(context);
        }
    }
}

public static class AuthenticationCookieMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationCookieMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ServiceTokenMiddleware>();
    }
}