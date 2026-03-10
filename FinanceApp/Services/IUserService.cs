using FinanceApp.DTOs;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Services
{
    public interface IUserService
    {    
        Task<PaginatedList<User>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<User> GetUserById(int? id);
        Task<User> UpdateUser(UpdateUserDTO update, int? id);
        Task<User> DeleteUser(int? id);
        Task<User> ChangePasswordAsync(ChangePasswordDTO changePasswordDto);
        Task<List<Role>> GetAllRoles();
        Task<User> UpdateProfile(UserProfileDTO profile, int? id);
        Task<User> DeleteProfilePicture(int id);
    }
}
