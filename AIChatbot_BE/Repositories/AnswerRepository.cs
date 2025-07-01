using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class AnswerRepository
    {
        private readonly AichatbotDbContext _context;
        public AnswerRepository()
        {
            _context = new AichatbotDbContext();
        }
        
        public void AddAnswer(Answer answer)
        {
            _context.Answers.Add(answer);
            _context.SaveChanges();
        }
        
        public Answer GetAnswerById(string answerId)
        {
            if (string.IsNullOrEmpty(answerId))
                throw new ArgumentNullException(nameof(answerId));
            return _context.Answers.FirstOrDefault(a => a.AnswerId == answerId);
        }
    }
}
