using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class ChatRoom
{
    public string ChatId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ChatRoomQuestion> ChatRoomQuestions { get; set; } = new List<ChatRoomQuestion>();
}
