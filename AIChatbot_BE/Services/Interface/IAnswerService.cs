using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IAnswerService
    {
        public Task CreateAnswerFromQuestionAsync(Question question);
        public Answer GetAnswerById(string answerId);
    }
}
