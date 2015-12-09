using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaCurator.Common;
using System.Diagnostics;

namespace MediaCurator.Server
{
    public class Listener
    {
        private HttpListener listener;
        private Task task;
        private CancellationTokenSource cancelSource;

        public const string UrlPrefix = "http://+:80/mediacurator/";

        public Listener()
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(UrlPrefix);

            // Use synchronous processing model
            cancelSource = new CancellationTokenSource();
            task = new Task(() => Listen(cancelSource.Token), TaskCreationOptions.LongRunning);
        }

        public void Start()
        {
            try
            {
                this.listener.Start();
                this.listener.IgnoreWriteExceptions = true;
                task.Start();
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 5)  // Access denied
                {
                    // Run cmd.exe as Administrator:
                    // netsh http show urlacl
                    // netsh http delete urlacl url=http://+:80/mediacurator/
                    // netsh http add urlacl url=http://+:80/mediacurator/ sddl=D:(A;;GX;;;WD)(A;;GX;;;LS)
                    // In the SSDL above WD means "\Everyone" and LS means "NT AUTHORITY\LOCAL SERVICE"
                    // In SDDL D:(A;;GX;;;BU)(A;;GX;;;NS) BU means "BUILTIN\Users" and NS means "NT AUTHORITY\NETWORK SERVICE" 
                    // +http://social.msdn.microsoft.com/Forums/en/roboticsdss/thread/3b496d55-cd29-4bb4-aeb3-3269068dc8d8
                    // +http://urlreservation.codeplex.com/
                }
                Trace.TraceError(ex.Message);
            }
        }

        public void Stop()
        {
            cancelSource.Cancel();
            this.listener.Close();
            task.Wait(TimeSpan.FromSeconds(2));
        }

        private void Listen(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    if (this.listener.IsListening)
                    {
                        // HttpListener.GetContext() blocks execution. We use synchronous processing model.
                        HttpListenerContext context = this.listener.GetContext();
                        // We expect requests from the local computer only.
                        if (context.Request.IsLocal)
                        {
                            // Process request on a separate thread in the CLR thread pool.
                            Task.Factory.StartNew(() => RequestHandler.ProcessRequest(context, cancelToken));
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode != 995)  // 995 = "The I/O operation has been aborted because of either a thread exit or an application request."
                {
                    Trace.TraceError(ex.Message);
                    throw ex;
                }
            }
        }

    }
}
