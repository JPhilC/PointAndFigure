using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.Classes.Messaging
{
    public static class NotificationMessages
    {
        public static readonly string ShowBusyCursor = Guid.NewGuid().ToString();
        public static readonly string ShowDefaultCursor = Guid.NewGuid().ToString();
        public static readonly string GetPnFChart = Guid.NewGuid().ToString();
    }
}
