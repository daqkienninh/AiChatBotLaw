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

        public void UpdateAccount(RegisteredUser updatedUser)
        {
            var account = dbContext.RegisteredUsers
                .FirstOrDefault(p => p.UserId == updatedUser.UserId);

            if (account == null)
                throw new Exception("Tài khoản không tồn tại.");

            // Cập nhật Email nếu được truyền và khác email cũ
            if (!string.IsNullOrWhiteSpace(updatedUser.UserEmail) &&
                updatedUser.UserEmail != account.UserEmail)
            {
                var emailExists = dbContext.RegisteredUsers
                    .Any(u => u.UserEmail == updatedUser.UserEmail && u.UserId != account.UserId);

                if (emailExists)
                    throw new Exception("Email đã được sử dụng bởi tài khoản khác.");

                account.UserEmail = updatedUser.UserEmail;
            }

            // Cập nhật Name nếu có
            if (!string.IsNullOrWhiteSpace(updatedUser.UserName))
            {
                account.UserName = updatedUser.UserName;
            }

            // Cập nhật Password nếu có
            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                account.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
            }

            // Cập nhật Image nếu có
            if(!string.IsNullOrWhiteSpace(updatedUser.image))
            {
                account.image = updatedUser.image;
            }

            // (Không thay đổi Role, Status, CreatedAt nếu không cần)

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
