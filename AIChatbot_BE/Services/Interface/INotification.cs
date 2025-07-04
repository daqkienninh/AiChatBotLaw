using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Models;

namespace Services.Interface
{
    public interface INotification
    {
        void CreateNotification(Notification notification);
        void UpdateNotification(Notification updatedNotification);
        void DeleteNotification(string notificationId);
        Notification GetNotificationById(string notificationId);
    }
}