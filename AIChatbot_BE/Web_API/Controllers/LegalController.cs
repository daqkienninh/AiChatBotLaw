using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Repositories.Models;
using Services.Interface;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LegalController : ControllerBase
    {
        private readonly ILegalService _legalService;
        private readonly IWebHostEnvironment _env;

        public LegalController(ILegalService legalService, IWebHostEnvironment env)
        {
            _legalService = legalService;
            _env = env;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Tạo đường dẫn thư mục uploads
            var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir); // Tạo thư mục nếu chưa tồn tại

            // Tạo đường dẫn đầy đủ tới file
            var filePath = Path.Combine(uploadsDir, file.FileName);

            // Ghi file ra disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Gọi xử lý Python
            var result = await _legalService.ProcessPdfAsync(filePath);
            return Ok(result);
        }

        [HttpPost("update/clause")]
        public async Task<IActionResult> UploadClauseTitle(string chapterId, string clauseId, string newTitle)
        {
            var result = await _legalService.UpdateClauseTitleAsync(chapterId, clauseId, newTitle);
            if (result)
            {
                return Ok("Clause title updated successfully.");
            }
            else
            {
                return BadRequest("Failed to update clause title.");
            }
        }

        [HttpGet("chapters")]
        public async Task<IActionResult> GetAllLegalChapters()
        {
            var chapters = await _legalService.GetAllLegalChapter();
            if (chapters == null || chapters.Count == 0)
            {
                return NotFound("No legal chapters found.");
            }
            return Ok(chapters);
        }

        [HttpPost("create/chapter")]
        public async Task<IActionResult> CreateChapter(string newChapterId, string newChapterName)
        {
            if (newChapterId == null || newChapterName == null)
            {
                return BadRequest("Invalid chapter data.");
            }
            var newChapter = new LegalChapter
            {
                Id = newChapterId,
                Title = newChapterName,
                Clauses = new List<LegalClause>() // Khởi tạo danh sách clauses rỗng
            };
            var result = await _legalService.CreateChapterAsync(newChapter);
            if (result)
            {
                return Ok("Chapter created successfully.");
            }
            else
            {
                return BadRequest("Failed to create chapter.");
            }
        }
    }
}
