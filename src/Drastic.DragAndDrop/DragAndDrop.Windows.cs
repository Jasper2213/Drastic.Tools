﻿// <copyright file="DragAndDrop.Windows.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Drastic.DragAndDrop
{
    /// <summary>
    /// Drag And Drop View.
    /// </summary>
    public partial class DragAndDrop
    {
        private Microsoft.UI.Xaml.UIElement? panel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DragAndDrop"/> class.
        /// </summary>
        /// <param name="window">Window. Sets the drag and drop view to the inner Window Contents.</param>
        public DragAndDrop(Microsoft.UI.Xaml.Window window)
            : this(window.Content!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DragAndDrop"/> class.
        /// </summary>
        /// <param name="element">Sets the drag and drop view to the UIElement.</param>
        public DragAndDrop(Microsoft.UI.Xaml.UIElement element)
        {
            this.panel = element;
            this.panel.AllowDrop = true;
            this.panel.DragOver += this.Panel_DragOver;
            this.panel.Drop += this.Panel_Drop;
            this.panel.DragLeave += this.Panel_DragLeave;
            this.panel.DropCompleted += this.Panel_DropCompleted;
        }

        /// <summary>
        /// Dispose Elements.
        /// </summary>
        internal void DisposePlatformElements()
        {
            if (this.panel != null)
            {
                this.panel.AllowDrop = false;
                this.panel.DragOver -= this.Panel_DragOver;
                this.panel.Drop -= this.Panel_Drop;
                this.panel.DragLeave -= this.Panel_DragLeave;
                this.panel.DropCompleted -= this.Panel_DropCompleted;
            }
        }

        private void Panel_DropCompleted(Microsoft.UI.Xaml.UIElement sender, Microsoft.UI.Xaml.DropCompletedEventArgs args)
        {
            this.IsDragging = false;
        }

        private void Panel_DragLeave(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            this.IsDragging = false;
        }

        private async void Panel_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Any())
                {
                    var filePaths = new List<string>();
                    foreach (var item in items)
                    {
                        if (item is StorageFile file)
                        {
                            filePaths.Add(item.Path);
                        }
                    }

                    this.Drop?.Invoke(this, new DragAndDropOverlayTappedEventArgs(filePaths));
                }
            }

            this.IsDragging = false;
        }

        private void Panel_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            this.IsDragging = true;
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }
    }
}
