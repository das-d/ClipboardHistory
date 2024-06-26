﻿using System.Configuration;
using System.Data;
using System.Windows;

namespace ClipboardHistory
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    { 
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetupExceptionHandling();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            MessageBox.Show($"{exception}", "Unhandled Exception caught");
            //try
            //{
            //    System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            //    message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(ex, "Exception in LogUnhandledException");
            //}
            //finally
            //{
            //    _logger.Error(exception, message);
            //}
        }
    }

}
