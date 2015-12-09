namespace MediaCurator.Common
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// These settings are shared among all the other projects.
    /// </summary>
    public static class CommonSettings
    {
        private static string rootDataDir;

        /// <summary>
        /// Gets RootDataDir. The directory where the database and other user files are stored. It is under SpecialFolder.CommonApplicationData, the same for all user accounts. This path is hard-coded in the installator, it is used to clean up the data during uninstall.
        /// </summary>
        public static string RootDataDir
        {
            get
            {
                return string.IsNullOrEmpty(rootDataDir)
                           ? rootDataDir =
                             Path.Combine(
                                 Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                 "Dignicom",
                                 "Media Curator")
                           : rootDataDir;
            }
        }

        public const string DatabaseFileName = "MediaCurator.data";

        public static string DatabaseFilePath
        {
            get
            {
                return Path.Combine(RootDataDir, DatabaseFileName);
            }
        }

        public static bool IsWin7OrAbove
        {
            get
            {
                var os = System.Environment.OSVersion;
                return (os.Version >= new Version(6, 1));
            }
        }

        public static string NativeDLLDir = (Marshal.SizeOf(typeof(IntPtr)) == 4) ? "32" : "64";


    }
}
