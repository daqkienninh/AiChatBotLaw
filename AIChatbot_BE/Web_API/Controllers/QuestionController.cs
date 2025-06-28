using Microsoft.AspNetCore.Mvc;
using Repositories;
using Repositories.Models;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly QuestionRepository _repository;

        public QuestionController()
        {
            _repository = new QuestionRepository();
        }

        [HttpGet("{id}")]
        public IActionResult GetQuestionById(string id)
        {
            var question = _repository.GetQuestionById(id);

            if (question == null)
                return NotFound($"Question with ID: {id} is deleted or not existed!");

            var dto = new QuestionDTO
            {
                QuestionId = question.QuestionId,
                UserId = question.UserId,
                QuestionContent = question.QuestionContent,
                QuesCreateAt = question.QuesCreateAt
            };
            return Ok(dto);
        }

        [HttpPost]
        public IActionResult CreateQuestion([FromBody] CreateQuestionDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.QuestionContent))
                return BadRequest("Invalid question data!");

            if (!_repository.UserExists(dto.UserId))
                return BadRequest($"Cannot send question because user with id: {dto.UserId} does not exist!");

            var question = new Question
            {
                UserId = dto.UserId,
                QuestionContent = dto.QuestionContent,
                QuesCreateAt = DateTime.Now
            };
            _repository.CreateQuestion(question);
            return CreatedAtAction(nameof(GetQuestionById), new { id = question.QuestionId }, question);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteQuestion(string id)
        {
            var existingQuestion = _repository.GetQuestionById(id);
            if (existingQuestion == null)
                return NotFound("Question not found!");
            _repository.DeleteQuestion(id);
            return Ok($"Question with ID: {id} deleted!");
        }


    }
}