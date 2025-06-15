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
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            
            return Ok(_registeredUserService.GetAllAccounts());
        }

        [HttpDelete]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }
            
            _registeredUserService.DeleteAccount(id);
            return Ok("User deleted successfully.");
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] UpdateUserDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid update data");

            var updatedUser = new RegisteredUser
            {
                UserId = id,
                UserName = dto.UserName,
                UserEmail = dto.UserEmail,
                Password = dto.Password,
                image = dto.Image ?? null // Nếu Image là null, gán giá trị null
            };

            _registeredUserService.UpdateAccount(updatedUser);
            return Ok("Cập nhật thành công");
        }


        [HttpPost]
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
