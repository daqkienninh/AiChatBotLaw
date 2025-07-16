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
        Task<bool> UpdateClauseTitleAsync(string chapterId, string clauseId, string newTitle);
        Task<List<LegalChapter>> GetAllLegalChapter();
    }
}
