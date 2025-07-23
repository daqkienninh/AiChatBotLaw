using System;
using System.Collections.Generic;

namespace Repositories;

public partial class RegisteredUser
{
    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string? UserStatus { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Password { get; set; }

    public string? Image { get; set; }

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
