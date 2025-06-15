using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class RegisteredUser
{
    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string Password { get; set; } = null!;
    public string? image { get; set; } = null!;

    public string? UserStatus { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
