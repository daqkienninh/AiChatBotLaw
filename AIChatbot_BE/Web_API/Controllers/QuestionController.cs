using Microsoft.AspNetCore.Mvc;
using Repositories;
using Repositories.Models;
using Services.Implement;
using Services.Interface;
using System.Text.Json;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestion _questionService;
        private readonly IEmbeddingService _embeddingService;

        public QuestionController(IQuestion questionService, IEmbeddingService embeddingService)
        {
            _questionService = questionService;
            _embeddingService = embeddingService; // Assuming you have an EmbeddingService implementation
        }

        [HttpGet("{id}")]
        public IActionResult GetQuestionById(string id)
        {
            var question = _questionService.GetQuestionById(id);

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
            var embedding = _embeddingService.GenerateEmbeddingAsync(dto.QuestionContent);

            if (dto == null || string.IsNullOrWhiteSpace(dto.QuestionContent))
                return BadRequest("Invalid question data!");

            //if (!_questionService.)
            //    return BadRequest($"Cannot send question because user with id: {dto.UserId} does not exist!");

            var question = new Question
            {
                UserId = dto.UserId,
                QuestionContent = dto.QuestionContent,
                QuesCreateAt = DateTime.Now,
                Embedding = JsonSerializer.Serialize(embedding)
            };
            _questionService.CreateQuestion(question);
            return CreatedAtAction(nameof(GetQuestionById), new { id = question.QuestionId }, question);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteQuestion(string id)
        {
            var existingQuestion = _questionService.GetQuestionById(id);
            if (existingQuestion == null)
                return NotFound("Question not found!");
            _questionService.DeleteQuestion(id);
            return Ok($"Question with ID: {id} deleted!");
        }
    }
}