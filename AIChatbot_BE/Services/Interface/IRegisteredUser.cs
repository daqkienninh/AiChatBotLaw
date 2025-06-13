using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IRegisteredUser
    {
        void CreateAccount(RegisteredUser account);
        void UpdateAccount(RegisteredUser updatedUser);
        void DeleteAccount(string userId);
        RegisteredUser GetAccountById(string userId);
        List<RegisteredUser> GetAllAccounts();
        RegisteredUser Login(string email, string password);

        void Register(string email, string password); // Optional: if you want to have a separate method for registration
    }
}
