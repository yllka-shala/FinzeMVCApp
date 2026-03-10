using FinanceApp.DTOs;
using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IAccountService
    {
        Task<LoginResponseDTO> Login(LoginDTO loginDto);
        Task<User> RegisterUser(RegisterUserDTO create);
        Task<VerificationEmailResponseDTO> VerifyEmail(string token);
        Task<User> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO);
        Task<User> ResetPassword(ResetPaswordDTO resetPasswordDto);
    }
}
