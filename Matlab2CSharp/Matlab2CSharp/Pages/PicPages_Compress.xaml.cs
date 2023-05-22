// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Core;
using System.Threading.Tasks;
using WinRT.Interop;
using Windows.Foundation.Metadata;
using MathWorks;
using MathWorks.MATLAB;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Windows.Devices.Radios;
using System.Net.Http;
using System.IO.Enumeration;
using System.Windows.Input;
using System.Windows;
using Windows.Graphics.Imaging;
using System.Drawing;
using System.Diagnostics;
using Windows.System;
using Windows.Storage.FileProperties;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Matlab2CSharp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PicPages_Compress : Page
    {
        public PicPages_Compress()
        {
            this.InitializeComponent();
            if(image != null)
            {
                myImage.Source = loadedBitmapImage;
                compressImage.Source = afterBitmapImage;
            }
            if(beforeSize!=0)
            {
                double result = imageBytes;
                if ((result / (1024 * 1024)) >= 1)
                {
                    result /= 1024 * 1024;
                    result = Math.Round(result, 2);
                    OriginalImage.Text = $"原始图像 大小:{result}MB";
                }
                else if (result / (1024) >= 1)
                {
                    result /= 1024;
                    result = Math.Round(result, 2);
                    OriginalImage.Text = $"原始图像 大小:{result}KB";
                }
                else
                {
                    result = Math.Round(result, 2);
                    OriginalImage.Text = $"原始图像 大小:{result}Bytes";
                }
            }
            if(afterSize!=0)
            {
                double result = afterImageBytes;
                if ((result / (1024 * 1024)) >= 1)
                {
                    result /= 1024 * 1024;
                    result = Math.Round(result, 2);
                    CompressImage.Text = $"压缩图像 大小:{result}MB";
                }
                else if (result / (1024) >= 1)
                {
                    result /= 1024;
                    result = Math.Round(result, 2);
                    CompressImage.Text = $"压缩图像 大小:{result}KB";
                }
                else
                {
                    result = Math.Round(result, 2);
                    CompressImage.Text = $"压缩图像 大小:{result}Bytes";
                }
            }
        }

        private async void ChoosePicture(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            filePicker.FileTypeFilter.Add(".png");
            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                image = file;
                filePath = file.Path;
                BitmapImage bitmapImage = new BitmapImage();
                FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
                bitmapImage.SetSource(stream);
 
                loadedBitmapImage = bitmapImage;
                myImage.Source = loadedBitmapImage;
                currentBitmapImage = bitmapImage;

                BasicProperties properties = await file.GetBasicPropertiesAsync();
                beforeSize = properties.Size;
                double result = Convert.ToDouble(beforeSize);
                imageBytes = result;
                if ((result/(1024*1024))>=1)
                {
                    result /= 1024 * 1024 ;
                    result=Math.Round(result,2);
                    OriginalImage.Text = $"原始图像 大小:{result}MB";
                }
                else if(result /(1024)>=1)
                {
                    result /= 1024;
                    result = Math.Round(result, 2);
                    OriginalImage.Text = $"原始图像 大小:{result}KB";
                }
                else
                {
                    result = Math.Round(result, 2);
                    OriginalImage.Text = $"原始图像 大小:{result}Bytes";
                }
                beforeSize = result;

            }
        }

        private void GetPicture_CSharp(object sender, RoutedEventArgs e)
        {
            /* 初始思路：找算法去压缩图像
             * 实际写代码之后的思路：全部转成jpg格式
             * 这算法没经过系统的学习压根想不出来。
             * 早知道，还是选择UWP好。哈哈。
             */
            if(image == null)
            {
                ErrorMessage_CantFindImage(sender, e);
            }
            else
            {
                BitmapImage toSaveThisFile = currentBitmapImage;
                SaveToFile(toSaveThisFile);
            }
        }

        private async void SaveToFile(BitmapImage bitmapImage)
        {
            /*原理：.Png转.Jpg
             * 感觉...不如画图工具直接转换
             * 不过效率比画图工具高
             * 转换出来的图像要比画图工具转换的还要少个5kb-10kb
             */
            FileSavePicker savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("JPEG Image", new List<string>() { ".jpg" });
            savePicker.SuggestedFileName = "New Image";
            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                IRandomAccessStream stream = await image.OpenAsync(FileAccessMode.Read);
                await writeableBitmap.SetSourceAsync(stream);
                var pixelBuffer = writeableBitmap.PixelBuffer.ToArray();

                var width = writeableBitmap.PixelWidth;
                var height = writeableBitmap.PixelHeight;
                Guid encoderId;

                switch (file.FileType.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        encoderId = BitmapEncoder.JpegEncoderId;
                        break;
                    case ".png":
                        encoderId = BitmapEncoder.PngEncoderId;
                        break;
                    default:
                        throw new ArgumentException("Invalid file type");
                }
                using (var stream2 = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(encoderId, stream2);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96.0, 96.0, pixelBuffer);
                    await encoder.FlushAsync();
                }
                compressImage.Source = bitmapImage;
                afterBitmapImage = bitmapImage;
                BasicProperties properties = await file.GetBasicPropertiesAsync();
                afterSize = properties.Size;
                double result = Convert.ToDouble(afterSize);
                afterImageBytes = result;
                if ((result / (1024 * 1024)) >= 1)
                {
                    result /= 1024 * 1024;
                    result = Math.Round(result, 2);
                    CompressImage.Text = $"压缩图像 大小:{result}MB";
                }
                else if (result / (1024) >= 1)
                {
                    result /= 1024;
                    result = Math.Round(result, 2);
                    CompressImage.Text = $"压缩图像 大小:{result}KB";
                }
                else
                {
                    result = Math.Round(result, 2);
                    CompressImage.Text = $"压缩图像 大小:{result}Bytes";
                }
                afterSize = result;
            }
           

        }
        private async void GetPicture_Matlab(object sender, RoutedEventArgs e)
        {
            /*不是很想实现自定义压缩比，好麻烦
             *受不了了，一拳把异步窗口打爆
             *先定义个常数ratio=0.5用着
             *后期慢慢加功能吧
             */
            if (image == null)
            {
                ErrorMessage_CantFindImage(sender, e);
            }
            else
            {
                matlabFunctions = Settings.matlabFunctionFile;
                if (matlabFunctions == null)
                {
                    ErrorMessage_CantFindMatlab(sender, e);
                    matlabBottonFlyout.Visibility = Visibility.Visible;
                }else if(ratio==0)
                {
                    ErrorMessage_CantFindRatio(sender, e);
                }
                else
                {
                    /*如果你想自己编写函数，进行对应操作
                    * 那么函数应该传入以下四个参数
                    *原图片路径，压缩图片路径。
                    *长压缩比m，宽压缩比n`
                    *同时函数名应与
                    *photo_compress(photo_address,save_address,m,n)
                    *保持一致
                    */
                    filePath = matlabFunctions.Path;
                    string foldPath = Settings.folderPath;
                    string fileName = matlabFunctions.Name;
                    string imageFoldPath=Path.GetDirectoryName(image.Path);
                    MLApp.MLApp matlab = new MLApp.MLApp();
                    matlab.Visible = 0;
                    matlab.Execute($"clc");
                    matlab.Execute($"clear");
                    matlab.Execute($"cd {foldPath};");
                    matlab.Execute($"m= {ratio};");
                    matlab.Execute($"n= {ratio};");
                    matlab.Execute($"file_name =\"{imageFoldPath}\";");
                    matlab.Execute($"name=\"{image.Name}\"");
                    matlab.Execute("photo_address = fullfile(file_name,name);");
                    matlab.Execute("save_address = fullfile(file_name,strcat('compress_',name));");
                    matlab.Execute("photo_compress(photo_address,save_address,m,n);");

                    string newImageName = imageFoldPath + "\\compress_" + image.Name;
                    StorageFile afterImage= await StorageFile.GetFileFromPathAsync(newImageName);
                    BitmapImage bmpImage = new BitmapImage();
                    await bmpImage.SetSourceAsync(await afterImage.OpenReadAsync());


                   
                    compressImage.Source = bmpImage;
                    afterBitmapImage = bmpImage;
                    BasicProperties properties = await afterImage.GetBasicPropertiesAsync();
                    afterSize = properties.Size;
                    double result = Convert.ToDouble(afterSize);
                    afterImageBytes = result;
                    if ((result / (1024 * 1024)) >= 1)
                    {
                        result /= 1024 * 1024;
                        result = Math.Round(result, 2);
                        CompressImage.Text = $"压缩图像 大小:{result}MB";
                    }
                    else if (result / (1024) >= 1)
                    {
                        result /= 1024;
                        result = Math.Round(result, 2);
                        CompressImage.Text = $"压缩图像 大小:{result}KB";
                    }
                    else
                    {
                        result = Math.Round(result, 2);
                        CompressImage.Text = $"压缩图像 大小:{result}Bytes";
                    }
                    afterSize = result;
                }
            }
        }

        private async void ErrorMessage_CantFindImage(object sender,RoutedEventArgs e)
        {
            ContentDialog dialog=new ContentDialog();
            dialog.Title = "Error!\n错误原因：未找到图片。\n请重新加载图片\n";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }
        private async void ErrorMessage_CantFindMatlab(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Error!\n错误原因：未能找到Matlab依赖。\n请重新加载Matlab环境\n";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }
        private async void ErrorMessage_CantFindRatio(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Error!\n错误原因：未设置压缩比。\n请设置压缩比后再进行操作\n";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }
        private void ClearPicture(object sender, RoutedEventArgs e)
        {
            if(image == null)
            {
                ErrorMessage_CantFindImage(sender, e);
            }
            image=null;
            myImage.Source = null;
            compressImage.Source = null;
            loadedBitmapImage = null;
            filePath = null;
            currentBitmapImage = null;
            beforeSize = 0;
            afterSize = 0;
            afterImageBytes = 0;
            imageBytes = 0;
            OriginalImage.Text = $"原始图像";
            CompressImage.Text = $"压缩图像";
        }
        public void LoadHelpPage(object sender, RoutedEventArgs e)
        {
            string pageName = "Matlab2CSharp.Pages." + "HelpPage";
            Type pageType = Type.GetType(pageName);
            MainWindow.currentFrame.Navigate(pageType);
        }
        private void SetRatio(object sender, RoutedEventArgs e)
        {
            RadioButton selectedButton = sender as RadioButton;
            if (selectedButton != null)
            {
                string type=selectedButton.Content.ToString();
                if(type =="25%")
                {
                    ratio = 5;
                    RatioButtonsHeader.Header = "压缩比为25%";
                }
                else if(type =="50%")
                {
                    ratio = 7;
                    RatioButtonsHeader.Header = "压缩比为50%";
                }
                else if(type =="80%")
                {
                    ratio = 9;
                    RatioButtonsHeader.Header = "压缩比为80%";
                }
               
            }
        }
        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.F2)
            {
                GetPicture_Matlab(null, null);
                //将null, null作为参数传递，因为GetPicture_Matlab方法没有使用参数
            }else if(e.Key == VirtualKey.F1)
            {
                GetPicture_CSharp(null, null);
            }
        }

      
        private static double imageBytes;
        private static double afterImageBytes;
        private static StorageFile image;
        private static BitmapImage loadedBitmapImage;
        private static BitmapImage afterBitmapImage;
        private static StorageFile matlabFunctions;
        private static string filePath;
        private static BitmapImage currentBitmapImage;
        private static int ratio;
        private static double beforeSize;
        private static double afterSize;
    }
}
