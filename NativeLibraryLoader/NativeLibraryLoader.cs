using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NativeLibraryNetStandard
{
    /// <summary>
    /// Exposes functionality for loading native libraries and function pointers.
    /// </summary>
    public abstract class NativeLibraryLoader
    {
        /// <summary>
        /// Loads a native library by name and returns an operating system handle to it.
        /// </summary>
        /// <param name="name">The name of the library to open.</param>
        /// <returns>The operating system handle for the shared library.</returns>
        public IntPtr LoadNativeLibrary(string name)
        {
            return LoadNativeLibrary(name, new EmptyPathResolver());
        }

        /// <summary>
        /// Loads a native library by name and returns an operating system handle to it.
        /// </summary>
        /// <param name="names">An ordered list of names. Each name is tried in turn, until the library is successfully loaded.
        /// </param>
        /// <returns>The operating system handle for the shared library.</returns>
        public IntPtr LoadNativeLibrary(string[] names)
        {
            return LoadNativeLibrary(names, new EmptyPathResolver());
        }

        /// <summary>
        /// Loads a native library by name and returns an operating system handle to it.
        /// </summary>
        /// <param name="name">The name of the library to open.</param>
        /// <param name="pathResolver">The path resolver to use.</param>
        /// <returns>The operating system handle for the shared library.</returns>
        public IntPtr LoadNativeLibrary(string name, IPathResolver pathResolver)
        {
            if (string.IsNullOrEmpty(name))
            {
                return IntPtr.Zero;
            }

            IntPtr ret = LoadWithResolver(name, pathResolver);

            return ret;
        }

        /// <summary>
        /// Loads a native library by name and returns an operating system handle to it.
        /// </summary>
        /// <param name="names">An ordered list of names. Each name is tried in turn, until the library is successfully loaded.
        /// </param>
        /// <param name="pathResolver">The path resolver to use.</param>
        /// <returns>The operating system handle for the shared library.</returns>
        public IntPtr LoadNativeLibrary(string[] names, IPathResolver pathResolver)
        {
            if (names is null || names.Length == 0)
            {
                return IntPtr.Zero;
            }

            IntPtr ret = IntPtr.Zero;
            foreach (string name in names)
            {
                ret = LoadWithResolver(name, pathResolver);
                if (ret != IntPtr.Zero)
                {
                    return ret;
                }
            }

            return IntPtr.Zero;
        }

        private IntPtr LoadWithResolver(string name, IPathResolver pathResolver)
        {
            if (Path.IsPathRooted(name))
            {
                name = @"D:\development\llama\native\LLamaSharp\NetStandardTest\bin\Debug\runtimes\win-x64\native\cuda11\llama.dll";
                var res =  CoreLoadNativeLibrary(name);
                var error = Marshal.GetLastWin32Error();
                return res;
            }
            else
            {
                foreach (string loadTarget in pathResolver.EnumeratePossibleLibraryLoadTargets(name))
                {
                    if (!Path.IsPathRooted(loadTarget) || File.Exists(loadTarget))
                    {
                        IntPtr ret = CoreLoadNativeLibrary(loadTarget);
                        if (ret != IntPtr.Zero)
                        {
                            return ret;
                        }
                    }
                }

                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Loads a function pointer out of the given library by name.
        /// </summary>
        /// <param name="handle">The operating system handle of the opened shared library.</param>
        /// <param name="functionName">The name of the exported function to load.</param>
        /// <returns>A pointer to the loaded function.</returns>
        public IntPtr LoadFunctionPointer(IntPtr handle, string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
            {
                throw new ArgumentException("Parameter must not be null or empty.", nameof(functionName));
            }

            return CoreLoadFunctionPointer(handle, functionName);
        }

        /// <summary>
        /// Frees the library represented by the given operating system handle.
        /// </summary>
        /// <param name="handle">The handle of the open shared library.</param>
        public void FreeNativeLibraryHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException("Parameter must not be zero.", nameof(handle));
            }

            CoreFreeNativeLibrary(handle);
        }

        /// <summary>
        /// Loads a native library by name and returns an operating system handle to it.
        /// </summary>
        /// <param name="name">The name of the library to open. This parameter must not be null or empty.</param>
        /// <returns>The operating system handle for the shared library.
        /// If the library cannot be loaded, IntPtr.Zero should be returned.</returns>
        protected abstract IntPtr CoreLoadNativeLibrary(string name);

        /// <summary>
        /// Frees the library represented by the given operating system handle.
        /// </summary>
        /// <param name="handle">The handle of the open shared library. This must not be zero.</param>
        protected abstract void CoreFreeNativeLibrary(IntPtr handle);

        /// <summary>
        /// Loads a function pointer out of the given library by name.
        /// </summary>
        /// <param name="handle">The operating system handle of the opened shared library. This must not be zero.</param>
        /// <param name="functionName">The name of the exported function to load. This must not be null or empty.</param>
        /// <returns>A pointer to the loaded function.</returns>
        protected abstract IntPtr CoreLoadFunctionPointer(IntPtr handle, string functionName);

        /// <summary>
        /// Returns a default library loader for the running operating system.
        /// </summary>
        /// <returns>A LibraryLoader suitable for loading libraries.</returns>
        public static NativeLibraryLoader GetPlatformDefaultLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsNativeLibraryLoader();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new UnixNativeLibraryLoader();
            }

            throw new PlatformNotSupportedException("This platform cannot load native libraries.");
        }

        private class WindowsNativeLibraryLoader : NativeLibraryLoader
        {
            protected override void CoreFreeNativeLibrary(IntPtr handle)
            {
                Kernel32.FreeLibrary(handle);
            }

            protected override IntPtr CoreLoadFunctionPointer(IntPtr handle, string functionName)
            {
                return Kernel32.GetProcAddress(handle, functionName);
            }

            protected override IntPtr CoreLoadNativeLibrary(string name)
            {
                return Kernel32.LoadLibrary(name);
            }
        }

        private class UnixNativeLibraryLoader : NativeLibraryLoader
        {
            protected override void CoreFreeNativeLibrary(IntPtr handle)
            {
                Libdl.dlclose(handle);
            }

            protected override IntPtr CoreLoadFunctionPointer(IntPtr handle, string functionName)
            {
                return Libdl.dlsym(handle, functionName);
            }

            protected override IntPtr CoreLoadNativeLibrary(string name)
            {
                return Libdl.dlopen(name, Libdl.RTLD_NOW);
            }
        }
    }
}
