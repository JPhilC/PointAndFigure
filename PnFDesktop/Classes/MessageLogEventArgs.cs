using System;

namespace PnFDesktop.Classes
{
    public enum LogType
    {
        Information = 0,
        Warning,
        Error,
        Validation
    }

    public class MessageLoggedEventArgs
       : EventArgs
    {
        private readonly LogType _LogType;
        private readonly string _Message;
        private readonly string _Details;
        private readonly Exception _Exception;
        private readonly ValidationError _ValidationError;
        private readonly DateTime _Timestamp;
        private bool _Cancel;

        public MessageLoggedEventArgs(LogType logType, string message)
        {
            _LogType = logType;
            _Message = message;
            _Details = null;
            _Exception = null;
            _ValidationError = null;
            _Timestamp = DateTime.Now;
        }

        public MessageLoggedEventArgs(LogType logType, string message, string details)
           : this(logType, message)
        {
            _Details = details;
        }

        public MessageLoggedEventArgs(LogType logType, string message, Exception exception)
        {
            _LogType = logType;
            _Message = message;
            _Details = FormatExceptionForMessage(exception);
            _Exception = exception;
            _ValidationError = null;
            _Timestamp = DateTime.Now;
        }

        public MessageLoggedEventArgs(ValidationError error)
        {
            _LogType = LogType.Validation;
            _Message = null;
            _Details = null;
            _Exception = null;
            _ValidationError = error;
            _Timestamp = DateTime.Now;
        }

        public LogType LogType => _LogType;

        public string Message =>
            _Message ?? (_ValidationError != null ? _ValidationError.Message
                : string.Empty);

        public string Details => _Details ?? string.Empty;

        public Exception Exception => _Exception;

        public ValidationError ValidationError => _ValidationError;

        public DateTime Timestamp => _Timestamp;

        public bool Cancel
        {
            get => _Cancel;
            set => _Cancel = value;
        }

        private string FormatExceptionForMessage(Exception ex)
        {
            string messageBody = FormatException(ex);
            if (ex != null)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    messageBody += ("[Inner Exception:]" + FormatException(ex));
                }
            }

            return messageBody;
        }

        private string FormatException(Exception ex)
        {
            if (ex != null)
            {
                return "\r\n'" + (ex.Source != null ? ex.Source : "<Unknown>")
                               + "' raised an unhandled "
                               + ex.GetType().Name
                               + " exception:\r\n\r\n"
                               + (ex.Message != null ? ex.Message : "")
                               + "\r\n\r\n"
                               + (ex.StackTrace != null ? ex.StackTrace : "")
                               + "\r\n";
            }
            else
            {
                return "";
            }
        }

    }
}
