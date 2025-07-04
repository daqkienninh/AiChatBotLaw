using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Models;

namespace Services.Interface
{
    public interface IQuestion
    {
        void CreateQuestion(Question question);
        void DeleteQuestion(string questionId);
        Question GetQuestionById(string questionId);
    }
}
