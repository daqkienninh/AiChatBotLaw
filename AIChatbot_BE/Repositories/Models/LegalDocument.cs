using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class LegalDocument
    {
        public object Id { get; set; }

        public string DocumentTitle { get; set; }
        public List<LegalChapter> Chapters { get; set; } = new();
    }

    public class LegalChapter
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public List<float>? Embedding { get; set; }
        public List<LegalClause> Clauses { get; set; } = new();
    }

    public class LegalClause
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
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
