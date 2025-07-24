using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IChatRoomService
    {
        ChatRoom GetOrCreateChatRoom(string userId);
        void AddQuestionToChatRoom(string chatRoomId, string questionId);
        Task<List<AnswQues>> GetUserDailyHistoryAsync(string userId);
    }
}
