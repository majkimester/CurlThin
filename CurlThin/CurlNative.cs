using System;
using System.IO;
using System.Runtime.InteropServices;
using CurlThin.Enums;
using CurlThin.SafeHandles;

namespace CurlThin
{
    /// <remarks>
    ///     Type mappings (C -> C#):
    ///     - size_t -> UIntPtr
    ///     - int    -> int
    ///     - long   -> int
    /// </remarks>
    public static class CurlNative
    {
        private const string LIBCURL = "libcurl.dll";
        private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;

        #region Load platform dependent library

        static CurlNative()
        {
            // Load the platform dependent libcurl.dll
            if (!TryLoadNativeLibrary(AppDomain.CurrentDomain.RelativeSearchPath))
                TryLoadNativeLibrary(Path.GetDirectoryName(typeof(CurlNative).Assembly.Location));
        }

        private static bool TryLoadNativeLibrary(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
                return false;

            string archFolder = GetArchitectureFolder();

            string fullPath = Path.Combine(basePath, archFolder, LIBCURL);

            if (!File.Exists(fullPath))
                return false;

            IntPtr handle = LoadLibrary(fullPath);
            return handle != IntPtr.Zero;
        }

        private static string GetArchitectureFolder()
        {
            if (!Environment.Is64BitProcess)
                return "x86";
            else
                return IsArm64() ? "arm64" : "x64";
        }

        private static bool IsArm64()
        {
            SYSTEM_INFO info;
            GetNativeSystemInfo(out info);
            const ushort PROCESSOR_ARCHITECTURE_ARM64 = 12;
            return info.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_ARM64;
        }

        [DllImport("kernel32.dll")]
        private static extern void GetNativeSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

        #endregion

        #region libCURL library

        [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_global_init")]
        public static extern CURLcode Init(CURLglobal flags = CURLglobal.DEFAULT);

        [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_global_cleanup")]
        public static extern void Cleanup();

        public static class Easy
        {
            [UnmanagedFunctionPointer(CALLING_CONVENTION)]
            public delegate UIntPtr DataHandler(IntPtr data, UIntPtr size, UIntPtr nmemb, IntPtr userdata);

            [UnmanagedFunctionPointer(CALLING_CONVENTION)]
            public delegate int XferInfoFunctionDelegate(IntPtr clientp, long dltotal, long dlnow, long ultotal, long ulnow);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_init")]
            public static extern SafeEasyHandle Init();

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_cleanup")]
            public static extern void Cleanup(IntPtr handle);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_perform")]
            public static extern CURLcode Perform(SafeEasyHandle handle);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_reset")]
            public static extern void Reset(SafeEasyHandle handle);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, int value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, CURLAUTH value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, IntPtr value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_setopt", CharSet = CharSet.Ansi)]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, string value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, DataHandler value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(SafeEasyHandle handle, CURLoption option, XferInfoFunctionDelegate callback);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out int value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out IntPtr value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, out double value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_getinfo", CharSet = CharSet.Ansi)]
            public static extern CURLcode GetInfo(SafeEasyHandle handle, CURLINFO option, IntPtr value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_easy_strerror")]
            public static extern IntPtr StrError(CURLcode errornum);
        }

        public static class Multi
        {
            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_init")]
            public static extern SafeMultiHandle Init();

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_cleanup")]
            public static extern CURLMcode Cleanup(IntPtr multiHandle);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_add_handle")]
            public static extern CURLMcode AddHandle(SafeMultiHandle multiHandle, SafeEasyHandle easyHandle);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_remove_handle")]
            public static extern CURLMcode RemoveHandle(SafeMultiHandle multiHandle, SafeEasyHandle easyHandle);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option, int value);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_info_read")]
            public static extern IntPtr InfoRead(SafeMultiHandle multiHandle, out int msgsInQueue);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_socket_action")]
            public static extern CURLMcode SocketAction(SafeMultiHandle multiHandle, SafeSocketHandle sockfd,
                CURLcselect evBitmask,
                out int runningHandles);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct CURLMsg
            {
                public CURLMSG msg; /* what this message means */
                public IntPtr easy_handle; /* the handle it concerns */
                public CURLMsgData data;

                [StructLayout(LayoutKind.Explicit)]
                public struct CURLMsgData
                {
                    [FieldOffset(0)] public IntPtr whatever; /* (void*) message-specific data */
                    [FieldOffset(0)] public CURLcode result; /* return code for transfer */
                }
            }

            #region curl_multi_setopt for CURLMOPT_TIMERFUNCTION

            public delegate int TimerCallback(IntPtr multiHandle, int timeoutMs, IntPtr userp);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option, TimerCallback value);

            #endregion

            #region curl_multi_setopt for CURLMOPT_SOCKETFUNCTION

            public delegate int SocketCallback(IntPtr easy, IntPtr s, CURLpoll what, IntPtr userp, IntPtr socketp);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(SafeMultiHandle multiHandle, CURLMoption option,
                SocketCallback value);

            #endregion
        }

        public static class Slist
        {
            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_slist_append")]
            public static extern IntPtr Append(IntPtr list, string data);

            [DllImport(LIBCURL, CallingConvention = CALLING_CONVENTION, EntryPoint = "curl_slist_free_all")]
            public static extern void FreeAll(IntPtr handle);
        }

        #endregion
    }
}