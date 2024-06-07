// <copyright file="App.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Drastic.Tray.Sample.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly TrayIcon icon;

        private Window? window;

        public App()
        {
            TrayImage trayImage = new TrayImage(System.Drawing.Image.FromStream(GetResourceFileContent("TrayIcon.ico")!));
            List<TrayMenuItem> menuItems = new List<TrayMenuItem>
            {
                new TrayMenuItem("Hello!", trayImage, async () => { }, new List<TrayMenuItem>() { new("Test") }),
                new TrayMenuItem("From!", trayImage, async () => { }),
                new TrayMenuItem("Windows!", trayImage, async () => { }),
            };
            icon = new TrayIcon("Tray Icon", trayImage, menuItems);
            icon.RightClicked += (object? sender, TrayClickedEventArgs e) =>
            {
                System.Diagnostics.Debug.WriteLine("Right Click!");
            };
            icon.LeftClicked += (object? sender, TrayClickedEventArgs e) =>
            {
                System.Diagnostics.Debug.WriteLine("Left Click!");
            };
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            window = new MainWindow();
            window.Activate();
        }

        /// <summary>
        /// Get Resource File Content via FileName.
        /// </summary>
        /// <param name="fileName">Filename.</param>
        /// <returns>Stream.</returns>
        public static Stream? GetResourceFileContent(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Drastic.Tray.Sample.WinUI." + fileName;
            if (assembly is null)
            {
                return null;
            }

            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
