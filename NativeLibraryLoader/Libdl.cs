using System;
using System.Runtime.InteropServices;

namespace NativeLibraryNetStandard
{
    // #define RTLD_LAZY       0x00001 /* Lazy function call binding.  */
    // #define RTLD_NOW        0x00002 /* Immediate function call binding.  */
    // #define RTLD_BINDING_MASK   0x3 /* Mask of binding time value.  */
    // #define RTLD_NOLOAD     0x00004 /* Do not load the object.  */
    // #define RTLD_DEEPBIND   0x00008 /* Use deep binding.  */

    // /* If the following bit is set in the MODE argument to `dlopen',
    // the symbols of the loaded object and its dependencies are made
    // visible as if the object were linked directly into the program.  */
    // #define RTLD_GLOBAL     0x00100

    // /* Unix98 demands the following flag which is the inverse to RTLD_GLOBAL.
    // The implementation does this by default and so we can define the
    // value to zero.  */
    // #define RTLD_LOCAL      0

    // /* Do not delete object when closed.  */
    // #define RTLD_NODELETE   0x01000
    internal static class Libdl
    {
        private static class Libdl1
        {
            private const string LibName = "libdl";

            [DllImport(LibName, SetLastError = true)]
            public static extern IntPtr dlopen(string fileName, int flags);

            [DllImport(LibName)]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport(LibName)]
            public static extern int dlclose(IntPtr handle);

            [DllImport(LibName)]
            public static extern string dlerror();
        }

        private static class Libdl2
        {
            private const string LibName = "libdl.so.2";

            [DllImport(LibName, SetLastError = true)]
            public static extern IntPtr dlopen(string fileName, int flags);

            [DllImport(LibName)]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport(LibName)]
            public static extern int dlclose(IntPtr handle);

            [DllImport(LibName)]
            public static extern string dlerror();
        }

        static Libdl()
        {
            try
            {
                Libdl1.dlerror();
                m_useLibdl1 = true;
            }
            catch
            {
            }
        }

        private static bool m_useLibdl1;

        /// <summary>
        /// RTLD_GLOBAL + RTLD_NOW
        /// </summary>
        public const int RTLD_NOW = 0x00102;

        public static IntPtr dlopen(string fileName, int flags) => m_useLibdl1 ? Libdl1.dlopen(fileName, flags) : Libdl2.dlopen(fileName, flags);

        public static IntPtr dlsym(IntPtr handle, string name) => m_useLibdl1 ? Libdl1.dlsym(handle, name) : Libdl2.dlsym(handle, name);

        public static int dlclose(IntPtr handle) => m_useLibdl1 ? Libdl1.dlclose(handle) : Libdl2.dlclose(handle);

        public static string dlerror() => m_useLibdl1 ? Libdl1.dlerror() : Libdl2.dlerror();
    }
}
