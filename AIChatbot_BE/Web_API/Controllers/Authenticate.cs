using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;
using Repositories.DBContext;
using Repositories.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Services.Implement;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Authenticate : ControllerBase
    {
        private readonly AichatbotDbContext _context;
        private readonly RegisteredUserService _registeredUserService;

        public Authenticate(AichatbotDbContext context, RegisteredUserService registeredServices)
        {
            _context = context;
            _registeredUserService = registeredServices;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.RegisteredUsers
                .FirstOrDefault(u => u.UserEmail == request.Email);

            if (user == null)
            {
                return NotFound("Tài khoản không tồn tại.");
            }

            if (user.UserStatus == "Banned")
            {
                return Unauthorized("Tài khoản đã bị khóa.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Mật khẩu không đúng.");
            }

            // Nếu muốn trả về thông tin cơ bản (không bao gồm password)
            var token = GenerateToken(user); // Hàm tạo token, cần triển khai
            return Ok(new { token }); // Trả về token trong phản hồi
        }

        private string GenerateToken(RegisteredUser user)
        {
            // Triển khai logic tạo token (ví dụ: sử dụng thư viện như System.IdentityModel.Tokens.Jwt)
            // Ví dụ cơ bản:
            var key = "this_is_a_very_secure_key_1234567890"; // Thay bằng key bảo mật của bạn
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.UserEmail)
        }),
                Expires = DateTime.UtcNow.AddHours(1), // Token hết hạn sau 1 giờ
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("register")]
        public IActionResult RegisterAsync([FromBody] RegisterRequest request)
        {

            try
            {
                 _registeredUserService.Register(request.Email, request.Password);
                return Ok(new
                {
                    Message = "Đăng ký thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
