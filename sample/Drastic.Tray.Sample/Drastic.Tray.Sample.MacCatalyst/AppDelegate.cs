// <copyright file="AppDelegate.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Drastic.PureLayout;

namespace Drastic.Tray.Sample.MacCatalyst;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    /// <inheritdoc/>
    public override UIWindow? Window
    {
        get;
        set;
    }

    /// <inheritdoc/>
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // create a new window instance based on the screen size
        Window = new UIWindow(UIScreen.MainScreen.Bounds);

        Window.RootViewController = new SampleViewController(Window!);

        // make the window visible
        Window.MakeKeyAndVisible();

        return true;
    }
}

public class SampleViewController : UIViewController
{
    public UIButton TrayButton = new UIButton(UIButtonType.RoundedRect);
    private readonly UIWindow window;

    private TrayIcon? trayIcon;

    public SampleViewController(UIWindow window)
    {
        this.window = window;
        SetupUI();
        SetupLayout();
    }

    private void SetupUI()
    {
        View!.AddSubview(TrayButton);
        TrayButton.SetTitle("Add Tray Icon", UIControlState.Normal);
        TrayButton.TouchUpInside += TrayButton_TouchUpInside;
    }

    private async void TrayButton_TouchUpInside(object? sender, EventArgs e)
    {
        List<TrayMenuItem> menuItems = new List<TrayMenuItem>();
        UIImage? image = UIImage.GetSystemImage("trophy.circle");
        TrayImage trayImage = new TrayImage(image!);
        menuItems.Add(new TrayMenuItem("Hello!", trayImage, async () => { }, "h"));
        menuItems.Add(new TrayMenuItem());
        menuItems.Add(new TrayMenuItem("From!", trayImage, async () => { }, "f"));
        menuItems.Add(new TrayMenuItem());
        menuItems.Add(new TrayMenuItem("Mac Catalyst!", trayImage, async () => { }, "m", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.CommandKeyMask));
        trayIcon = new Drastic.Tray.TrayIcon("Tray Icon", trayImage, menuItems);
        trayIcon.RightClicked += (object? sender, TrayClickedEventArgs e) => { trayIcon.OpenMenu(); };
        trayIcon.LeftClicked += (object? sender, TrayClickedEventArgs e) =>
        {
            UIAlertController okAlertController = UIAlertController.Create("Drastic.Tray.Sample", "Welcome!", UIAlertControllerStyle.Alert);
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(okAlertController, true, null);
        };
    }

    private void SetupLayout()
    {
        TrayButton.AutoCenterInSuperview();
    }
}
