using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class RegisteredUserRepository
    {
        private readonly AichatbotDbContext dbContext;

        public RegisteredUserRepository()
        {
        dbContext = new AichatbotDbContext();
        }

        public void CreateAccount(RegisteredUser account)
        {
            // Kiểm tra xem email đã tồn tại chưa
            var existingAccount = dbContext.RegisteredUsers.FirstOrDefault(a => a.UserEmail == account.UserEmail);
            if (existingAccount != null)
            {
                throw new Exception("Email đã được sử dụng.");
            }

            int newUserIdInt = 1;

            if (dbContext.RegisteredUsers.Any())
            {
                // Lấy UserId về dưới dạng IEnumerable để xử lý bằng LINQ in-memory
                var maxId = dbContext.RegisteredUsers
                    .AsEnumerable() // Chuyển sang in-memory LINQ
                    .Select(u =>
                    {
                        return int.TryParse(u.UserId, out int id) ? id : 0;
                    })
                    .Max();

                newUserIdInt = maxId + 1;
            }

            // Gán lại UserId mới (chuyển thành chuỗi)
            account.UserId = "USR" + newUserIdInt.ToString("D3"); // ví dụ: "001", "002"
            account.UserStatus = "Active"; // Gán trạng thái mặc định là "Active"
            account.Role = "User"; // Gán vai trò mặc định là "User"
            account.CreatedAt = DateTime.Now; // Gán thời gian tạo tài khoản
            dbContext.RegisteredUsers.Add(account);
            dbContext.SaveChanges();
        }

        public void UpdateAccount(string id, string newName, string newEmail, string newPassword)
        {
            var account = dbContext.RegisteredUsers.FirstOrDefault(p => p.UserId == id);

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(newEmail))
            {
                var emailExists = dbContext.RegisteredUsers.Any(u => u.UserEmail == newEmail && u.UserId != id);
                if (emailExists)
                {
                    throw new Exception("Email đã được sử dụng bởi tài khoản khác.");
                }

                account.UserEmail = newEmail;
            }

            // Nếu newName không rỗng thì cập nhật
            if (!string.IsNullOrWhiteSpace(newName))
            {
                account.UserName = newName;
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                account.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            dbContext.RegisteredUsers.Update(account);
            dbContext.SaveChanges();
        }

        public void UpdateAccountEmail(string id, string newEmail)
        {
            var account = dbContext.RegisteredUsers.FirstOrDefault(p => p.UserId == id);
            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại.");
            }
            // Kiểm tra xem email đã tồn tại chưa
            var emailExists = dbContext.RegisteredUsers.Any(u => u.UserEmail == newEmail && u.UserId != id);
            if (emailExists)
            {
                throw new Exception("Email đã được sử dụng bởi tài khoản khác.");
            }
            account.UserEmail = newEmail;
            dbContext.RegisteredUsers.Update(account);
            dbContext.SaveChanges();
        }

        public void UpdateAccountName(string id, string newName)
        {
            var account = dbContext.RegisteredUsers.FirstOrDefault(p => p.UserId == id);
            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại.");
            }
            account.UserName = newName;
            dbContext.RegisteredUsers.Update(account);
            dbContext.SaveChanges();
        }

        public void UpdateAccountPassword(string id, string newPassword)
        {
            var account = dbContext.RegisteredUsers.FirstOrDefault(p => p.UserId == id);
            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại.");
            }
            account.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            dbContext.RegisteredUsers.Update(account);
            dbContext.SaveChanges();
        }


        public RegisteredUser GetById(string id)
        {

            return dbContext.RegisteredUsers.SingleOrDefault(p => p.UserId.Equals(id));

        }


        public void DeleteAccount(string id)
        {
            var account = GetById(id)
                ?? dbContext.RegisteredUsers.FirstOrDefault(p => p.UserId == id);

            if (account != null)
            {
                account.UserStatus = "Banned"; // Chuyển trạng thái thành "Banned"

                dbContext.RegisteredUsers.Update(account); // Đánh dấu để cập nhật
                dbContext.SaveChanges();
            }
        }

        public List<RegisteredUser> GetAll()
        {

            return dbContext.RegisteredUsers.ToList();
        }

        public RegisteredUser Login(string email, string passwrod)
        {
            return dbContext.RegisteredUsers.FirstOrDefault(a => a.UserEmail.Equals(email) && a.Password.Equals(passwrod));

        }

        public void Register(string email, string password)
        {
            // Kiểm tra xem email đã tồn tại chưa
            var existingAccount = dbContext.RegisteredUsers.FirstOrDefault(a => a.UserEmail == email);
            if (existingAccount != null)
            {
                throw new Exception("Email đã được sử dụng.");
            }

            int newUserIdInt = 1;

            if (dbContext.RegisteredUsers.Any())
            {
                var maxId = dbContext.RegisteredUsers
                    .AsEnumerable()
                    .Select(u => int.TryParse(u.UserId?.Replace("USR", ""), out int id) ? id : 0)
                    .Max();

                newUserIdInt = maxId + 1;
            }

            var newUser = new RegisteredUser
            {
                UserId = "USR" + newUserIdInt.ToString("D3"),
                UserName = "User" + newUserIdInt.ToString("D3"), // Tạo tên người dùng mặc định
                UserEmail = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password), // Hash password bằng BCrypt
                UserStatus = "Active",
                Role = "User",
                CreatedAt = DateTime.Now,
                Questions = new List<Question>()
            };

            dbContext.RegisteredUsers.Add(newUser);
            dbContext.SaveChanges();
        }


    }
}
