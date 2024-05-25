// <copyright file="TrayIcon.Windows.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Drastic.Tray
{
    /// <summary>
    /// Tray Icon.
    /// </summary>
    public partial class TrayIcon : ITrayIcon
    {
        private readonly ContextMenuStrip contextMenuStrip;
        private readonly NotifyIcon notifyIcon;
        private Icon? icon;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrayIcon"/> class.
        /// </summary>
        /// <param name="name">Icon Title.</param>
        /// <param name="image">Icon Image Stream. Optional.</param>
        /// <param name="menuItems">Items to populate context menu. Optional.</param>
        public TrayIcon(string name, TrayImage image, List<TrayMenuItem>? menuItems = null)
        {
            notifyIcon = new NotifyIcon();
            UpdateName(name);
            UpdateImage(image);
            this.menuItems = menuItems ?? new List<TrayMenuItem>();
            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.Visible = true;
            UpdateMenu(this.menuItems);
        }

        private void ContextMenuStrip_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is DrasticToolStripMenuItem stripItem)
            {
                stripItem.Item.Action?.Invoke();
            }
        }

        private void NativeElementDispose()
        {
            notifyIcon?.Dispose();
            icon?.Dispose();
        }

        private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                LeftClicked?.Invoke(this, TrayClickedEventArgs.Empty);
            }

            if (e.Button == MouseButtons.Right)
            {
                RightClicked?.Invoke(this, TrayClickedEventArgs.Empty);
            }
        }

        private ToolStripItem GenerateItem(TrayMenuItem item)
        {
            if (item.IsSeperator)
            {
                return new ToolStripSeparator();
            }

            DrasticToolStripMenuItem menu = new DrasticToolStripMenuItem(item)
            {
                Text = item.Text
            };
            if (item.Icon is not null)
            {
                menu.Image = item.Icon.Image;
            }

            return menu;
        }

        public void UpdateMenu(IEnumerable<TrayMenuItem> menuItems)
        {
            this.menuItems = menuItems.ToList();
            contextMenuStrip.Items.Clear();
            ToolStripItem[] items = this.menuItems.Select(n => GenerateItem(n)).Reverse().ToArray();
            contextMenuStrip.Items.AddRange(items);
        }

        public void UpdateImage(TrayImage image)
        {
            Bitmap test = new Bitmap(image?.Image!);
            icon = Icon.FromHandle(test.GetHicon());
            notifyIcon.Icon = icon;
        }

        public void UpdateName(string name)
            => notifyIcon.Text = name;

        private class DrasticToolStripMenuItem : ToolStripMenuItem
        {
            public DrasticToolStripMenuItem(TrayMenuItem item)
            {
                Text = item.Text;
                if (item.Icon is not null)
                {
                    Image = item.Icon.Image;
                }

                Item = item;
            }

            public TrayMenuItem Item { get; }
        }
    }
}
