using System;
using System.Runtime.InteropServices;

namespace CurlThin.SafeHandles
{
    public sealed class SafeSlistHandle : SafeHandle
    {
        private SafeSlistHandle() : base(IntPtr.Zero, true)
        {
        }

        public SafeSlistHandle Append(string data)
        {
            if (IsClosed)
                throw new ObjectDisposedException(nameof(SafeSlistHandle));

            IntPtr newPtr = CurlNative.Slist.Append(handle, data);
            if (newPtr == IntPtr.Zero)
                throw new InvalidOperationException("CurlNative.Slist.Append failed");
            SetHandle(newPtr);
            return this;
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        public static SafeSlistHandle Null => new SafeSlistHandle();

        protected override bool ReleaseHandle()
        {
            CurlNative.Slist.FreeAll(handle);
            return true;
        }
    }
}