using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ChatRoomRepository
    {
        private readonly TestDbContext dbContext;
        public ChatRoomRepository()
        {
            dbContext = new();
        }
        public ChatRoom GetOrCreateChatRoom(string userId)
        {
            var today = DateTime.Today;

            var chatRoom = dbContext.ChatRooms
                .FirstOrDefault(c => c.UserId == userId && c.CreatedAt == today);

            if (chatRoom == null)
            {
                chatRoom = new ChatRoom
                {
                    ChatId = Guid.NewGuid().ToString(), // Generate a new unique ChatId
                    UserId = userId,
                    CreatedAt = today
                };
                dbContext.ChatRooms.Add(chatRoom);
                dbContext.SaveChanges();
            }
            return chatRoom;
        }

        public ChatRoom GetChatRoomById(string chatId)
        {
            if (string.IsNullOrEmpty(chatId))
            {
                throw new ArgumentException("Chat ID cannot be null or empty.", nameof(chatId));
            }
            return dbContext.ChatRooms.FirstOrDefault(cr => cr.ChatId == chatId);
        }
        public List<ChatRoom> GetAllChatRooms()
        {
            return dbContext.ChatRooms.ToList();
        }
        public async Task<List<string>> GetQuestionIdsForUserTodayAsync(string userId)
        {
            var today = DateTime.Today;

            var chatRoom = await dbContext.ChatRooms
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CreatedAt == today);

            if (chatRoom == null)
                return new List<string>();

            var questionIds = await dbContext.ChatRoomQuestions
                .Where(crq => crq.ChatId == chatRoom.ChatId)
                .Select(crq => crq.QuestionId)
                .ToListAsync();

            return questionIds;
        }

        public void AddQuestionToChatRoom(string chatRoomId, string questionId)
        {
            var chatRoomQuestion = new ChatRoomQuestion
            {
                ChatId = chatRoomId,
                QuestionId = questionId
            };

            dbContext.ChatRoomQuestions.Add(chatRoomQuestion);
            dbContext.SaveChanges();
        }
        public void ProcessNewQuestion(string userId, string questionId)
        {
            // 1. Lấy hoặc tạo ChatRoom cho user hôm nay
            var chatRoom = GetOrCreateChatRoom(userId);

            // 2. Gắn Question vào ChatRoom
            AddQuestionToChatRoom(chatRoom.ChatId, questionId);
        }
        public List<string> GetQuestionsInChatRoom(string chatRoomId)
        {
            return dbContext.ChatRoomQuestions
                .Where(q => q.ChatId == chatRoomId)
                .Select(q => q.QuestionId)
                .ToList();
        }
    }
}
