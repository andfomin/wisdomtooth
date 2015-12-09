using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MediaCurator.Common;
using MediaCurator.Data;
using MediaCurator.Server;
using MediaCurator.Processor;
using MediaCurator.Controller;
using MediaCurator.Data.SQLite;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;

namespace Test01
{
    class Program
    {
        private static Controller controller;

        static void Main(string[] args)
        {
            string nativeDllPath = Path.Combine(Directory.GetCurrentDirectory(), CommonSettings.NativeDLLDir);
            SQLite.SetDllDirectory(nativeDllPath);

            controller = new Controller();
            //controller.Start();

            DoIt3();
            RunWriteAndDelete();
            Thread.Sleep(100);
            RunWriteAndDelete();
            Thread.Sleep(100);
            RunWriteAndDelete();

            Console.WriteLine("h - harvest feeds. p - start power monitor. Esc - quit.");

            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(false);
                Console.WriteLine("");
                switch (cki.KeyChar)
                {
                    case 'h':
                        controller.HarvestFeeds();
                        break;
                    case 'p':
                        var powerMonitor = new PowerMonitor();
                        PowerMonitorWindow.Initialize(powerMonitor);
                        break;
                    ////case 'p':
                    ////    ////ProcessQueue();
                    ////    break;
                }
            } while (cki.Key != ConsoleKey.Escape);

            ////PowerMonitorWindow.Terminate();
            controller.Stop();

            ////Instance.DisposeAll();
            //Connection.CloseAll();
        }

        private static void DoIt()
        {
            ////try
            ////{
            ////    var instance = new Instance(CommonSettings.RootDataDir, CommonSettings.BaseName);

            ////    if (!instance.DatabaseExists(CommonSettings.Database))
            ////    {
            ////        instance.CreateDatabase(CommonSettings.Database);
            ////    }

            ////    /* Opening and closing connections to a ESENT database is a costly operation, and there is no concept of Connection Pooling with ESENT. A way to mimic a connection pool is to keep a dummy connection (that is not otherwise used) open for the duration of the application’s lifetime. */
            ////    Connection idleConnection = new Connection(Instance.Default, CommonSettings.Database);
            ////    idleConnection.EnforceDatabaseSchema();

            ////    ////var connection = new Connection(Instance.Default, CommonSettings.Database);
            ////    ////var table = new Documents(connection, false);
            ////    ////bool schemaOk = table.IsSchemaOk(true);
            ////    ////Console.WriteLine("Schema of table {0} is Ok: {1}", table.TableName, schemaOk);

            ////}
            ////catch (Exception ex)
            ////{
            ////    Console.WriteLine(ex.Message);
            ////}
        }


        private static void DoIt3()
        {
            string filePath = CommonSettings.DatabaseFilePath;

            try
            {
                if (!Connection.DatabaseExists(filePath))
                {
                    Connection.CreateDatabase(filePath);
                }

                /* Opening and closing connections to a SQLite database is a costly operation, because it creates a WAL file and maps it to shared memory, then deletes it after the connection closed. There is no concept of Connection Pooling with SQLite. A way to mimic a connection pool is to keep a dummy connection (that is not otherwise used) open for the duration of the application’s lifetime. It prevents resource leak by implementing SafeHandle. A connection does not do anything until it is being used by a statement. */
                var idleConnection = new Connection(filePath);
                bool schemaOk = idleConnection.IsSchemaOk(true);
                Console.WriteLine("Schema of table {0} is Ok: {1}", idleConnection.Documents.TableName, schemaOk);

                //using (var connection = new Connection(filePath))
                //{
                //    bool schemaOk = connection.Documents.IsSchemaOk(true);

                //  //connection.Documents.Delete(2);

                //    var doc = new BrowserHelperMessageDocument
                //    {
                //        DocumentId = Guid.Empty,
                //        MessageType = Guid.Empty,
                //        Message = "qwe",
                //    };
                //  //connection.Documents.Write(doc);


                //  //var doc1 =  connection.Documents.Read<BrowserHelperMessageDocument>(2);
                //  //doc1.Message = "qwer";
                //  //connection.Documents.Write(doc1);
                //}

                //                idleConnection.ExecuteStatement("SELECT Id FROM [Documents] WHERE Id = 0", "51542118");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static void RunWriteAndDelete()
        {
            Task.Factory.StartNew(() =>
                {
                    WriteAndDelete();
                }
            );
        }

        private static void WriteAndDelete()
        {
            var thread = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Thread {0}", thread);

            Stopwatch sw = new Stopwatch();
            var ids = new List<int>();

            try
            {
                using (var connection = new Connection(CommonSettings.DatabaseFilePath))
                {
                    for (int i = 0; i < 4; i++)
                    {

                        sw.Start();
                        for (int j = 0; j < 1000; j++)
                        {
                            var doc = new BrowserHelperMessageDocument
                            {
                                DocumentId = Guid.Empty,
                                MessageType = Guid.Empty,
                                Message = "qwe",
                            };
                            connection.Documents.Write(doc);
                            ids.Add((int)doc.Id);
                            connection.Documents.Write(doc);
                        }
                        sw.Stop();
                        Console.WriteLine("{0}. Write. {1}", thread, sw.Elapsed.ToString());

                        sw.Reset();
                        sw.Start();
                        var ids2 = ids.Select(id => (int)connection.Documents.Read<BrowserHelperMessageDocument>(id).Id).ToList();
                        ids.Clear();
                        sw.Stop();
                        Console.WriteLine("{0}. Read. {1}", thread, sw.Elapsed.ToString());

                        sw.Reset();
                        sw.Start();
                        ids2.ForEach(id => connection.Documents.Delete(id));
                        ids2.Clear();
                        sw.Stop();
                        Console.WriteLine("{0}. Delete. {1}", thread, sw.Elapsed.ToString());
                    }
                }

                Console.WriteLine("Thread {0} finished.", thread);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
