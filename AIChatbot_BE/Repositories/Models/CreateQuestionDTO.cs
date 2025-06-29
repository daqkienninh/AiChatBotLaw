using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class CreateQuestionDTO
    {
        public string UserId { get; set; }
        public string QuestionContent { get; set; }
    }
}
