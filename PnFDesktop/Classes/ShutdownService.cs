using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Messaging;
using System;


namespace PnFDesktop.Classes
{
    public static class ShutdownNotifications
    {
        public static readonly string ConfirmShutdown = Guid.NewGuid().ToString();

        public static readonly string NotifyShutdown = Guid.NewGuid().ToString();
    }

    public static class ShutdownService
    {
        public static bool RequestShutdown()
        {
            var shouldAbortShutdown = false;

            WeakReferenceMessenger.Default.Send(new NotificationMessageAction<bool>(
                ShutdownNotifications.ConfirmShutdown,
                shouldAbort => shouldAbortShutdown |= shouldAbort));

            if (!shouldAbortShutdown)
            {
                // This time it is for real
                WeakReferenceMessenger.Default.Send(new NotificationMessage(ShutdownNotifications.NotifyShutdown));
            }
            return !shouldAbortShutdown;
        }
    }
}