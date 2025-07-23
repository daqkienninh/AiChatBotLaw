using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class UpdateClauseDTO
    {
        public string ChapterId { get; set; }
        public string ClauseId { get; set; }
        public string? NewClauseText { get; set; }
        public List<LegalClauseItem>? NewClauseItems { get; set; }
        public List<LegalPoint>? NewPoints { get; set; }
    }
}
