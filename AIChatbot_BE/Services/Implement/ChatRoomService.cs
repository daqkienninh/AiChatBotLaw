using Repositories;
using Repositories.Models;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Services.Implement
{
    public class ChatRoomService : IChatRoomService
    {
        private readonly ChatRoomRepository _chatRoomRepository;
        private readonly QuestionRepository _questionRepository;
        private readonly AnswerRepository _answQuesRepository;
        public ChatRoomService()
        {
            _chatRoomRepository = new ChatRoomRepository();
            _questionRepository = new QuestionRepository();
            _answQuesRepository = new AnswerRepository();
        }

        public void AddQuestionToChatRoom(string chatRoomId, string questionId)
        {
            _chatRoomRepository.AddQuestionToChatRoom(chatRoomId, questionId);
        }

        public ChatRoom GetOrCreateChatRoom(string userId)
        {
            return _chatRoomRepository.GetOrCreateChatRoom(userId);
        }

        public async Task<List<AnswQues>> GetUserDailyHistoryAsync(string userId)
        {
            List<string> questionIds = new List<string>();
            questionIds = await _chatRoomRepository.GetQuestionIdsForUserTodayAsync(userId);
            List<AnswQues> answQuesList = new List<AnswQues>();

            foreach (var questionId in questionIds)
            {
                var question = _questionRepository.GetQuestionById(questionId);
                var answer = _answQuesRepository.GetAnswerById(questionId);
                if (question != null)
                {
                    // Assuming you want to do something with the question, like logging or processing
                    Console.WriteLine($"Question ID: {question.QuestionId}, Content: {question.QuestionContent}");
                }
                AnswQues answQues = new AnswQues
                {
                    QuestionContent = question.QuestionContent,
                    AnswerContent = answer.AnsContent,
                };
                answQuesList.Add(answQues);
            }
            return answQuesList;

        }
    }
}
