using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaCurator.Controller;

namespace MediaCurator
{
    static class Program
    {
        private static MediaCurator.Controller.Controller controller;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Trace.TraceInformation("Application is starting.");

            controller = new MediaCurator.Controller.Controller();
            /* Controller.Start() uses a separate thread to start components, however it is synchronous. */
            controller.Start();

            // We need an invisible window wich will receive a message from Windows about shutdown.
            var form = new HiddenForm();

            var powerMonitor = new PowerMonitor();
            /* powerMonitor is disposed in HiddenForm.Dispose() in HiddenForm.Designer.cs */
            form.PowerMonitor = powerMonitor;

            // Handle the ApplicationExit event to know when the application is exiting.
            Application.ApplicationExit += OnApplicationExit;

            /* Application.Run() will block execution on this thread processing Windows messages until the main form receives the message to quit. */
            Application.Run(form);
        }

        ////private static void OnTimerElapsed(Object sender, TimerElapsedEventArgs e)
        ////{
        ////    SystemEvents.TimerElapsed -= OnTimerElapsed;
        ////    Trace.TraceInformation("Application is starting.");
        ////    controller.Start();
        ////}

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            //// How to stop by itself. System.Windows.Forms.Application.Exit();
            Trace.TraceInformation("Application is exiting.");

            // Because this is a static event, detach event handlers to prevent memory leaks when the application is disposed.
            Application.ApplicationExit -= OnApplicationExit;            

            /* Controller.Stop() uses a separate thread to stop components, however it is synchronous. */
            controller.Stop();
        }
    }
}
