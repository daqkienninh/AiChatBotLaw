using Microsoft.AspNetCore.Mvc;
using Repositories.DBContext;
using Repositories.Models;
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
        [HttpGet("getall")]
        public IActionResult GetAllUsers()
        {
            
            return Ok(_registeredUserService.GetAllAccounts());
        }

        [HttpDelete("delete")]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }
            
            _registeredUserService.DeleteAccount(id);
            return Ok("User deleted successfully.");
        }

        [HttpPut("update")]
        public IActionResult Update(string id, string newName, string newEmail, string newPassword)
        {
            
            _registeredUserService.UpdateAccount(id, newName, newEmail, newPassword);
            return Ok("User updated successfully.");
        }

        [HttpPut("updatename")]
        public IActionResult Update(string id, string newName)
        {
            _registeredUserService.UpdateAccountName(id, newName);
            return Ok("User updated successfully.");
        }

        [HttpPut("updateemail")]
        public IActionResult UpdateEmail(string id, string newEmail)
        {
            _registeredUserService.UpdateAccountEmail(id, newEmail);
            return Ok("User email updated successfully.");
        }
        [HttpPut("updatepassword")]
        public IActionResult UpdatePassword(string id, string newPassword)
        {
            _registeredUserService.UpdateAccountPassword(id, newPassword);
            return Ok("User password updated successfully.");
        }

        [HttpPost("create")]
        public IActionResult Create(RegisteredUser account)
        {
            if (account == null)
            {
                return BadRequest("Account data is required.");
            }
            
            _registeredUserService.CreateAccount(account);
            return Ok("User created successfully.");
        }
    }
}
