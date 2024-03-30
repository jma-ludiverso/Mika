using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Extensions;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Threading.Tasks;

namespace MikaWeb.Areas.Identity
{
    public class IdentityTokenClaimService : ITokenClaimsService
    {

        private readonly UserManager<MikaWebUser> _userManager;
        private readonly ApiConfiguration _configuration;

        public IdentityTokenClaimService(UserManager<MikaWebUser> userManager, IOptions<ApiConfiguration> config)
        {
            _userManager = userManager;
            _configuration = config.Value;
        }

        public async Task<string> GetTokenAsync(MikaWebUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.JwtSettings.Key);
            var issuer = _configuration.JwtSettings.Issuer;
            var audience = _configuration.JwtSettings.Audience;

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Sid, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.GivenName, $"{user.Nombre} {user.Apellidos}")
                };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims.ToArray()),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
