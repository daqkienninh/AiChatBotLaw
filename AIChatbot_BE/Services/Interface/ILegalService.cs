using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repositories.LawRepository;

namespace Services.Interface
{
    public interface ILegalService
    {
        Task<SyncResult> ProcessPdfAsync(IFormFile file);
        Task<List<LegalChapter>> GetAllLegalChapter();

        Task<bool> UpdateClauseAsync(string chapterId, string clauseId, string? newClauseText = null, List<LegalClauseItem>? newClauseItems = null, List<LegalPoint>? newPoints = null);
    }
}
