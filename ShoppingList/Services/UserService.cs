using LabWeb.DTOs;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;

namespace LabWeb.Services;

public class UserService : GenericService<User, UserDto>, IUserService
{
    public UserService(IUserRepository repository) : base(repository)
    {
        
    }
}