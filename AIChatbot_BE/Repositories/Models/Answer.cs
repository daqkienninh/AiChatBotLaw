using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Answer
{
    public string AnswerId { get; set; } = null!;

    public string? QuestionId { get; set; }

    public string AnsContent { get; set; } = null!;

    public DateTime? AnsCreateAt { get; set; }

    public string? LegalclauseId { get; set; }

    public virtual Question? Question { get; set; }
}
