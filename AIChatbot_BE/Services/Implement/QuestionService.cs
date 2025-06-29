using Repositories;
using Repositories.Models;
using Services.Interface;

namespace Services.Implement
{
    public class QuestionService : IQuestion
    {
        private readonly QuestionRepository _questionRepository = new QuestionRepository();

        public QuestionService()
        {
        }

        public void CreateQuestion(Question question)
        {
            _questionRepository.CreateQuestion(question);
        }

        public void DeleteQuestion(string questionId)
        {
            _questionRepository.DeleteQuestion(questionId);
        }

        public Question GetQuestionById(string questionId)
        {
            if (string.IsNullOrEmpty(questionId))
            {
                return null;
            }
            var result = _questionRepository.GetQuestionById(questionId);
            return result;
        }
    }
}