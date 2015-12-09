using System.Windows.Forms;
using System;
using MediaCurator.Controller;

namespace MediaCurator
{
    public partial class HiddenForm : System.Windows.Forms.Form
    {
        private PowerMonitor powerMonitor;

        public PowerMonitor PowerMonitor
        {
            set
            {
                this.powerMonitor = value;
                this.powerMonitor.RegisterForNotifications(this.Handle);
                /* powerMonitor is disposed in HiddenForm.Dispose() in HiddenForm.Designer.cs */
            }
        }

        public HiddenForm()
        {
            InitializeComponent();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }
            base.SetVisibleCore(false);
        }

        /* This form's window procedure is also used for receiving power notification messages. */
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
