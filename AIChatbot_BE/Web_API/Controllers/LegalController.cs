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

        [HttpPut("upload")]
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

        [HttpPut("article")]
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

        [HttpPut("clause")]
        public async Task<IActionResult> UploadClauseItem(string chapterId, string clauseId, string itemId, string newContent)
        {
            var result = await _legalService.UpdateClauseItemTextAsync(chapterId, clauseId, itemId, newContent);
            if (result)
            {
                return Ok("Clause item updated successfully.");
            }
            else
            {
                return BadRequest("Failed to update clause item.");
            }
        }

        [HttpPut("point")]
        public async Task<IActionResult> UploadPointItem(string chapterId, string articleId, string clauseId, string pointId, string newContent)
        {
            var result = await _legalService.UpdatePointTextAsync(chapterId, clauseId, clauseId, pointId, newContent);
            if (result)
            {
                return Ok("Point item updated successfully.");
            }
            else
            {
                return BadRequest("Failed to update point item.");
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

        [HttpPost("chapter")]
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

        [HttpPost("article")]
        public async Task<IActionResult> AddClauseToChapter(string chapterId, string id, string title)
        {
            var newClause = new LegalClause
            {
                Id = id, // Tạo ID mới cho clause
                Title = title,
                Type = "DIEU",
                ClauseItems = new List<LegalClauseItem>() // Khởi tạo danh sách clause items nếu null
            };
            if (chapterId == null || newClause == null)
            {
                return BadRequest("Invalid chapter or clause data.");
            }
            var result = await _legalService.AddClauseToChapterAsync(chapterId, newClause);
            if (result)
            {
                return Ok("Clause added to chapter successfully.");
            }
            else
            {
                return BadRequest("Failed to add clause to chapter.");
            }
        }

        [HttpPost("clause")]
        public async Task<IActionResult> AddClauseItem(string chapterId, string clauseId, string id, string text)
        {
            var newClauseItem = new LegalClauseItem
            {
                Id = id, // Tạo ID mới cho clause item
                Text = text,
                Embedding = null, // Khởi tạo Embedding nếu cần
                Points = new List<LegalPoint>() // Khởi tạo danh sách points rỗng
            };
            if (chapterId == null || clauseId == null || newClauseItem == null)
            {
                return BadRequest("Invalid chapter or clause item data.");
            }
            var result = await _legalService.AddClauseItemAsync(chapterId, clauseId, newClauseItem);
            if (result)
            {
                return Ok("Clause item added successfully.");
            }
            else
            {
                return BadRequest("Failed to add clause item.");
            }
        }
        [HttpPost("point")]
        public async Task<IActionResult> AddPoint(string chapterId, string clauseId, string clauseItemId, string id, string text)
        {
            var newPoint = new LegalPoint
            {
                Id = id, // Tạo ID mới cho point
                Text = text,
                Embedding = null // Khởi tạo Embedding nếu cần
            };
            if (chapterId == null || clauseId == null || clauseItemId == null || newPoint == null)
            {
                return BadRequest("Invalid chapter or point data.");
            }
            var result = await _legalService.AddPointAsync(chapterId, clauseId, clauseItemId, newPoint);
            if (result)
            {
                return Ok("Point added successfully.");
            }
            else
            {
                return BadRequest("Failed to add point.");
            }
        }
    }
}
