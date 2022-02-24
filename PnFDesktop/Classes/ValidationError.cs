using System;
using System.Diagnostics;

namespace PnFDesktop.Classes
{
    public class ValidationError
    {
        private Guid _ItemId;
        private string _Property;
        private string _Message;

        public ValidationError(Guid itemId, string property, string message)
        {
            Debug.Assert(itemId != Guid.Empty, "No Item Id specified.");
            Debug.Assert(property != null, "No property specified.");    /* An empty string is okay if error doesn't relate to a specific property */
            Debug.Assert(!string.IsNullOrEmpty(message), "No message specified.");

            _ItemId = itemId;
            _Property = property;
            _Message = message;
        }


        public Guid ItemId
        {
            get => _ItemId;
            set => _ItemId = value;
        }

        public string Property
        {
            get => _Property;
            set => _Property = value;
        }

        public string Message
        {
            get => _Message;
            set => _Message = value;
        }
    }
}
