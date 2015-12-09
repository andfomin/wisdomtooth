namespace MediaCurator.Common
{
    using System;

    /// <summary>
    /// File time to DateTime conversion.
    /// </summary>
    public static class FILETIMEExtensions
    {
        public static DateTime ToDateTime(this System.Runtime.InteropServices.ComTypes.FILETIME filetime)
        {
            // Based on +http://stackoverflow.com/questions/724148/is-there-a-faster-way-to-scan-through-a-directory-recursively-in-net/
            long highBits = filetime.dwHighDateTime;
            highBits = highBits << 32;
            return DateTime.FromFileTimeUtc(highBits + (long)filetime.dwLowDateTime);
        }
    }
}
