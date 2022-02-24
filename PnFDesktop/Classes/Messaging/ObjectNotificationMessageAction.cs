using System;

namespace PnFDesktop.Messaging
{
    public class ObjectNotificationMessageAction<TCallbackParameter> : NotificationMessageAction<TCallbackParameter>
    {
        public Guid ObjectId { get; private set; }

        public ObjectNotificationMessageAction(string notification, Guid objectId, Action<TCallbackParameter> callback)
            : base(notification, callback)
        {
            ObjectId = objectId;
        }

        public ObjectNotificationMessageAction(object sender, string notification, Guid objectId, Action<TCallbackParameter> callback)
            : base(sender, notification, callback)
        {
            ObjectId = objectId;
        }

        public ObjectNotificationMessageAction(object sender, object target, string notification, Guid objectId, Action<TCallbackParameter> callback)
            : base(sender, target, notification, callback)
        {
            ObjectId = objectId;
        }

    }
}
