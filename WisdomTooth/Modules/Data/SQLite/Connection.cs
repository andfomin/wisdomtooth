using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections.Concurrent;
using MediaCurator.Common;
using System.Threading;

namespace MediaCurator.Data.SQLite
{
    /// <summary>
    /// The parent SafeHandle class contains a finalizer that ensures that the handle is closed.
    /// To dispose unmanaged resourses for all connections, call CloseAll() manually.
    /// </summary>
    public class Connection : System.Runtime.InteropServices.SafeHandle
    {
        /// <summary>
        /// Used to make sure only one thread can perform critical actions.
        /// </summary>
        private static readonly object LockObject = new object();

        private static List<Connection> Connections = new List<Connection>();

        private Transaction transaction;
        private Dictionary<string, Statement> StatementCache;

        public Documents Documents { get; private set; }

        public Connection(string filePath)
            : this(filePath, false, false)
        {
        }

        public Connection(string filePath, bool readOnly, bool create)
            : base(IntPtr.Zero, true)
        {
            StatementCache = new Dictionary<string, Statement>();
            Documents = new Documents(this);

            lock (LockObject)
            {
                /* Calling sqlite3_initialize() after the library has already been initialized is harmless. */
                int error = SQLite.sqlite3_initialize();
                NoError(error, "5122824", true);

                int flags = (readOnly ? SQLite.SQLITE_OPEN_READONLY : SQLite.SQLITE_OPEN_READWRITE)
                            | (create ? SQLite.SQLITE_OPEN_CREATE : 0);

                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    SetHandle(IntPtr.Zero);
                }
                finally
                {
                    /* This is the code that we want in a constrained execution region. We need to avoid the situation where sqlite3_open_v2 is called but the handle isn't set, so the instance is never terminated. This would happen, for example, if there was a ThreadAbortException between the call to sqlite3_open_v2 and the call to SetHandle.
                     * A database connection handle is usually returned in ppDb, even if an error occurs. Whether or not an error occurs when it is opened, resources associated with the database connection handle should be released by passing it to sqlite3_close(). */
                    error = SQLite.sqlite3_open_v2(Encoding.UTF8.GetBytes(filePath), out this.handle, flags, IntPtr.Zero);
                    SetHandle(handle);
                    NoError(error, "5123142", true);
                }

                Connections.Add(this);

                /* Write operations are much much faster with synchronous OFF. If the application running SQLite crashes, the data will be safe, but the database might become corrupted if the operating system crashes or the computer loses power before that data has been written to the disk surface. */

                using (var statement = new Statement(this, "PRAGMA synchronous = OFF;"))
                {
                    statement.ExecuteConcurrently();
                }
            }
        }

