﻿// <copyright file="TrayMenuItem.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

namespace Drastic.Tray
{
    /// <summary>
    /// Drastic Tray Menu Item.
    /// </summary>
    public class TrayMenuItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrayMenuItem"/> class.
        /// </summary>
        public TrayMenuItem()
        {
            Text = string.Empty;
            IsSeperator = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrayMenuItem"/> class.
        /// </summary>
        /// <param name="text">Menu Text.</param>
        /// <param name="icon">Icon.</param>
        /// <param name="action">Action to perform when clicked.</param>
        public TrayMenuItem(string text, TrayImage? icon = null, Func<Task>? action = null)
        {
            Text = text;
            Icon = icon;
            Action = action;
        }

#if MACCATALYST

        /// <summary>
        /// Initializes a new instance of the <see cref="TrayMenuItem"/> class.
        /// </summary>
        /// <param name="text">Menu Text.</param>
        /// <param name="icon">Icon.</param>
        /// <param name="action">Action to perform when clicked.</param>
        /// <param name="keyEquivalent">Keyboard Shortcut key.</param>
        /// <param name="keyEquivalentModifierMask">Key.</param>
        public TrayMenuItem(string text, TrayImage? icon = null, Func<Task>? action = null, string? keyEquivalent = default, NSEventModifierMask? keyEquivalentModifierMask = default)
        {
            Text = text;
            Icon = icon;
            Action = action;
            KeyEquivalent = keyEquivalent;
            KeyEquivalentModifierMask = keyEquivalentModifierMask;
        }

#endif

        /// <summary>
        /// Gets a value indicating whether the tray menu item is a seperator.
        /// </summary>
        public bool IsSeperator { get; }

        /// <summary>
        /// Gets the text for the menu item.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the icon for the menu item.
        /// Optional.
        /// </summary>
        public TrayImage? Icon { get; }

        /// <summary>
        /// Gets the action to be performed when the item is clicked.
        /// Optional.
        /// </summary>
        public Func<Task>? Action { get; }

#if MACCATALYST

        /// <summary>
        /// Gets the Key Equivalent shortcut.
        /// Optional.
        /// </summary>
        public string? KeyEquivalent { get; }

        /// <summary>
        /// Gets the Key Equivalent Modifer Mask shortcut.
        /// Optional.
        /// </summary>
        public NSEventModifierMask? KeyEquivalentModifierMask { get; }

#endif
    }
}
