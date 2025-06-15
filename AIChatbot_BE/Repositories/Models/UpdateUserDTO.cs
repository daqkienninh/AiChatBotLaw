using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class UpdateUserDTO
    {
            public string UserName { get; set; }
            public string UserEmail { get; set; }
            public string Password { get; set; }
            public string? Image { get; set; } = null!;
    }
}
