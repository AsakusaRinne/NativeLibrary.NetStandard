using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NativeLibraryNetStandard
{
    /// <summary>
    /// A class which contains method for loading native library.
    /// </summary>
    public static class NativeLibrary
    {
        private static readonly NativeLibraryLoader s_platformDefaultLoader = NativeLibraryLoader.GetPlatformDefaultLoader();

        /// <summary>
        /// Try to load the library.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="handle">The handle of the native library. It is <see cref="IntPtr.Zero"/> if the loading fails.</param>
        /// <param name="freeResult">
        /// Whether the result Intptr should be free. 
        /// If set to false, the user is responsible for releasing it via <see cref="FreeNativeLibraryHandle(IntPtr)"/>
        /// </param>
        /// <returns>Whether the native library is loaded successfully.</returns>
        public static bool TryLoad(string filename, out IntPtr handle, bool freeResult = false)
        {
            var library = new NativeLibraryHolder(filename, autoFree:freeResult);
            handle = library.Handle;
            var error = Marshal.GetLastWin32Error();
            //library.Dispose();
            return library.Handle != IntPtr.Zero;
        }

        /// <summary>
        /// Free the loaded native library handle.
        /// </summary>
        /// <param name="handle"></param>
        public static void FreeNativeLibraryHandle(IntPtr handle)
        {
            s_platformDefaultLoader.FreeNativeLibraryHandle(handle);
        }
    }
}
