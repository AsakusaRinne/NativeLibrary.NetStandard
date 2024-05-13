using System;
using System.Runtime.InteropServices;

namespace NativeLibraryNetStandard
{
    internal static class Kernel32
    {
        /// <summary>
        /// seee https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-loadlibraryw
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary(string fileName);
        /// <summary>
        /// see http://www.pinvoke.net/default.aspx/kernel32.getprocaddress
        /// </summary>
        /// <param name="module"></param>
        /// <param name="procName"></param>
        /// <returns></returns>

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(IntPtr module, string procName);

        [DllImport("kernel32", SetLastError = true)]
        public static extern int FreeLibrary(IntPtr module);

        [DllImport("kernel32", SetLastError=true)]
        public static extern bool SetDllDirectory(string fileName);
    }
}
