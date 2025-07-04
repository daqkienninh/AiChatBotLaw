using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.DBContext;
using Repositories.Models;

namespace Repositories
{
    public class NotificationRepository
    {
        private readonly AichatbotDbContext dbContext;

        public NotificationRepository()
        {
            dbContext = new AichatbotDbContext();
        }

        public void CreateNotification(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            int newNotificationIdInt = 1;
            if (dbContext.Questions.Any())
            {
                var maxId = dbContext.Notifications
                .AsEnumerable()
                .Select(q => int.TryParse(q.NotificationId, out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();

                newNotificationIdInt = maxId + 1;
            }
            notification.NotificationId = newNotificationIdInt.ToString();
            notification.Title = notification.Title ?? "Default Title";
            notification.Content = notification.Content ?? "Default Content";
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsGlobal = true;
            dbContext.Notifications.Add(notification);
            dbContext.SaveChanges();
        }
        public void UpdateNotification(Notification updatedNotification)
        {
            var existing = dbContext.Notifications.FirstOrDefault(n => n.NotificationId == updatedNotification.NotificationId);
            if (existing != null)
            {
                existing.Title = updatedNotification.Title;
                existing.Content = updatedNotification.Content;
                existing.IsGlobal = updatedNotification.IsGlobal;

                dbContext.SaveChanges();
            }
        }
        public void DeleteNotification(string notificationId)
        {
            var notification = dbContext.Notifications.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification != null)
            {
                dbContext.Notifications.Remove(notification);
                dbContext.SaveChanges();
            }
        }
        public Notification GetNotificationById(string notificationId)
        {
            return dbContext.Notifications.FirstOrDefault(n => n.NotificationId == notificationId);
        }
    }
}
