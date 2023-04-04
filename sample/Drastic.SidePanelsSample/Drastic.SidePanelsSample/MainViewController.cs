﻿using System;
using SidePanels;

namespace Drastic.SidePanelsSample
{
    public partial class MainViewController : SidePanelController
    {
        public MainViewController()
        {
            LeftPanel = new LeftViewController();
            var center = new UINavigationController(new CenterViewController());
            center.NavigationBar.Translucent = false;
            CenterPanel = center;
            RightPanel = new RightViewController();
        }
    }
}