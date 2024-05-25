// <copyright file="TrayIcon.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace Drastic.Tray
{
    public partial class TrayIcon : IDisposable
    {
        private List<TrayMenuItem> menuItems { get; set; } = new List<TrayMenuItem>();
        private bool disposedValue;

        /// <summary>
        /// Left Clicked Event.
        /// </summary>
        public event EventHandler<TrayClickedEventArgs>? LeftClicked;

        /// <summary>
        /// Right Clicked Event.
        /// </summary>
        public event EventHandler<TrayClickedEventArgs>? RightClicked;

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Is Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    NativeElementDispose();
                }

                disposedValue = true;
            }
        }

#if !MACCATALYST && !WINDOWS && !MACOS
        /// <summary>
        /// Dispose Native Elements.
        /// </summary>
        public void NativeElementDispose()
        {
        }
#endif
    }
}