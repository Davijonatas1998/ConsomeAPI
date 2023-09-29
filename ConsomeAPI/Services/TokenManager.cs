using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Http;
using Xunit;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace ConsomeAPI.Services
{
    public static class TokenManager
    {
        [Test]
        public static async Task RolesManager(HttpContext context, string email, string refreshToken, string dateExpire)
        {
            string ApiBaseUrl = "https://localhost:7254/api/Account/RoleManager?" + $"email={email}&refreshToken={refreshToken}&date={dateExpire}";

            var client = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl);
            var response = await client.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                CookieManager(response, context);
            }
        }

        [Test]
        public static async Task RefreshToken(HttpContext context, string token, string refreshToken)
        {
            string ApiBaseUrl = "https://localhost:7254/api/Account/RefreshToken?" + $"Token={token}&RefreshToken={refreshToken}";

            var client = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl);
            var response = await client.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                CookieManager(response, context);
            }
        }

        [Fact]
        public static void CookieManager(HttpResponseMessage response, HttpContext context)
        {
            var result = response.Content.ReadAsStringAsync().Result;
            context.Response.Cookies.Append("JWTApplication", result, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(2),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
