using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories;
using Repositories.Models;
using Services.Interface;

namespace Services.Implement
{
    public class NotificationService : INotification
    {
        private readonly NotificationRepository _notificationRepository = new NotificationRepository();
        public NotificationService()
        {
        }
        public NotificationService(NotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public void CreateNotification(Notification notification)
        {
            _notificationRepository.CreateNotification(notification);
        }

        public void UpdateNotification(Notification updatedNotification)
        {
            _notificationRepository.UpdateNotification(updatedNotification);
        }

        public void DeleteNotification(string notificationId)
        {
            _notificationRepository.DeleteNotification(notificationId);
        }

        public Notification GetNotificationById(string notificationId)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                return null;
            }
            var result = _notificationRepository.GetNotificationById(notificationId);
            return result;
        }

    }
}