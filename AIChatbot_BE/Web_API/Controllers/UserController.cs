using Microsoft.AspNetCore.Mvc;
using Repositories.DBContext;
using Services.Implement;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AichatbotDbContext _context;
        public UserController(AichatbotDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.RegisteredUsers.ToList();
            return Ok(users);
        }
    }
}
