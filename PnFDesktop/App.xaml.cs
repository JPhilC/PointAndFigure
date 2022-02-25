using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes;
using PnFDesktop.Config;
using PnFDesktop.ViewModels;
using System;
using System.IO;
using System.Text;
using System.Windows;

namespace PnFDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);

            // Using FirstChangeExceptions generates too much noise.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            MessageLog.Instance.MessageLogged += Instance_MessageLogged;
            // Load any saved form layout settings.
            FormLayoutManager.Current.Load();
        }



        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Save the form layout settings.
                FormLayoutManager.Current.Save();
                // Save application options
                ApplicationOptionsManager.SaveOptions();
                MessageLog.Instance.MessageLogged -= Instance_MessageLogged;
            }
            catch (Exception ex)
            {
                writeException(ex);
                MessageBox.Show(
                    "An unexpected error has occurred within Gems. Error details have been logged and Gems will now close.",
                    "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                base.OnExit(e);
            }
        }

        void Instance_MessageLogged(object sender, MessageLoggedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new MessagePresenter(e));
        }

        #region Application exception handling ...

        /// <summary>
        /// Application level exception handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            this.DispatcherUnhandledException -=
                new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(
                    this.Application_DispatcherUnhandledException);
            writeException(e.Exception);
            MessageBox.Show(
                "An unexpected error has occurred within PnFDesktop. Error details have been logged and PnFDesktop will now close.",
                "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            this.Shutdown();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            writeException((Exception)e.ExceptionObject);
            MessageBox.Show(
                "An unexpected error has occurred within PnFDesktop. Error details have been logged and PnFDesktop will now close.",
                "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown();
        }


        private void writeException(Exception e)
        {
            string logFileName = Path.Combine(PnFDesktopConfig.ErrorLogPath, Path.GetRandomFileName() + ".err");
            StringBuilder sb = new StringBuilder();
            writeException(sb, e);
            using (TextWriter logWriter = new StreamWriter(logFileName))
            {
                logWriter.Write(sb.ToString());
                logWriter.Close();
            }


        }

        /// <summary>
        /// Pushes exception details into a StringBuilder recursively as long as InnerExceptions exist.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="e"></param>
        private void writeException(StringBuilder sb, Exception e)
        {
            sb.AppendLine(e.GetType().Name);
            if (!string.IsNullOrWhiteSpace(e.Message))
            {
                sb.AppendLine("Exception: " + e.Message);
            }

            if (!string.IsNullOrWhiteSpace(e.Source))
            {
                sb.AppendLine("Source: " + e.Source);
            }

            if (!string.IsNullOrWhiteSpace(e.StackTrace))
            {
                sb.AppendLine("Stack Trace");
                sb.AppendLine(e.StackTrace);
            }

            if (e.InnerException != null)
            {
                sb.AppendLine("Inner Exception");
                writeException(sb, e.InnerException);
            }
        }

        #endregion

    }

}
