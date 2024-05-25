// <copyright file="NativeClassInstance.Apple.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace Drastic.Interop
{
    internal sealed class NativeClassInstance : IDisposable
    {
        private readonly GCHandle parentHandle;

        internal NativeClassInstance(IntPtr instance, GCHandle parentHandle)
        {
            Handle = instance;
            this.parentHandle = parentHandle;
        }

        ~NativeClassInstance()
        {
            Dispose(false);
        }

        public IntPtr Handle { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            parentHandle.Free();
        }
    }
}
