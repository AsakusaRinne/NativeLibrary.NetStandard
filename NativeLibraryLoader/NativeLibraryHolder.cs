using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NativeLibraryNetStandard
{
    /// <summary>
    /// Represents a native shared library opened by the operating system.
    /// This type can be used to load native function pointers by name.
    /// </summary>
    public class NativeLibraryHolder : IDisposable
    {
        private static readonly NativeLibraryLoader s_platformDefaultLoader = NativeLibraryLoader.GetPlatformDefaultLoader();
        private readonly NativeLibraryLoader _loader;
        private bool _autoFree;
        private Dictionary<string, object> _functionCache;

        /// <summary>
        /// The operating system handle of the loaded library.
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// Construct a <see cref="NativeLibraryHolder"/> with a handle.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="autoFree">If true, the owner of the handle will be the new object.</param>
        public NativeLibraryHolder(IntPtr handle, bool autoFree) : this(handle, autoFree, s_platformDefaultLoader)
        {

        }

        /// <summary>
        /// Construct a <see cref="NativeLibraryHolder"/> with a handle.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="autoFree">If true, the owner of the handle will be the new object.</param>
        /// <param name="loader"></param>
        public NativeLibraryHolder(IntPtr handle, bool autoFree, NativeLibraryLoader loader)
        {
            Handle = handle;
            _autoFree = autoFree;
            _loader = loader;
            _functionCache = new();
        }

        /// <summary>
        /// Constructs a new NativeLibraryHandler using the platform's default library loader.
        /// </summary>
        /// <param name="name">The name of the library to load.</param>
        /// <param name="autoFree">Whether the handle should be automatically free when the <see cref="NativeLibrary"/> is disposed.</param>
        public NativeLibraryHolder(string name, bool autoFree = true) : this(name, s_platformDefaultLoader, new EmptyPathResolver(), autoFree)
        {
        }

        /// <summary>
        /// Constructs a new NativeLibraryHandler using the platform's default library loader.
        /// </summary>
        /// <param name="names">An ordered list of names to attempt to load.</param>
        /// <param name="autoFree">Whether the handle should be automatically free when the <see cref="NativeLibrary"/> is disposed.</param>
        public NativeLibraryHolder(string[] names, bool autoFree = true) : this(names, s_platformDefaultLoader, new EmptyPathResolver(), autoFree)
        {
        }

        /// <summary>
        /// Constructs a new NativeLibraryHandler using the specified library loader.
        /// </summary>
        /// <param name="name">The name of the library to load.</param>
        /// <param name="loader">The loader used to open and close the library, and to load function pointers.</param>
        /// <param name="autoFree">Whether the handle should be automatically free when the <see cref="NativeLibrary"/> is disposed.</param>
        public NativeLibraryHolder(string name, NativeLibraryLoader loader, bool autoFree = true) : this(name, loader, new EmptyPathResolver(), autoFree)
        {
        }

        /// <summary>
        /// Constructs a new NativeLibraryHandler using the specified library loader.
        /// </summary>
        /// <param name="names">An ordered list of names to attempt to load.</param>
        /// <param name="loader">The loader used to open and close the library, and to load function pointers.</param>
        /// <param name="autoFree">Whether the handle should be automatically free when the <see cref="NativeLibrary"/> is disposed.</param>
        public NativeLibraryHolder(string[] names, NativeLibraryLoader loader, bool autoFree = true) : this(names, loader, new EmptyPathResolver(), autoFree)
        {
        }

        /// <summary>
        /// Constructs a new NativeLibraryHandler using the specified library loader.
        /// </summary>
        /// <param name="name">The name of the library to load.</param>
        /// <param name="loader">The loader used to open and close the library, and to load function pointers.</param>
        /// <param name="pathResolver">The path resolver, used to identify possible load targets for the library.</param>
        /// <param name="autoFree">Whether the handle should be automatically free when the <see cref="NativeLibrary"/> is disposed.</param>
        public NativeLibraryHolder(string name, NativeLibraryLoader loader, IPathResolver pathResolver, bool autoFree = true)
        {
            _loader = loader;
            _autoFree = autoFree;
            _functionCache = new();
            Handle = _loader.LoadNativeLibrary(name, pathResolver);
        }

        /// <summary>
        /// Constructs a new NativeLibraryHandler using the specified library loader.
        /// </summary>
        /// <param name="names">An ordered list of names to attempt to load.</param>
        /// <param name="loader">The loader used to open and close the library, and to load function pointers.</param>
        /// <param name="pathResolver">The path resolver, used to identify possible load targets for the library.</param>
        /// <param name="autoFree">Whether the handle should be automatically free when the <see cref="NativeLibrary"/> is disposed.</param>
        public NativeLibraryHolder(string[] names, NativeLibraryLoader loader, IPathResolver pathResolver, bool autoFree = true)
        {
            _loader = loader;
            _autoFree = autoFree;
            _functionCache = new();
            Handle = _loader.LoadNativeLibrary(names, pathResolver);
        }

        /// <summary>
        /// Loads a function whose signature matches the given delegate type's signature.
        /// </summary>
        /// <typeparam name="T">The type of delegate to return.</typeparam>
        /// <param name="name">The name of the native export.</param>
        /// <returns>A delegate wrapping the native function.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no function with the given name
        /// is exported from the native library.</exception>
        public T LoadFunction<T>(string name) where T: Delegate
        {
            if (_functionCache.TryGetValue(name, out object cachedFunc))
            {
                return (T)cachedFunc;
            }
            else
            {
                IntPtr functionPtr = _loader.LoadFunctionPointer(Handle, name);
                var error = Marshal.GetLastWin32Error();
                if (functionPtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"No function was found with the name {name}.");
                }

                var func = Marshal.GetDelegateForFunctionPointer<T>(functionPtr);
                _functionCache[name] = func;
                return func;
            }
        }

        /// <summary>
        /// Loads a function pointer with the given name.
        /// </summary>
        /// <param name="name">The name of the native export.</param>
        /// <returns>A function pointer for the given name, or 0 if no function with that name exists.</returns>
        public IntPtr LoadFunction(string name)
        {
            return _loader.LoadFunctionPointer(Handle, name);
        }

        /// <summary>
        /// Frees the native library. Function pointers retrieved from this library will be void.
        /// </summary>
        public void Dispose()
        {
            if (_autoFree && Handle != IntPtr.Zero)
            {
                _loader.FreeNativeLibraryHandle(Handle);
            }
        }
    }
}
