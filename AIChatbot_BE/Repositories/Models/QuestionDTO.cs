using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class QuestionDTO
    {   
        public string QuestionId { get; set; }
        public string UserId { get; set; }
        public string QuestionContent { get; set; }
        public DateTime? QuesCreateAt { get; set; }
        public string? Embedding { get; set; }
    }
}