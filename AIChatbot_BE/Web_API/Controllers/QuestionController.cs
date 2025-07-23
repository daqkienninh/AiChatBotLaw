using Microsoft.AspNetCore.Mvc;
using Repositories;
using Repositories.Models;
using Services.Implement;
using Services.Interface;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestion _questionService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IAnswerService _answerService;
        private readonly IChatRoomService _chatRoomService;

        public QuestionController(IQuestion questionService, IEmbeddingService embeddingService, IAnswerService answerService, IChatRoomService chatRoomService)
        {
            _questionService = questionService;
            _embeddingService = embeddingService; // Assuming you have an EmbeddingService implementation
            _answerService = answerService;
            _chatRoomService = chatRoomService;
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
        public async Task<IActionResult> CreateQuestion(string userId, [FromBody] string questionContent)
        {
            if (questionContent == null)
            {
                return BadRequest("Invalid question data!");
            }
            var embedding = _embeddingService.GenerateEmbeddingAsync(questionContent);

            var question = new Question
            {
                UserId = userId,
                QuestionContent = questionContent,
                QuesCreateAt = DateTime.Now,
                Embedding = JsonSerializer.Serialize(embedding)
            };
            _questionService.CreateQuestion(question);
            await _answerService.CreateAnswerFromQuestionAsync(question);

            var chatRoom = _chatRoomService.GetOrCreateChatRoom(userId);

            // 4. Thêm QuestionId vào ChatRoomQuestions
            _chatRoomService.AddQuestionToChatRoom(chatRoom.ChatId, question.QuestionId.ToString());
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

        [HttpGet("daily-history/{userId}")]
        public async Task<IActionResult> GetUserDailyHistory(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var history = await _chatRoomService.GetUserDailyHistoryAsync(userId);

            if (history == null || !history.Any())
            {
                return NotFound("No chat history found for today.");
            }

            return Ok(history);
        }
    }
}