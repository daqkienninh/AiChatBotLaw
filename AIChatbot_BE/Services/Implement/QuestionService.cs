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

        public QuestionService(QuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public void CreateQuestion(CreateQuestionDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.QuestionContent))
            {
                throw new ArgumentException("Invalid question data.");
            }

            // Tự tạo ID hoặc để repository sinh nếu bạn dùng auto-increment
            var question = new Question
            {
                UserId = dto.UserId,
                QuestionContent = dto.QuestionContent,
                QuesCreateAt = DateTime.UtcNow // hoặc DateTime.Now nếu bạn không dùng UTC
            };

            _questionRepository.CreateQuestion(question);
        }

        public void DeleteQuestion(string questionId)
        {
            if (string.IsNullOrEmpty(questionId))
            {
                throw new ArgumentException("Question ID is required.");
            }
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

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Repositories;
//using Repositories.Models;
//using Services.Interface;
//namespace Services.Implement
//{
//    public class QuestionService : IQuestion
//    {
//        private readonly QuestionRepository _questionRepository = new QuestionRepository();

//        public QuestionService()
//        {
//        }
//        public QuestionService(QuestionRepository questionRepository)
//        {
//            _questionRepository = questionRepository;
//        }
//        public void CreateQuestion(Question question)
//        {
//            if (question == null)
//            {
//                throw new ArgumentException("Invalid question data!");
//            }

//            _questionRepository.CreateQuestion(question);
//        }

//        public void DeleteQuestion(string questionId)
//        {
//            if (string.IsNullOrWhiteSpace(questionId))
//            {
//                throw new ArgumentException("Question ID is required!");
//            }

//            try
//            {
//                _questionRepository.DeleteQuestion(questionId);
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException($"Error deleting question: {ex.Message}", ex);
//            }
//        }

//        public Question GetQuestionById(string questionId)
//        {
//            if (string.IsNullOrEmpty(questionId))
//            {
//                return null;
//            }
//            var result = _questionRepository.GetQuestionById(questionId);
//            return result;
//        }
//    }
//}