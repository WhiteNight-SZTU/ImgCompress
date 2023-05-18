// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI;
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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.ViewManagement;
using System.Net.Http;
using Windows.Networking.Connectivity;
using System.Threading.Tasks;

using MathWorks;
using MathWorks.MATLAB;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Microsoft.UI.Xaml.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using Windows.Storage.Streams;
using Microsoft.UI.Windowing;

namespace Matlab2CSharp.Pages
{
    public sealed partial class Settings : Page
    {
      
        public Settings()
        {
            this.InitializeComponent();
            isChanging_ChangingNavigationView = false;
            if(navigationView!=null)
                if(navigationView.PaneDisplayMode==Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top)
                    navigationViewPane_ChangeNavigationViewPane.IsOn = true;
            isChanging_ChangingNavigationView = true;

            isChanging_FullScreen = false;
            m_appWindow = GetAppWindowForCurrentWindow();
            if(m_appWindow != null)
                if(m_appWindow.Presenter.Kind==AppWindowPresenterKind.FullScreen)
                    FullScreenSetter.IsOn= true;
            isChanging_FullScreen = true;

            IntelIcon.Visibility = Visibility.Collapsed;
            initIntel();
        }
        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            navigationView = MainWindow.navigationView;
            var toggleSwitch = sender as ToggleSwitch;
            if(isChanging_ChangingNavigationView==true)
            {
                if(toggleSwitch.IsOn==true)
                {
                    navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
                }
                else
                {
                    navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                }
            }
        }
        private async void initMatlab(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            string url = "https://white-night.club/matlab/photo_compress.m";
            HttpResponseMessage response = await client.GetAsync(url);
            Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();

            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("m file", new List<string>() { ".m" });
            savePicker.SuggestedFileName = "photo_compress.m";
            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                using (var streamToWriteTo = await file.OpenStreamForWriteAsync())
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }
            }
            matlabFunctionFile = file;
        }
        private async void setPath(object sender,RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            filePicker.FileTypeFilter.Add(".m");
            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                folderPath = Path.GetDirectoryName(file.Path);
                matlabFunctionFile = file;
            }
                

        }

        private async void checkIntel(object sender,RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            bool isNetworkAvailable = false;
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            await DoAsyncOperation();

            if (InternetConnectionProfile != null && InternetConnectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
            {
                isNetworkAvailable = true;
            }
            if (isNetworkAvailable)
                IntelIcon.Symbol = Microsoft.UI.Xaml.Controls.Symbol.Accept;
            else
                IntelIcon.Symbol = Microsoft.UI.Xaml.Controls.Symbol.Cancel;

            progressBar.Visibility = Visibility.Collapsed;
            IntelIcon.Visibility = Visibility.Visible;
        }
        
        private async void initIntel()
        {
            progressBar.Visibility = Visibility.Visible;
            bool isNetworkAvailable = false;
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            await DoAsyncOperation();

            if (InternetConnectionProfile != null && InternetConnectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                isNetworkAvailable = true;
            if (isNetworkAvailable)
            {
                IntelIcon.Symbol = Microsoft.UI.Xaml.Controls.Symbol.Accept;
            }
            else
            {
                IntelIcon.Symbol = Microsoft.UI.Xaml.Controls.Symbol.Cancel;
            }
            progressBar.Visibility = Visibility.Collapsed;
            IntelIcon.Visibility = Visibility.Visible;

        }

        private async Task DoAsyncOperation()
        {
            await Task.Delay(2000);
        }

        private void Full_Screen(object sender, RoutedEventArgs e)
        {
            navigationView = MainWindow.navigationView;
            var toggleSwitch = sender as ToggleSwitch;
            if (isChanging_FullScreen == true)
            {
                m_appWindow = GetAppWindowForCurrentWindow();
                if (toggleSwitch.IsOn == true)
                {
                    if (m_appWindow != null)
                    {
                        m_appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                        MainWindow.current.SetAppBar(false);
                    }
                }
                else
                {
                    if (m_appWindow != null)
                    {
                        m_appWindow.SetPresenter(AppWindowPresenterKind.Default);
                        MainWindow.current.SetAppBar(true);
                    }
                        
                }
            }
        }

        private Microsoft.UI.Windowing.AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
        }

        private Microsoft.UI.Windowing.AppWindow m_appWindow;
        private static Microsoft.UI.Xaml.Controls.NavigationView navigationView;
        private static bool isChanging_ChangingNavigationView = false;
        private static bool isChanging_FullScreen = false;
        public static StorageFile matlabFunctionFile;
        public static StorageFolder matlatFunctionFolder;
        public static string folderPath;
    }
}
