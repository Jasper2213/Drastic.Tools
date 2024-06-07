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
        private readonly ContextMenuStrip _contextMenuStrip;
        private readonly NotifyIcon _notifyIcon;
        private Icon? _icon;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrayIcon"/> class.
        /// </summary>
        /// <param name="name">Icon Title.</param>
        /// <param name="image">Icon Image Stream. Optional.</param>
        /// <param name="menuItems">Items to populate context menu. Optional.</param>
        public TrayIcon(string name, TrayImage image, List<TrayMenuItem>? menuItems = null)
        {
            _notifyIcon = new NotifyIcon();
            UpdateName(name);
            UpdateImage(image);
            _menuItems = menuItems ?? new List<TrayMenuItem>();
            _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
            _notifyIcon.ContextMenuStrip = _contextMenuStrip;
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
            _notifyIcon.Visible = true;
            UpdateMenu(_menuItems);
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
            _notifyIcon?.Dispose();
            _icon?.Dispose();
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

            ToolStripMenuItem menu = new(item.Text);
            if (item.Icon is not null)
            {
                menu.Image = item.Icon.Image;
            }

            if (item.SubMenuItems is not null)
            {
                foreach (var subItem in item.SubMenuItems)
                {
                    menu.DropDownItems.Add(GenerateItem(subItem));
                }
            }

            return menu;
        }

        public void UpdateMenu(IEnumerable<TrayMenuItem> menuItems)
        {
            _menuItems = menuItems.ToList();
            _contextMenuStrip.Items.Clear();

            ToolStripItem[] items = _menuItems.Select(GenerateItem).ToArray();

            _contextMenuStrip.Items.AddRange(items);
        }

        public void UpdateImage(TrayImage image)
        {
            Bitmap test = new(image?.Image!);
            _icon = Icon.FromHandle(test.GetHicon());
            _notifyIcon.Icon = _icon;
        }

        public void UpdateName(string name)
            => _notifyIcon.Text = name;

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
