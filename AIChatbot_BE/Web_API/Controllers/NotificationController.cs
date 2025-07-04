using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Interface;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotification _notificationService;
        public NotificationController(INotification notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public IActionResult CreateNotification([FromBody] CreateNotificationDTO dto)
        {
            var newNotification = new Notification
            {
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = DateTime.Now,
                IsGlobal = true
            };

            _notificationService.CreateNotification(newNotification);
            return Ok(newNotification);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateNotification(string id, [FromBody] UpdateNotificationDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid update data!");
            var updatedNotification = new Notification
            {
                NotificationId = id,
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };
            _notificationService.UpdateNotification(updatedNotification);
            return Ok("Update Successfully!");
        }

        [HttpDelete]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Notification ID is required!");
            }
            _notificationService.DeleteNotification(id);
            return Ok("Notification deleted successfully!");
        }

        [HttpGet("{id}")]
        public IActionResult GetNotification(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Notification ID is required!");
            }
            var notification = _notificationService.GetNotificationById(id);
            if (notification == null)
            {
                return NotFound("Notification not found!");
            }
            return Ok(notification);
        }
    }
}