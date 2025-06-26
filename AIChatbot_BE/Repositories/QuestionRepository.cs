using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;

namespace Repositories
{
    public class QuestionRepository
    {
        private readonly AichatbotDbContext dbContext;

        public QuestionRepository()
        {
            dbContext = new AichatbotDbContext();
        }

        public void CreateQuestion(Question question)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            int newQuestionIdInt = 1;

            if (dbContext.Questions.Any())
            {
                var maxId = dbContext.Questions
                    .AsEnumerable()
                    .Select(q => int.TryParse(q.QuestionId, out int id) ? id : 0)
                    .Max();

                newQuestionIdInt = maxId + 1;
            }

            question.QuestionId = newQuestionIdInt.ToString();
            question.UserId = question.UserId ?? "USR000";
            question.QuestionContent = question.QuestionContent ?? "";
            question.QuesCreateAt = DateTime.Now;

            dbContext.Questions.Add(question);
            dbContext.SaveChanges();
        }

        public void DeleteQuestion(string questionId)
        {
            if (string.IsNullOrEmpty(questionId))
                throw new ArgumentNullException(nameof(questionId));
            var question = dbContext.Questions.Find(questionId);
            if (question != null)
            {
                dbContext.Questions.Remove(question);
                dbContext.SaveChanges();
            }
        }

        public Question GetQuestionById(string questionId)
        {
            if (string.IsNullOrEmpty(questionId))
                throw new ArgumentNullException(nameof(questionId));

            return dbContext.Questions
                .Include(q => q.Answers)
                .Include(q => q.User)
                .FirstOrDefault(q => q.QuestionId == questionId);
        }
    }
}