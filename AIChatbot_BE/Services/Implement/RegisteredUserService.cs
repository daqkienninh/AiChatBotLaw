using Repositories;
using Repositories.Models;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implement
{
    public class RegisteredUserService : IRegisteredUser
    {
        private readonly RegisteredUserRepository _registeredUserRepository;

        public RegisteredUserService()
        {
        }
        public RegisteredUserService(RegisteredUserRepository registeredUserRepository)
        {
            _registeredUserRepository = registeredUserRepository;
        }
        public void CreateAccount(RegisteredUser account)
        {
            _registeredUserRepository.CreateAccount(account);
        }

        public void DeleteAccount(string userId)
        {
            _registeredUserRepository.DeleteAccount(userId);
        }

        public RegisteredUser GetAccountById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            var result = _registeredUserRepository.GetById(userId);
            return result;
        }

        public List<RegisteredUser> GetAllAccounts()
        {
            var result = _registeredUserRepository.GetAll();
            return result;
        }

        public RegisteredUser Login(string email, string password)
        {
            return _registeredUserRepository.Login(email, password);
        }

        public void UpdateAccount(RegisteredUser c)
        {
            _registeredUserRepository.UpdateAccount(c);
        }
    }
}
