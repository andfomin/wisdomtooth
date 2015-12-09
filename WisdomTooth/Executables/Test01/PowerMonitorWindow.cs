using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaCurator.Controller;

namespace Test01
{
    public class PowerMonitorWindow : System.Windows.Forms.Form
    {
        private static PowerMonitorWindow window;

        private PowerMonitor powerMonitor;

        public static void Initialize(PowerMonitor powerMonitor)
        {
            if (window != null)
            {
                throw new Exception();
            }

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                Task.Factory.StartNew(
                     () =>
                     {
                         window = new PowerMonitorWindow();
                         window.powerMonitor = powerMonitor;
                         window.powerMonitor.RegisterForNotifications(window.Handle);
                         mre.Set();
                         Application.Run();
                         window.powerMonitor.Dispose();
                         window.Dispose();
                         window = null;
                     },
                     TaskCreationOptions.LongRunning);

                mre.WaitOne();
            }
        }

        public static void Terminate()
        {
            if (window != null)
            {
                Application.Exit();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == PowerMonitor.WM_POWERBROADCAST && this.powerMonitor != null)
            {
                this.powerMonitor.PowerBroadcastMessageHandler(m.Msg, m.WParam, m.LParam);
            }
        }
    }
}
