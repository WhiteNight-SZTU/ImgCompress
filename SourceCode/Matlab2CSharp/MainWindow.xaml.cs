// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using WinRT.Interop;

using System.Runtime.InteropServices; // For DllImport
using WinRT; // required to support Window.As<ICompositionSupportsSystemBackdrop>()
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Matlab2CSharp
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow current;
        public static Microsoft.UI.Xaml.Controls.NavigationView navigationView;
        public static Microsoft.UI.Xaml.Controls.Frame currentFrame;

        public MainWindow()
        {
            this.InitializeComponent();
            current =this;
            SetTitleBar(AppTitleBar);
            ExtendsContentIntoTitleBar = true;
            Type pageType = Type.GetType("Matlab2CSharp.Pages.HomePage");
            pagesFrame.Navigate(pageType);


        }
        private void LoadPages(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            
            navigationView = sender;
            var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
            string pageName = "Matlab2CSharp.Pages." + ((string)selectedItem.Tag);
            Type pageType = Type.GetType(pageName);
            if(pageType == null)
            {

            }
            else
            {
                pagesFrame.Navigate(pageType);
                currentFrame = pagesFrame;
            }
           
        }
     

    }
    
    
}
