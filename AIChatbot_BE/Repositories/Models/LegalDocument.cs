using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class LegalChapter
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; } = "CHUONG";
        public List<float>? Embedding { get; set; }
        public List<LegalClause> Clauses { get; set; } = new();
    }

    public class LegalClause
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; } = "DIEU";
        public List<float>? Embedding { get; set; }
        public List<LegalClauseItem> ClauseItems { get; set; } = new();
    }

    public class LegalClauseItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<float>? Embedding { get; set; }
        public List<LegalPoint> Points { get; set; } = new();
    }

    public class LegalPoint
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<float>? Embedding { get; set; }
    }
}
