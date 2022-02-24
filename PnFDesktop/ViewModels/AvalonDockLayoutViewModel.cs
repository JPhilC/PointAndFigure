using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes;
using PnFDesktop.Interfaces;
using PnFDesktop.Messaging;
using Xceed.Wpf.AvalonDock;

namespace PnFDesktop.ViewModels
{
    public class AvalonDockLayoutViewModel
    {
        #region fields ...
        private const string LayoutFileName = "layout.user";
        private const string DefaultLayoutFilename = "default.layout";
        // The XML workspace layout string is stored in this field
        private string _current_layout;

        ILayoutViewModelParent _parent = null;

        #endregion

        #region Constructor ...
        /// <summary>
        /// Parameterized class constructor to model properties that
        /// are required for access to parent viewmodel.
        /// </summary>
        /// <param name="parent"></param>
        public AvalonDockLayoutViewModel(ILayoutViewModelParent parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Hidden class constructor
        /// </summary>
        protected AvalonDockLayoutViewModel()
        {

        }
        #endregion

        #region Commands ...
        #region Load/Save workspace to temporary string ...
        private RelayCommand _SaveWorkspaceLayoutToStringCommand;
        public RelayCommand SaveWorkspaceLayoutToStringCommand
        {
            get
            {
                return _SaveWorkspaceLayoutToStringCommand ??= new RelayCommand(() =>
                    {
                        // Sends a GetWorkspaceLayout message to registered recipients. The message will reach all recipients
                        // that registered for this message type using one of the Register methods.
                        WeakReferenceMessenger.Default.Send(new NotificationMessageAction<string>(
                            LayoutNotificationMessages.GetWorkspaceLayout,
                            (result) =>
                            {
                                _parent.IsBusy = true;
                                CommandManager.InvalidateRequerySuggested();
                                _current_layout = result;
                                _parent.IsBusy = false;
                            })
                        );
                    },
                    () => !_parent.IsBusy
                );
            }
        }

        private RelayCommand _LoadWorkspaceLayoutFromStringCommand;
        public RelayCommand LoadWorkspaceLayoutFromStringCommand
        {
            get
            {
                return _LoadWorkspaceLayoutFromStringCommand ??= new RelayCommand(() =>
                   {
                   // Is there any layout that could possible be loaded?
                   if (string.IsNullOrEmpty(_current_layout) == true)
                           return;

                       // Sends a LoadWorkspaceLayout message to registered recipients. The message will reach all recipients
                       // that registered for this message type using one of the Register methods.
                       WeakReferenceMessenger.Default.Send(new NotificationMessage<string>(_current_layout, LayoutNotificationMessages.LoadWorkspaceLayout));
                   },
                    () => !_parent.IsBusy && !string.IsNullOrEmpty(_current_layout)
                   );
            }
        }

        #endregion Load/Save workspace to temporary string

        #region Load Save WorkSpace on application start-up and shutdown ...

        private RelayCommand<object> _LoadLayoutCommand;
        /// <summary>
        /// Implement a command to load the layout of an AvalonDock-DockingManager instance.
        /// This layout defines the position and shape of each document and tool window
        /// displayed in the application.
        /// 
        /// Parameter:
        /// The command expects a reference to a <seealso cref="DockingManager"/> instance to
        /// work correctly. Not supplying that reference results in not loading a layout (silent return).
        /// </summary>
        public RelayCommand<object> LoadLayoutCommand
        {
            get
            {
                return _LoadLayoutCommand
                   ?? (_LoadLayoutCommand = new RelayCommand<object>((p) =>
                   {
                       DockingManager docManager = p as DockingManager;

                       if (docManager == null)
                           return;

                       try
                       {
                           this.LoadDockingManagerLayout();
                       }
                       catch
                       {
                       }
                       finally
                       {
                           _parent.IsBusy = false;
                       }
                   }));
            }
        }


        private RelayCommand<object> _SaveLayoutCommand;
        /// <summary>
        /// Implements a command to save the layout of an AvalonDock-DockingManager instance.
        /// This layout defines the position and shape of each document and tool window
        /// displayed in the application.
        /// 
        /// Parameter:
        /// The command expects a reference to a <seealso cref="string"/> instance to
        /// work correctly. The string is supposed to contain the XML layout persisted
        /// from the DockingManager instance. Not supplying that reference to the string
        /// results in not saving a layout (silent return).
        /// </summary>
        public RelayCommand<object> SaveLayoutCommand
        {
            get
            {
                return _SaveLayoutCommand
                   ?? (_SaveLayoutCommand = new RelayCommand<object>((p) =>
                   {
                       string xmlLayout = p as string;

                       if (xmlLayout == null)
                           return;

                       this.SaveDockingManagerLayout(xmlLayout);
                   }));
            }
        }

        #endregion Load Save WorkSpace on application start-up and shutdown ...





        #endregion


        #region Methods ...
        #region LoadLayout
        /// <summary>
        /// Loads the layout of a particular docking manager instance from persistence
        /// and checks whether a file should really be reloaded (some files may no longer
        /// be available).
        /// </summary>
        private void LoadDockingManagerLayout()
        {
            string layoutFileName = string.Empty;

            try
            {
                _parent.IsBusy = true;

                layoutFileName = System.IO.Path.Combine(Environment.SpecialFolder.ApplicationData.ToString(), "PnFDesktop", AvalonDockLayoutViewModel.LayoutFileName);

                if (!System.IO.File.Exists(layoutFileName))
                {
                    // Check for default layout if no user layout at the moment.
                    layoutFileName = System.IO.Path.Combine(Environment.SpecialFolder.ApplicationData.ToString(), "PnFDesktop", AvalonDockLayoutViewModel.DefaultLayoutFilename);
                    if (!System.IO.File.Exists(layoutFileName))
                    {
                        _parent.IsBusy = false;
                        return;
                    }
                }

                string sTaskError = string.Empty;

                Task taskToProcess = null;
                taskToProcess = Task.Factory.StartNew<string>((stateObj) =>
                {
                    string xml = string.Empty;

                    try
                    {
                        // Begin Aysnc Task
                        using (FileStream fs = new FileStream(layoutFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (StreamReader reader = new StreamReader(fs, Encoding.Default))
                            {
                                xml = reader.ReadToEnd();
                            }
                        }
                    }
                    catch (OperationCanceledException exp)
                    {
                        throw exp;
                    }
                    catch (Exception except)
                    {
                        throw except;
                    }
                    finally
                    {
                    }

                    return xml;                     // End of async task

                }, null).ContinueWith(ant =>
                {
                    try
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            WeakReferenceMessenger.Default.Send(new NotificationMessage<string>(ant.Result, LayoutNotificationMessages.LoadWorkspaceLayout));
                        }),
                        DispatcherPriority.Background);
                    }
                    catch (AggregateException aggExp)
                    {
                        throw new Exception("One or more errors have occured during load layout processing.", aggExp);
                    }
                    finally
                    {
                        _parent.IsBusy = false;
                    }
                });
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
        #endregion LoadLayout

        #region SaveLayout
        private void SaveDockingManagerLayout(string xmlLayout)
        {
            // Create XML Layout file on close application (for re-load on application re-start)
            if (xmlLayout == null)
                return;

            string fileName = System.IO.Path.Combine(Environment.SpecialFolder.ApplicationData.ToString(), "PnFDesktop", AvalonDockLayoutViewModel.LayoutFileName);

            File.WriteAllText(fileName, xmlLayout);
        }
        #endregion SaveLayout

        #endregion
    }
}
