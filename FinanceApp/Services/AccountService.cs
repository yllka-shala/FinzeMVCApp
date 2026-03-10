using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Enums;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace FinanceApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly FinanceAppContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public AccountService(FinanceAppContext context, 
                              IHttpContextAccessor httpContextAccessor, 
                              IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<LoginResponseDTO> Login(LoginDTO loginDto)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(c => c.Email == loginDto.Email);
            if (user == null)
            {
                return new LoginResponseDTO { Message = "Invalid email or password." };
            }

            if(!user.IsActive || !user.IsVerified)
            {
                return new LoginResponseDTO { Message = "InActive User or not verified!" };
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
            if (!isPasswordValid)
            {
                return new LoginResponseDTO { Message = "Invalid email or password." };
            }

            var roleName = await GetRoleName(user.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("ProfileImage", user.ProfilePicture ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, Settings.AuthCookieName);

            await _httpContextAccessor.HttpContext.SignInAsync(Settings.AuthCookieName,
                                                               new ClaimsPrincipal(claimsIdentity),
                                                               new AuthenticationProperties());

            return new LoginResponseDTO
            {
                Message = "Login successful.",
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}"
            };
        }

        public async Task<User> RegisterUser(RegisterUserDTO create)
        {
            var exist = await _context.Users.AnyAsync(c => c.Email.ToLower() == create.Email.ToLower());
            if (exist)
                return null;

            var roleId = await GetRoleId();

            var user = new User
            {
                FirstName = create.FirstName,
                LastName = create.LastName,
                Email = create.Email,
                ProfilePicture = create.ProfilePicture,
                RoleId = roleId,
                IsActive = true,
                Password = BCrypt.Net.BCrypt.HashPassword(create.Password),
                VerificationToken = GenerateVerificationToken(),
                EmailTokenExpires = DateTime.Now.AddHours(24)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await SendVerificationEmail(user);

            return user;
        }

        public async Task<VerificationEmailResponseDTO> VerifyEmail(string token)
        {
            var user = _context.Users.SingleOrDefault(x => x.VerificationToken == token);

            if (user == null)
                return null;

            if (user.EmailTokenExpires < DateTime.Now)
                return null;

            user.Verified = DateTime.Now;
            user.VerificationToken = null;

            await _context.SaveChangesAsync();

            var response = new VerificationEmailResponseDTO
            {
                Message = "Email verified successfuly, you can log in now!"
            };

            return response;
        }

        public async Task<User> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            var user = _context.Users.SingleOrDefault(x => x.Email == forgotPasswordDTO.Email);

            if (user == null)
                return null;

            user.ResetToken = GenerateResetToken();
            user.ResetTokenExpires = DateTime.Now.AddHours(3);

            await _context.SaveChangesAsync();

            await SendPasswordResetEmail(user);

            return user;
        }

        public async Task<User> ResetPassword(ResetPaswordDTO resetPasswordDto)
        {
            var user = GetAccountByResetToken(resetPasswordDto.Token);
            if (user == null)
                return null;

            if (user.IsActive == false)
                return null;

            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            user.PasswordReset = DateTime.Now;
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return user;
        }

        private async Task<int> GetRoleId()
        {
            bool adminExists = await _context.Users.AnyAsync(r => r.Role.Name == RoleType.Admin.ToString());

            string roleName = adminExists ? RoleType.Customer.ToString() : RoleType.Admin.ToString();

            int roleId = await _context.Roles.Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefaultAsync();

            if (roleId == 0)
                return 0;

            return roleId;
        }

        private async Task<string> GetRoleName(int userId)
        {
            var roleName = await _context.Users
                                         .Where(u => u.Id == userId)
                                         .Select(u => u.Role.Name)
                                         .FirstOrDefaultAsync();

            if (roleName == null)
                return null;

            return roleName;
        }

        private string GenerateVerificationToken()
        {
            var token = GenerateRandomToken();

            var tokenIsUnique = !_context.Users.Any(x => x.VerificationToken == token);
            if (!tokenIsUnique)
                return GenerateVerificationToken();

            return token;
        }

        private string GenerateResetToken()
        {
            var token = GenerateRandomToken();

            var tokenIsUnique = !_context.Users.Any(x => x.ResetToken == token);
            if (!tokenIsUnique)
                return GenerateResetToken();

            return token;
        }

        private User GetAccountByResetToken(string token)
        {
            var user = _context.Users.SingleOrDefault(x => x.ResetToken == token && x.ResetTokenExpires > DateTime.Now);
            return user;
        }

        private string GenerateRandomToken()
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            return token;
        }

        private async Task SendVerificationEmail(User user)
        {
            string message = $@"<p>Please use the below token to verify your email address with the <code>/Account/VerifyEmail</code> app route:</p>
                            <p><code>{user.VerificationToken}</code></p>";


            await _emailService.SendEmailAsync(
                    to: user.Email,
                    subject: "Sign-up Verification API - Verify Email",
                    html: $@"<h4>Verify Email</h4>
                            <p>Thanks for registering!</p>
                            {message}"
            );
        }

        private async Task SendPasswordResetEmail(User user)
        {
            string message = $@"<p>Please use the below token to reset your password with the <code>/Account/ResetPassword</code> app route:</p>
                            <p><code>{user.ResetToken}</code></p>";


            await _emailService.SendEmailAsync(
                to: user.Email,
                subject: "Sign-up Verification API - Reset Password",
                html: $@"<h4>Reset Password Email</h4>
                        {message}"
            );
        }
    }
}
