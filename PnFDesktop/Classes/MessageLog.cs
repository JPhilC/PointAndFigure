using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PnFDesktop.Config;


namespace PnFDesktop.Classes
{
    public class MessageLog
    {

        public event EventHandler<MessageLoggedEventArgs> MessageLogged;
        public event EventHandler ResetMessages;

        public static MessageLog Instance = new MessageLog();

        private MessageLog()
        {
        }

        public static void LogMessage(object sender, LogType logType, string message)
        {
            MessageLoggedEventArgs args = new MessageLoggedEventArgs(logType, message);
            MessageLog.Instance.OnMessageLogged(sender, args);
        }

        public static void LogMessage(object sender, LogType logType, string message, string details)
        {
            MessageLoggedEventArgs args = new MessageLoggedEventArgs(logType, message, details);
            MessageLog.Instance.OnMessageLogged(sender, args);
        }

        public static void LogMessage(object sender, LogType logType, string message, Exception exception)
        {
            MessageLoggedEventArgs args = new MessageLoggedEventArgs(logType, message, exception);
            // Write the exception out
            MessageLog.writeException(exception);
            MessageLog.Instance.OnMessageLogged(sender, args);
        }

        private static void writeException(Exception e)
        {
            if (e != null)
            {
                string logFileName = Path.Combine(PnFDesktopConfig.ErrorLogPath, Path.GetRandomFileName() + ".err");
                StringBuilder sb = new StringBuilder();
                MessageLog.writeException(sb, e);
                using (TextWriter logWriter = new StreamWriter(logFileName))
                {
                    logWriter.Write(sb.ToString());
                    logWriter.Close();
                }
            }
        }

        /// <summary>
        /// Pushes exception details into a StringBuilder recursively as long as InnerExceptions exist.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="e"></param>
        private static void writeException(StringBuilder sb, Exception e)
        {
            if (e != null)
            {
                sb.AppendLine(e.GetType().Name);
                sb.AppendLine("Exception: " + e.Message);
                sb.AppendLine("Source: " + e.Source);
                sb.AppendLine("Stack Trace");
                sb.AppendLine(e.StackTrace);
                if (e.InnerException != null)
                {
                    sb.AppendLine("Inner Exception");
                    writeException(sb, e.InnerException);
                }
            }
        }


        public static void LogMessage(object sender, ValidationError error)
        {
            MessageLoggedEventArgs args = new MessageLoggedEventArgs(error);
            MessageLog.Instance.OnMessageLogged(sender, args);
        }

        private void OnMessageLogged(object sender, MessageLoggedEventArgs e)
        {
            if (this.MessageLogged != null)
            {
                MessageLogged(sender, e);
            }
        }

        public static void Reset(object sender)
        {
            if (MessageLog.Instance.ResetMessages != null)
            {
                MessageLog.Instance.ResetMessages(sender, EventArgs.Empty);
            }
        }
    }
}
