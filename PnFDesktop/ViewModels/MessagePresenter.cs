using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFDesktop.Classes;

namespace PnFDesktop.ViewModels
{
    public class MessagePresenter : ObservableObject
    {

        private LogType _messageType;

        /// <summary>
        /// Sets and gets the MessageType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public LogType MessageType
        {
            get => _messageType;
            set => SetProperty(ref _messageType, value);
        }

        private string _message = "";

        /// <summary>
        /// Sets and gets the Message property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private Exception _exception = null;

        /// <summary>
        /// Sets and gets the Message property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Exception Exception
        {
            get => _exception;
            set => SetProperty(ref _exception, value);
        }

        public MessagePresenter(MessageLoggedEventArgs eventLogArgs)
        {
            MessageType = eventLogArgs.LogType;
            Message = eventLogArgs.Message;
            Exception = eventLogArgs.Exception;
        }
    }
}
