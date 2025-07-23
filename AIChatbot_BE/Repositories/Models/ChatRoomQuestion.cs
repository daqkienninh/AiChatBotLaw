using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class ChatRoomQuestion
{
    public int Id { get; set; }

    public string ChatId { get; set; } = null!;

    public string QuestionId { get; set; } = null!;

    public virtual ChatRoom Chat { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
