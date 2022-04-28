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
        /// Sets and gets the MessageString property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Messages => builder.ToString();


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
                App.Current.Dispatcher.Invoke(() =>
                    builder.AppendLine(prefix + message.Message)
                );
                if (message.Exception != null)
                {
                    App.Current.Dispatcher.Invoke(()=>
                        builder.AppendLine("Exception: " + message.Exception.Message)
                    );
                }
                OnPropertyChanged("Messages");

            });

        }

        #region Relay commands ...
        private RelayCommand _clearMessagesCommand;

        /// <summary>
        /// Gets the ClearMessagesCommand
        /// </summary>
        public RelayCommand ClearMessagesCommand
        {
            get
            {
                return _clearMessagesCommand ??= new RelayCommand(
                    () => {
                        builder.Clear();
                        OnPropertyChanged("Messages");
                    });
            }
        }

        #endregion

    }
}
