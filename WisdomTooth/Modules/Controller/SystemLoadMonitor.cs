using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediaCurator.Common;
using System.Activities;

namespace MediaCurator.Controller
{
    /// <summary>
    /// Implements Singleton design pattern
    /// +http://msdn.microsoft.com/en-us/library/ff650316.aspx
    /// </summary>
    public class SystemLoadMonitor : CodeActivity
    {
        private static volatile Task task;
        private static CancellationTokenSource cancelSource;
        private static ManualResetEvent canAddLoadEvent;
        private static object syncRoot = new Object();

        private TimeSpan defaultTimeout = TimeSpan.FromSeconds(5);
        public InArgument<TimeSpan> Timeout { get; set; }

        private static void EnsureInstance()
        {
            if (task == null)
            {
                lock (syncRoot)
                {
                    if (task == null)
                    {
                        canAddLoadEvent = new ManualResetEvent(false);
                        cancelSource = new CancellationTokenSource();
                        task = new Task(MonitorLoad, cancelSource.Token, TaskCreationOptions.LongRunning);
                        task.Start();
                    }
                }
            }
        }

        public static void Cancel()
        {
            EnsureInstance();
            cancelSource.Cancel();
            task.Wait(TimeSpan.FromSeconds(2));
            // Release waiting threads on exit.
            canAddLoadEvent.Set();
            Debug.Write("SystemLoadMonitor status: " + task.Status.ToString());
        }

        private static void MonitorLoad()
        {
            const int loadThreshold = 80;
            const int maxLoad = 100;
            const int circularBufferLength = 5;
            const int performanceCounterInterval = 1000; // msec. Matches task manager reading. Do not change.

            try
            {
               Debug.Write("SystemLoadMonitor started.");

                using (PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    // Circular buffer. Stick initially to the maximum value.
                    float[] values = Enumerable.Repeat<float>(maxLoad, circularBufferLength).ToArray();
                    int position = 0;

                    while (!cancelSource.Token.IsCancellationRequested)
                    {
                        position++;
                        if (position >= values.Length)
                        {
                            position = 0;
                        }

                        cpuCounter.NextValue();
                        Thread.Sleep(performanceCounterInterval);
                        // Now matches task manager value.    
                        values[position] = cpuCounter.NextValue();

                        bool canAddLoad = values.Average() < loadThreshold;

                        // If millisecondsTimeout is zero, WaitOne does not block. It tests the state of the wait handle and returns immediately. 
                        if (canAddLoad != canAddLoadEvent.WaitOne(0))
                        {
                            if (canAddLoad)
                            {
                                canAddLoadEvent.Set();
                            }
                            else
                            {
                                canAddLoadEvent.Reset();
                            }
                        }
                    }
                }
            }
            finally
            {
                // Release waiting threads on exit.
                canAddLoadEvent.Set();
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            EnsureInstance();

            // If the value of Timeout parameter is not specified, returns "00:00:00".
            TimeSpan timeout = this.Timeout.Get(context);
            if ((timeout == default(TimeSpan)) && this.Timeout.Expression == null)
            {
                timeout = defaultTimeout;
            }

            canAddLoadEvent.WaitOne(timeout);
        }
    }
}
