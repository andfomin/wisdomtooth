using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using MediaCurator.Common;
using System.Diagnostics;
using MediaCurator.Server;
using System.IO;
using MediaCurator.Data;
using System.Threading.Tasks;
using MediaCurator.Processor;
using System.Threading;
using MediaCurator.Data.SQLite;

namespace MediaCurator.Controller
{
    public class Controller
    {
        private const string EventLogSourceName = "Media Curator";

        private Listener listener;

        public void Start()
        {
            InitializeTraceListeners();
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        InitializeDatabase();
                        InitializeServer();
                        Trace.TraceInformation("Controller initialized.");
                        mre.Set();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(MediaCuratorException.ExceptionMessage(ex));
                    }
                });
                mre.WaitOne();
            }
        }

        public void Stop()
        {
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (this.listener != null)
                            this.listener.Stop();

                        Connection.CloseAll();
                        Indexer.CloseWriters();

                        mre.Set();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(MediaCuratorException.ExceptionMessage(ex));
                    }
                });
                mre.WaitOne();
            }
        }

        private void InitializeTraceListeners()
        {
            Trace.Listeners.Clear();
            Debug.Listeners.Clear();

            if (EventLog.SourceExists(EventLogSourceName))
            {
                Trace.Listeners.Add(new EventLogTraceListener(EventLogSourceName));
            }
#if DEBUG
            Debug.Listeners.Add(new ConsoleTraceListener());
#else
#endif
            ////if (EventLog.SourceExists(EventLogSourceName))
            ////{
            ////    // Apparently, it gets bounded to the Application log by default.
            ////    EventLog eventLog = new EventLog();
            ////    eventLog.Source = EventLogSourceName;
            ////    var q = from EventLogEntry e in eventLog.Entries
            ////            where (e.Source == EventLogSourceName)
            ////            select e;
            ////}

            ////string rootDataDir = CommonSettings.RootDataDir;
            ////if (!Directory.Exists(rootDataDir))
            ////{
            ////    Directory.CreateDirectory(rootDataDir);
            ////}
            ////string logFile = Path.Combine(rootDataDir, "trace.txt");
            ////TextWriterTraceListener logFileListener = new TextWriterTraceListener(logFile);
            ////Debug.Listeners.Add(logFileListener);
            ////Debug.AutoFlush = true;
        }

        private void InitializeDatabase()
        {
            ////string databasePath = Path.Combine(CommonSettings.RootDataDir, DatabaseFileName);
            ////Database.Databases[DatabaseAlias] = databasePath;
            ////if (!Database.DatabaseExists(DatabaseAlias))
            ////{
            ////    Database.CreateDatabase(DatabaseAlias);
            ////}
            ////Database.OpenIdleConnection();
            ////Database.EnforceSchema(DatabaseAlias);

            ////var instance = new Instance(CommonSettings.RootDataDir, CommonSettings.BaseName);
            ////if (!instance.DatabaseExists(CommonSettings.Database))
            ////{
            ////    instance.CreateDatabase(CommonSettings.Database);
            ////}
            /* Opening and closing connections to a ESENT database is a costly operation, and there is no concept of Connection Pooling with ESENT. A way to mimic a connection pool is to keep a dummy connection (that is not otherwise used) open for the duration of the application’s lifetime. It will be closed on the instance disposal. */
            ////Connection idleConnection = new Connection(Instance.Default, CommonSettings.Database);
            ////idleConnection.EnforceDatabaseSchema();

            string filePath = CommonSettings.DatabaseFilePath;

            if (!Connection.DatabaseExists(filePath))
            {
                Connection.CreateDatabase(filePath);
            }

            /* Opening and closing connections to a SQLite database is a costly operation, because it creates a WAL file and maps it to shared memory, then deletes it after the connection closed. There is no concept of Connection Pooling with SQLite. A way to mimic a connection pool is to keep a dummy connection (that is not otherwise used) open for the duration of the application’s lifetime. It prevents resource leak by implementing SafeHandle. A connection does not do anything until it is being used by a statement. */
            var idleConnection = new Connection(filePath);
            idleConnection.IsSchemaOk(true);
        }

        private void InitializeServer()
        {
            this.listener = new Listener();
            this.listener.Start();
        }

        private void CheckForUpdate()
        {
            string rootDataDir = CommonSettings.RootDataDir;

            // Simple "singleton" :)
            IEnumerable<string> files = Directory.EnumerateFiles(rootDataDir, "*.msi", SearchOption.AllDirectories);
            if (files.Any())
                return;

            UpdateDownloader updateDownloader = new UpdateDownloader();
            string url = updateDownloader.GetDownloadUrl();

            if (!string.IsNullOrEmpty(url))
            {
                Uri uri = new Uri(url);
                string fileName = Path.GetFileName(uri.LocalPath);

                string filePath = Path.Combine(rootDataDir, Guid.NewGuid().ToString(), fileName);
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                updateDownloader.IinitiateDownload(url, filePath);
            }
        }

        public void HarvestFeeds()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    FeedHarvester processor = new FeedHarvester();
                    processor.Process();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(MediaCuratorException.ExceptionMessage(ex));
                }
            });
        }

    }
}
