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
        private readonly NSObject _statusBarItem;
        private readonly ShimNSMenu _menu;
        private readonly NSObject? _statusBarButton;
        private readonly bool _setToSystemTheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrayIcon"/> class.
        /// </summary>
        /// <param name="name">Name of the icon.</param>
        /// <param name="image">Icon Image Stream. Optional.</param>
        /// <param name="menuItems">Items to populate context menu. Optional.</param>
        /// <param name="setToSystemTheme">Sets the icon to match the system theme.</param>
        public TrayIcon(string name, TrayImage image, List<TrayMenuItem>? menuItems = null, bool setToSystemTheme = true)
        {
            this._setToSystemTheme = setToSystemTheme;
            _menuItems = menuItems ?? new List<TrayMenuItem>();

            _menu = new ShimNSMenu();

            NSObject systemStatusBarObj = GetNSStatusBar().PerformSelector(new Selector("systemStatusBar"));
            _statusBarItem = Runtime.GetNSObject(ObjC.Call(systemStatusBarObj.Handle, "statusItemWithLength:", -1f))!;
            _statusBarButton = Runtime.GetNSObject(ObjC.Call(_statusBarItem.Handle, "button"))!;

            UpdateImage(image, setToSystemTheme);
            UpdateName(name);

            if (_statusBarButton is not null)
            {
                // Handle click
                // 26 = NSEventType.OtherMouseUp
                ObjC.Call(_statusBarButton.Handle, "sendActionOn:", (nint)NSEventType.OtherMouseUp);
                ObjC.Call(_statusBarButton.Handle, "setTarget:", Handle);
                ObjC.Call(_statusBarButton.Handle, "setAction:", new Selector("handleButtonClick:").Handle);
            }

            if (menuItems is not null)
            {
                SetupStatusBarMenu(menuItems);
            }
        }

        public CGRect GetFrame()
        {
            NSObject? statusBarButton = Runtime.GetNSObject(ObjC.Call(_statusBarItem!.Handle, "button"));
            NSObject? nsButtonWindow = Runtime.GetNSObject(ObjC.Call(statusBarButton!.Handle, "window"));
            if (nsButtonWindow is null)
            {
                return new CGRect(0, 0, 0, 0);
            }

            NSValue windowFrame = (NSValue)nsButtonWindow.ValueForKey(new Foundation.NSString("frame"));

            return windowFrame.CGRectValue;
        }

        public void OpenMenu()
        {
            NativeHandle nonNullHandle = _menu.GetNonNullHandle("menu");
            ObjC.Call(_statusBarItem.Handle, "popUpStatusItemMenu:", nonNullHandle);
        }

        public void SetupStatusBarMenu(List<TrayMenuItem> menuItems)
        {
            _menu.RemoveAllItems();

            foreach (TrayMenuItem item in _menuItems)
            {
                if (item.IsSeperator)
                {
                    _menu.AddItem(Runtime.GetNSObject<NSObject>(ObjC.Call(Interop.AppKit.GetClass("NSMenuItem"), "separatorItem"))!);
                    continue;
                }

                ShimNSMenuItem nsMenuItem = new(item);
                _menu.AddItem(nsMenuItem);
            }
        }

        private static NSObject GetNSMenu()
           => Runtime.GetNSObject(Interop.AppKit.Call("NSMenu", "new"))!;

        private static NSObject GetNSStatusBar()
           => Runtime.GetNSObject(Class.GetHandle("NSStatusBar"))!;

        private static NSObject GetNSMenuItem()
        {
            NSObject item = Runtime.GetNSObject(Interop.AppKit.Call("NSMenuItem", "alloc"))!;
            return item;
        }

        private static void NSApplicationActivateIgnoringOtherApps(bool ignoreSetting = true)
        {
            nint sharedApp = Interop.AppKit.Call("NSApplication", "sharedApplication");
            ObjC.Call(sharedApp, "activateIgnoringOtherApps:", ignoreSetting);
        }

        private static NSObject GetNSApplicationSharedApplicationCurrentEvent()
        {
            nint sharedApp = Interop.AppKit.Call("NSApplication", "sharedApplication");
            return Runtime.GetNSObject<NSObject>(ObjC.Call(sharedApp, "currentEvent"))!;
        }

        private void NativeElementDispose()
        {
            _statusBarItem.Dispose();
            _menu.Dispose();
        }

        [Export("handleButtonClick:")]
        private void HandleClick(NSObject senderStatusBarButton)
        {
            NSObject test = GetNSApplicationSharedApplicationCurrentEvent()!;
            NSEventType type = (NSEventType)ObjC.Call(test.Handle, "type");
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
            CGRect cgRect = new(0, 0, 40, 24);
            image.Image.Size = new CoreGraphics.CGSize(20, 20);
            ObjC.Call(_statusBarButton!.Handle, "setImage:", image.Image.Handle);
            ObjC.Call(_statusBarButton.Handle, "setFrame:", cgRect);
            ObjC.Call(image.Image.Handle, "setTemplate:", setToSystemTheme);
        }

        public void UpdateImage(TrayImage image)
            => UpdateImage(image, _setToSystemTheme);

        public void UpdateName(string name)
        {
        }

        internal class ShimNSMenu : NSObject
        {
            public ShimNSMenu()
            {
                Handle = Interop.AppKit.Call("NSMenu", "new");
            }

            public void RemoveAllItems()
            {
                ObjC.Call(Handle, "removeAllItems");
            }

            public void AddItem(NSObject item)
            {
                NativeHandle nonNullHandle = item.GetNonNullHandle("newItem");
                ObjC.Call(Handle, "addItem:", nonNullHandle);
            }

            public void AddItem(ShimNSMenuItem item)
            {
                NativeHandle nonNullHandle = item.GetNonNullHandle("newItem");
                ObjC.Call(Handle, "addItem:", nonNullHandle);
            }
        }

        internal class ShimNSMenuItem : NSObject
        {
            internal TrayMenuItem _item;

            private static readonly NativeClassDefinition CallbackClassDefinition;

            private readonly NativeClassInstance callbackClass;

            static ShimNSMenuItem()
            {
                CallbackClassDefinition = CreateCallbackClass();
            }

            public ShimNSMenuItem(TrayMenuItem item)
            {
                _item = item;
                Handle = Interop.AppKit.Call("NSMenuItem", "alloc");
                ObjC.Call(Handle, "initWithTitle:action:keyEquivalent:", Interop.NSString.Create(item.Text), ObjC.RegisterName("menuCallback:"), Interop.NSString.Create(string.Empty));
                callbackClass = CallbackClassDefinition.CreateInstance(this);
                SetTarget(callbackClass.Handle);

                if (_item.Icon is not null)
                {
                    Image = _item.Icon.Image;
                }

                KeyEquivalent = item.KeyEquivalent;
                KeyEquivalentModifierMask = item.KeyEquivalentModifierMask;

                // Add submenus if present
                if (item.SubMenuItems?.Count > 0)
                {
                    var submenu = new ShimNSMenu();
                    foreach (var subItem in item.SubMenuItems)
                    {
                        ShimNSMenuItem subMenuItem = new(subItem);
                        submenu.AddItem(subMenuItem);
                    }
                    ObjC.Call(Handle, "setSubmenu:", submenu.Handle);
                }
            }

            public NSImage? Image
            {
                get
                {
                    return Runtime.GetNSObject<NSImage>(ObjC.Call(Handle, "image"));
                }

                set
                {
                    NativeHandle arg = value.GetHandle();
                    ObjC.Call(Handle, "setImage:", arg);
                }
            }

            public string? KeyEquivalent
            {
                get
                {
                    return CFString.FromHandle(ObjC.Call(Handle, "keyEquivalent"));
                }

                set
                {
                    NativeHandle arg = CFString.CreateNative(value);
                    ObjC.Call(Handle, "setKeyEquivalent:", arg);
                }
            }

            public NSEventModifierMask? KeyEquivalentModifierMask
            {
                get
                {
                    return (NSEventModifierMask)(nuint)ObjC.Call(Handle, "keyEquivalentModifierMask");
                }

                set
                {
                    if (value is not null)
                    {
                        ObjC.Call(Handle, "setKeyEquivalentModifierMask:", (nuint)value);
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
                        instance._item.Action?.Invoke();
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
