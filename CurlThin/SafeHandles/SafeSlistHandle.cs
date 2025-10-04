﻿using System;
using System.Runtime.InteropServices;

namespace CurlThin.SafeHandles
{
    public sealed class SafeSlistHandle : SafeHandle
    {
        private SafeSlistHandle() : base(IntPtr.Zero, true)
        {
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