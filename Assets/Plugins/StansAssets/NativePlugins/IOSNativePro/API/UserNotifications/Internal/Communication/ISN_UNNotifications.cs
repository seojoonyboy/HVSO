using System;
using System.Collections.Generic;
using UnityEngine;

namespace SA.iOS.UserNotifications.Internal
{

    [Serializable]
    public class ISN_UNNotifications 
    {
        [SerializeField] List<ISN_UNNotification> m_notifications;

        public List<ISN_UNNotification> Notifications {
            get {
                return m_notifications;
            }
        }
    }
}