using Repositories.Models;
using System;
using System.Collections.Generic;

namespace Repositories;

public partial class Question
{
    public string QuestionId { get; set; } = null!;

    public string? UserId { get; set; }

    public string QuestionContent { get; set; } = null!;

    public DateTime? QuesCreateAt { get; set; }

    public string? Embedding { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<ChatRoomQuestion> ChatRoomQuestions { get; set; } = new List<ChatRoomQuestion>();

    public virtual RegisteredUser? User { get; set; }
}