        private void InternalClose()
        {
            lock (LockObject)
            {
                if (!IsInvalid)
                {
                    try
                    {
                        if (this.transaction != null)
                        {
                            this.transaction.Dispose();
                            this.transaction = null;
                        }
                        Documents = null;
                        /* Use a proxy list. Statemnts remove themselves from the original list; this may affect the iterator. */
                        this.StatementCache.Values.ToList().ForEach(i => i.Dispose());
                    }
                    finally
                    {
                        /* Use a constrained region so that the handle is always set as invalid after JetTerm is called. */
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                        }
                        finally
                        {
                            /* This is the code that we want in a constrained execution region. We need to avoid the situation where sqlite3_close is called but the handle isn't invalidated, so the connection is closed again. This would happen, for example, if there was a ThreadAbortException between the call to sqlite3_close and the call to SetHandle. */
                            /* Any pointer returned by a call to sqlite3_open_xxx(), including a NULL pointer, can be passed to sqlite3_close() If the database still has nonfinalized statements, the SQLITE_BUSY error will be returned. In that case,
you need to correct the problem and call sqlite3_close() again. */
                            int error = SQLite.sqlite3_close(this.handle);
                            this.SetHandle(IntPtr.Zero);
                            this.SetHandleAsInvalid();
                            NoError(error, "5141075", false);
                        }

                        Connections.Remove(this);
                    }
                }
            }
        }

        /// <summary>
        /// Free resources manually in deterministic order.
        /// </summary>
        public static void CloseAll()
        {
            lock (LockObject)
            {
                // Use a proxy list, because each Connection deletes itself from the list and affects the enumerator. 
                Connections.ToList().ForEach(i => i.InternalClose());
                /* sqlite3_shutdown() must only be called from a single thread. */
                SQLite.sqlite3_shutdown();
            }
        }

        internal bool NoError(int error, string message, bool throwException)
        {
            var result = (error == SQLite.SQLITE_OK || error == SQLite.SQLITE_DONE);
            if (!result)
            {
                int errcode = 0;
                string errmsg = null;
                if (!IsInvalid)
                {
                    errcode = SQLite.sqlite3_extended_errcode(this.handle);
                    errmsg = SQLite.sqlite3_errmsg(this.handle);
                }

                if (throwException)
                {
                    throw new MediaCurator.Common.MediaCuratorException("{0}: {1} {2} {3}. ", message, error, errcode, errmsg);
                }
                else
                {
                    Debug.WriteLine(string.Format("{0}: {1} {2} {3}.", message, error, errcode, errmsg));
                }
            }
            return result;
        }

        public override bool IsInvalid
        {
            get
            {
                return this.handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// This method is guaranteed to be called by the garbage collector only once and only if the handle is valid.
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            try
            {
                InternalClose();
            }
            catch (Exception)
            {
                // Do not disturb the finalizer thread.
            }
            return IsInvalid;
        }

        /// <summary>
        /// Provide implicit conversion of an Connection object to a IntPtr handle. 
        /// This is done so that an Connection can be used anywhere a database handle is required.
        /// </summary>
        /// <param name="esentTable">The Connection to convert.</param>
        /// <returns>The IntPtr wrapped by the Connection.</returns>
        public static implicit operator IntPtr(Connection connection)
        {
            return connection.handle;
        }

        public static bool DatabaseExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void CreateDatabase(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                throw new ArgumentException("51730166");
            }

            if (DatabaseExists(filePath))
            {
                throw new ArgumentException("51740191");
            }

            using (var connection = new Connection(filePath, false, true))
            {
                connection.ExecuteStatementOnce("PRAGMA journal_mode = WAL;", "41159106");
            }
        }

        internal Statement GetStatement(string sql)
        {
            if (this.StatementCache.ContainsKey(sql))
            {
                var statement = this.StatementCache[sql];
                statement.ResetAndClearBindings();
                return statement;
            }
            else
            {
                return new Statement(this, sql);  // It will call AddStatement() in the ctor
            }
        }

        internal void AddStatement(Statement statement)
        {
            StatementCache.Add(statement.Sql, statement);
        }

        internal void RemoveStatement(Statement statement)
        {
            StatementCache.Remove(statement.Sql);
        }

        public void ExecuteStatementOnce(string sql, string exceptionMessage)
        {
            using (var statement = new Statement(this, sql))
            {
                var error = SQLite.sqlite3_step(statement);
                if (!(error == SQLite.SQLITE_ROW || error == SQLite.SQLITE_DONE))
                {
                    NoError(error, exceptionMessage, true);
                }
            }
        }

        /// <summary>
        /// Checks correspondence between the code schema and the database schema.
        /// </summary>
        /// <param name="enforce">If the initial schema is wrong, makes an attempt to recreate the table.</param>
        /// <returns>True if the schema is OK</returns>
        public bool IsSchemaOk(bool enforce)
        {
            return Documents.IsSchemaOk(enforce);
        }

        internal void AddTransaction(Transaction transaction)
        {
            if (transaction != null && this.transaction != null)
            {
                throw new MediaCuratorException("5222259");
            }
            this.transaction = transaction;
        }

        internal void RemoveTransaction(Transaction transaction)
        {
            if (this.transaction != transaction)
            {
                throw new MediaCuratorException("52229274");
            }
            this.transaction = null;
        }

    }
}
