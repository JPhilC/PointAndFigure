using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes;

namespace PnFDesktop.ViewModels
{
    public class MessagesViewModel : ToolViewModel
    {
        private StringBuilder builder = new StringBuilder();

        /// <summary>
        /// The <see cref="MessageString" /> property's name.
        /// </summary>
        public const string MessagesPropertyName = "Messages";

        /// <summary>
        /// Sets and gets the MessageString property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Messages
        {
            get
            {
                return builder.ToString();
            }
        }


        /// <summary>
        /// Initializes a new instance of the ModelToolboxViewModel class.
        /// </summary>
        public MessagesViewModel() : base("Messages")
        {
            ContentId = "Messages";
            InitialPaneLocation = InitialPaneLocation.Bottom;
            // Register to receive messages of type MessagePresenter and
            // when one is received add it to the list of messages and point 
            // the LastMessage property at it.
            WeakReferenceMessenger.Default.Register<MessagePresenter>(this, (r, message) => {
                string prefix = "";
                switch (message.MessageType)
                {
                    case LogType.Error:
                        prefix = "*** Error: ";
                        break;
                    case LogType.Validation:
                        prefix = "Validation: ";
                        break;
                    case LogType.Warning:
                        prefix = "Warning: ";
                        break;
                    case LogType.Information:
                    default:
                        prefix = "";
                        break;
                }
                builder.AppendLine(prefix + message.Message);
                if (message.Exception != null)
                {
                    builder.AppendLine("Exception: " + message.Exception.Message);
                }
                OnPropertyChanged(MessagesPropertyName);

            });

        }

        #region Relay commands ...
        private RelayCommand _ClearMessagesCommand;

        /// <summary>
        /// Gets the ClearMessagesCommand
        /// </summary>
        public RelayCommand ClearMessagesCommand
        {
            get
            {
                return _ClearMessagesCommand
                    ?? (_ClearMessagesCommand = new RelayCommand(
                                          () => {
                                              builder.Clear();
                                              OnPropertyChanged(MessagesPropertyName);
                                          }));
            }
        }

        #endregion

    }
}
