// <copyright file="TrayIcon.Catalyst.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using AppKit;
using CoreFoundation;
using CoreGraphics;
using Drastic.Interop;
using Foundation;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drastic.Tray
{
    /// <summary>
    /// Tray Icon.
    /// </summary>
    public partial class TrayIcon : NSObject, ITrayIcon
    {
        private readonly NSObject statusBarItem;
        private readonly ShimNSMenu menu;
        private readonly NSObject? statusBarButton;
        private readonly bool setToSystemTheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrayIcon"/> class.
        /// </summary>
        /// <param name="name">Name of the icon.</param>
        /// <param name="image">Icon Image Stream. Optional.</param>
        /// <param name="menuItems">Items to populate context menu. Optional.</param>
        /// <param name="setToSystemTheme">Sets the icon to match the system theme.</param>
        public TrayIcon(string name, TrayImage image, List<TrayMenuItem>? menuItems = null, bool setToSystemTheme = true)
        {
            this.setToSystemTheme = setToSystemTheme;
            this.menuItems = menuItems ?? new List<TrayMenuItem>();

            menu = new ShimNSMenu();

            NSObject systemStatusBarObj = GetNSStatusBar().PerformSelector(new Selector("systemStatusBar"));
            statusBarItem = Runtime.GetNSObject(Drastic.Interop.ObjC.Call(systemStatusBarObj.Handle, "statusItemWithLength:", -1f))!;
            statusBarButton = Runtime.GetNSObject(Drastic.Interop.ObjC.Call(statusBarItem.Handle, "button"))!;

            UpdateImage(image, setToSystemTheme);
            UpdateName(name);

            if (statusBarButton is not null)
            {
                // Handle click
                // 26 = NSEventType.OtherMouseUp
                Drastic.Interop.ObjC.Call(statusBarButton.Handle, "sendActionOn:", (nint)NSEventType.OtherMouseUp);
                Drastic.Interop.ObjC.Call(statusBarButton.Handle, "setTarget:", Handle);
                Drastic.Interop.ObjC.Call(statusBarButton.Handle, "setAction:", new Selector("handleButtonClick:").Handle);
            }

            if (menuItems is not null)
            {
                SetupStatusBarMenu(menuItems);
            }
        }

        public CGRect GetFrame()
        {
            NSObject statusBarButton = Runtime.GetNSObject(Drastic.Interop.ObjC.Call(statusBarItem!.Handle, "button"));
            NSObject nsButtonWindow = Runtime.GetNSObject(Drastic.Interop.ObjC.Call(statusBarButton!.Handle, "window"));
            if (nsButtonWindow is null)
            {
                return new CGRect(0, 0, 0, 0);
            }

            NSValue windowFrame = (NSValue)nsButtonWindow.ValueForKey(new Foundation.NSString("frame"));

            return windowFrame.CGRectValue;
        }

        public void OpenMenu()
        {
            NativeHandle nonNullHandle = menu.GetNonNullHandle("menu");
            Drastic.Interop.ObjC.Call(statusBarItem.Handle, "popUpStatusItemMenu:", nonNullHandle);
        }

        public void SetupStatusBarMenu(List<TrayMenuItem> menuItems)
        {
            menu.RemoveAllItems();

            foreach (TrayMenuItem item in this.menuItems)
            {
                if (item.IsSeperator)
                {
                    menu.AddItem(Runtime.GetNSObject<NSObject>(Drastic.Interop.ObjC.Call(Drastic.Interop.AppKit.GetClass("NSMenuItem"), "separatorItem"))!);
                    continue;
                }

                ShimNSMenuItem nsMenuItem = new ShimNSMenuItem(item);
                menu.AddItem(nsMenuItem);
            }
        }

        private static NSObject GetNSMenu()
           => Runtime.GetNSObject(Drastic.Interop.AppKit.Call("NSMenu", "new"))!;

        private static NSObject GetNSStatusBar()
           => Runtime.GetNSObject(Class.GetHandle("NSStatusBar"))!;

        private static NSObject GetNSMenuItem()
        {
            NSObject item = Runtime.GetNSObject(Drastic.Interop.AppKit.Call("NSMenuItem", "alloc"))!;
            return item;
        }

        private static void NSApplicationActivateIgnoringOtherApps(bool ignoreSetting = true)
        {
            nint sharedApp = Drastic.Interop.AppKit.Call("NSApplication", "sharedApplication");
            Drastic.Interop.ObjC.Call(sharedApp, "activateIgnoringOtherApps:", ignoreSetting);
        }

        private static NSObject GetNSApplicationSharedApplicationCurrentEvent()
        {
            nint sharedApp = Drastic.Interop.AppKit.Call("NSApplication", "sharedApplication");
            return Runtime.GetNSObject<NSObject>(Drastic.Interop.ObjC.Call(sharedApp, "currentEvent"))!;
        }

        private void NativeElementDispose()
        {
            statusBarItem.Dispose();
            menu.Dispose();
        }

        [Export("handleButtonClick:")]
        private void HandleClick(NSObject senderStatusBarButton)
        {
            NSObject test = GetNSApplicationSharedApplicationCurrentEvent()!;
            NSEventType type = (NSEventType)Drastic.Interop.ObjC.Call(test.Handle, "type");
            NSApplicationActivateIgnoringOtherApps(true);
            switch (type)
            {
                case NSEventType.LeftMouseDown:
                    LeftClicked?.Invoke(this, TrayClickedEventArgs.Empty);
                    break;
                case NSEventType.RightMouseDown:
                    RightClicked?.Invoke(this, TrayClickedEventArgs.Empty);
                    break;
            }
        }

        public void UpdateMenu(IEnumerable<TrayMenuItem> items)
            => SetupStatusBarMenu(items.ToList());

        public void UpdateImage(TrayImage image, bool setToSystemTheme)
        {
            // Matching what is on macOS...
            // this.statusBarItem!.Button.Image.Size = new CGSize(20, 20);
            // this.statusBarItem!.Button.Frame = new CGRect(0, 0, 40, 24);
            CGRect cgRect = new CGRect(0, 0, 40, 24);
            image.Image.Size = new CoreGraphics.CGSize(20, 20);
            Drastic.Interop.ObjC.Call(statusBarButton!.Handle, "setImage:", image.Image.Handle);
            Drastic.Interop.ObjC.Call(statusBarButton.Handle, "setFrame:", cgRect);
            Drastic.Interop.ObjC.Call(image.Image.Handle, "setTemplate:", setToSystemTheme);
        }

        public void UpdateImage(TrayImage image)
            => UpdateImage(image, setToSystemTheme);

        public void UpdateName(string name)
        {
        }

        internal class ShimNSMenu : NSObject
        {
            public ShimNSMenu()
            {
                Handle = Drastic.Interop.AppKit.Call("NSMenu", "new");
            }

            public void RemoveAllItems()
            {
                Drastic.Interop.ObjC.Call(Handle, "removeAllItems");
            }

            public void AddItem(NSObject item)
            {
                NativeHandle nonNullHandle = item.GetNonNullHandle("newItem");
                Drastic.Interop.ObjC.Call(Handle, "addItem:", nonNullHandle);
            }

            public void AddItem(ShimNSMenuItem item)
            {
                NativeHandle nonNullHandle = item.GetNonNullHandle("newItem");
                Drastic.Interop.ObjC.Call(Handle, "addItem:", nonNullHandle);
            }
        }

        internal class ShimNSMenuItem : NSObject
        {
            internal TrayMenuItem Item;

            private static readonly NativeClassDefinition CallbackClassDefinition;

            private readonly NativeClassInstance callbackClass;

            static ShimNSMenuItem()
            {
                CallbackClassDefinition = CreateCallbackClass();
            }

            public ShimNSMenuItem(TrayMenuItem item)
            {
                Item = item;
                Handle = Drastic.Interop.AppKit.Call("NSMenuItem", "alloc");
                ObjC.Call(Handle, "initWithTitle:action:keyEquivalent:", Drastic.Interop.NSString.Create(item.Text), ObjC.RegisterName("menuCallback:"), Drastic.Interop.NSString.Create(string.Empty));
                callbackClass = CallbackClassDefinition.CreateInstance(this);
                SetTarget(callbackClass.Handle);

                if (Item.Icon is not null)
                {
                    Image = Item.Icon.Image;
                }

                KeyEquivalent = item.KeyEquivalent;
                KeyEquivalentModifierMask = item.KeyEquivalentModifierMask;
            }

            public AppKit.NSImage? Image
            {
                get
                {
                    return Runtime.GetNSObject<NSImage>(Drastic.Interop.ObjC.Call(Handle, "image"));
                }

                set
                {
                    NativeHandle arg = value.GetHandle();
                    Drastic.Interop.ObjC.Call(Handle, "setImage:", arg);
                }
            }

            public string? KeyEquivalent
            {
                get
                {
                    return CFString.FromHandle(Drastic.Interop.ObjC.Call(Handle, "keyEquivalent"));
                }

                set
                {
                    NativeHandle arg = CFString.CreateNative(value);
                    Drastic.Interop.ObjC.Call(Handle, "setKeyEquivalent:", arg);
                }
            }

            public NSEventModifierMask? KeyEquivalentModifierMask
            {
                get
                {
                    return (NSEventModifierMask)(nuint)Drastic.Interop.ObjC.Call(Handle, "keyEquivalentModifierMask");
                }

                set
                {
                    if (value is not null)
                    {
                        Drastic.Interop.ObjC.Call(Handle, "setKeyEquivalentModifierMask:", (nuint)value);
                    }
                }
            }

            private static NativeClassDefinition CreateCallbackClass()
            {
                NativeClassDefinition definition = NativeClassDefinition.FromObject("DrasticInteropMenuCallback");

                definition.AddMethod<MenuCallbackDelegate>(
                    "menuCallback:",
                    "v@:@",
                    (self, op, menu) =>
                    {
                        ShimNSMenuItem instance = definition.GetParent<ShimNSMenuItem>(self);
                        instance.Item.Action?.Invoke();
                    });

                definition.FinishDeclaration();

                return definition;
            }

            private void SetTarget(IntPtr target)
            {
                ObjC.Call(Handle, "setTarget:", target);
            }

            private void SetTag(long tag)
            {
                ObjC.Call(Handle, "setTag:", new IntPtr(tag));
            }
        }
    }
}