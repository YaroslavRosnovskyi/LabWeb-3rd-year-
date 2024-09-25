using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LabWeb.Models.IdentityModels;
using LabWeb.Services.Interfaces;
using LabWeb.Services.Interfaces.AzureInterfaces;
using LabWeb.SettingOptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LabWeb.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IBlobStorageService _blobStorageService;

        public TokenService(IOptions<JwtSettings> jwtSettings, IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var imageName = await _blobStorageService.GetBlobUrl(user.ImageName);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Id", user.Id.ToString()),
                new Claim("ImageName", imageName ?? "Default.jpg")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
