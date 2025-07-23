using Repositories;
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
        RegisteredUser GetAccountByEmail(string email);
        List<RegisteredUser> GetAllAccounts();
        void Register(string email, string password); // Optional: if you want to have a separate method for registration
    }
}
