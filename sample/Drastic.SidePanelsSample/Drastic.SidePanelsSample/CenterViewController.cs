﻿using System;
namespace Drastic.SidePanelsSample
{
    partial class CenterViewController : DebugViewController
    {
        public CenterViewController()
        {
            Title = "Center Panel";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Random rnd = new Random();

            View.BackgroundColor = UIColor.FromRGB(
                (float)rnd.NextDouble(),
                (float)rnd.NextDouble(),
                (float)rnd.NextDouble());
        }
    }
}