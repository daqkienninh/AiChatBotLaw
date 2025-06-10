using Microsoft.AspNetCore.Mvc;
using Repositories.DBContext;
using Services.Implement;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        
        private readonly RegisteredUserService _registeredUserService;
        public UserController(RegisteredUserService registeredUserService)
        {
            _registeredUserService = registeredUserService;
        }
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            
            return Ok(_registeredUserService.GetAllAccounts());
        }

    }
}
