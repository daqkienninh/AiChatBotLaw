using Azure.Core;
using Microsoft.AspNetCore.Authorization;
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

            var result = await _legalService.ProcessPdfAsync(file);

            return Ok(new { inserted = result.Inserted, updated = result.Updated, skipped = result.Skipped });
        }

        [HttpPut("Chapter")]
        public async Task<IActionResult> UpdateClause([FromBody] UpdateClauseDTO dto)
        {
            var success = await _legalService.UpdateClauseAsync(
            dto.ChapterId,
            dto.ClauseId,
            dto.NewClauseText,
            dto.NewClauseItems,
            dto.NewPoints
        );

            if (success)
                return Ok(new { message = "Cập nhật thành công." });
            else
                return BadRequest(new { message = "Không có gì được cập nhật hoặc lỗi." });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllChapters()
        {
            var chapters = await _legalService.GetAllLegalChapter();
            return Ok(chapters);
        }

    }
}
