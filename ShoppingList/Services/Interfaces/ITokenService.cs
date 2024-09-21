using LabWeb.Models;

namespace LabWeb.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    }
}