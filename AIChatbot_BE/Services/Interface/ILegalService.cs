using MongoDB.Bson;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ILegalService
    {
        Task<bool> ProcessPdfAsync(string filePath);
        Task<List<LegalChapter>> GetAllLegalChapter();

        Task<bool> UpdateClauseTitleAsync(string chapterId, string clauseId, string newTitle);
        Task<bool> CreateChapterAsync(LegalChapter legalChapter);
        Task<bool> AddClauseToChapterAsync(string chapterId, LegalClause newClause);
        Task<bool> AddClauseItemAsync(string chapterId, string clauseId, BsonDocument newClauseItem);
        Task<bool> AddPointAsync(string chapterId, string clauseId, string clauseItemId, BsonDocument newPoint);
    }
}
