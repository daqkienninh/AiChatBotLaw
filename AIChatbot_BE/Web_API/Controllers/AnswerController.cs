using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswerController : Controller
    {
        private readonly IAnswerService _answerService;

        public AnswerController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        [HttpGet]
        public IActionResult GetAnswerbyId(string questionId)
        {
            return Ok(_answerService.GetAnswerById(questionId));
        }
    }
}
