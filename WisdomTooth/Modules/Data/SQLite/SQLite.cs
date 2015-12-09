using System;
using System.Text;
using System.Runtime.InteropServices;

namespace MediaCurator.Data.SQLite
{
    public static class SQLite
    {
        #region /* DLL loading */
        //private const string SQLITE_DLL = "sqlite3.dll";
        private const string SQLITE_DLL = "SQLite.Interop.dll";

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDllDirectory(string lpPathName);

        #endregion

        #region /* Error codes */
        internal const int SQLITE_OK = 0;  /* Successful result */
        internal const int SQLITE_BUSY = 5;   /* The database file is locked */
        internal const int SQLITE_ROW = 100;  /* sqlite3_step() has another row ready */
        internal const int SQLITE_DONE = 101; /* sqlite3_step() has finished executing */

        #endregion

        #region /* Flags */

        /* Flags for sqlite3_open_v2 */
        internal const int SQLITE_OPEN_READONLY = 0x00000001;
        internal const int SQLITE_OPEN_READWRITE = 0x00000002;
        internal const int SQLITE_OPEN_CREATE = 0x00000004;

        /**/
        internal const int SQLITE_STATIC = 0;
        internal const int SQLITE_TRANSIENT = -1;

        #endregion

        #region /* Functions */
        /// <summary>
        /// Extended results are always returned.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_extended_errcode(IntPtr db);

        /// <summary>
        /// Returns a null-terminated, English language error string that is encoded in UTF-8. Memory to hold the error message string is managed internally. The application does not need to worry about freeing the result.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string sqlite3_errmsg(IntPtr db);

        /// <summary>
        /// Calling this function after the library has already been initialized is harmless. There is autoinit. Future releases of SQLite may require explicit initialization.
        /// </summary>
        /// <returns></returns>
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_initialize();

        /// <summary>
        /// Calling this function before the library has been initialized or after the library has already been shut down is harmless.
        /// </summary>
        /// <returns></returns>
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_shutdown();

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_open_v2(byte[] utf8Filename, out IntPtr ppDb, int flags, IntPtr zVfs);

        /// <summary>
        /// Closes a database connection and releases any associated data structures. All temporary items associated with this connection will be deleted. In order to succeed, all prepared statements associated with this database connection must be finalized.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_close(IntPtr db);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_busy_timeout(IntPtr db, int millisec);

        /* Statements */

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern int sqlite3_prepare16_v2(IntPtr db, string sqlStr, int sqlStrLen, out IntPtr stmt, IntPtr tail);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <param name="pidx">Bind index values start with one (1)</param>
        /// <param name="data">Buffer and its contents must remain valid until a new value is bound to that parameter, or the statement is finalized.</param>
        /// <param name="dataLen"></param>
        /// <param name="memCallback"></param>
        /// <returns></returns>
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_blob(IntPtr stmt, int pidx, IntPtr data, int dataLen, IntPtr memCallback);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_int(IntPtr stmt, int pidx, int data);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long sqlite3_last_insert_rowid(IntPtr db);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_step(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_blob(IntPtr stmt, int cidx);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_bytes(IntPtr stmt, int cidx);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_bytes16(IntPtr stmt, int cidx);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_int(IntPtr stmt, int cidx);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_text16(IntPtr stmt, int cidx);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_reset(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_clear_bindings(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_finalize(IntPtr stmt);

        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_changes(IntPtr db);

        #endregion

        public static string UTF16ToString(IntPtr buffer)
        {
            if (buffer == IntPtr.Zero)
            {
                return "";
            }
            return Marshal.PtrToStringUni(buffer);
        }


    }
}
