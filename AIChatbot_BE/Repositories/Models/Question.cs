using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Question
{
    public string QuestionId { get; set; } = null!;

    public string? UserId { get; set; }

    public string QuestionContent { get; set; } = null!;

    public DateTime? QuesCreateAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual RegisteredUser? User { get; set; }
}
