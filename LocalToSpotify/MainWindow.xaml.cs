using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TagLib;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LocalToSpotify
{
    // Need to move methods from here to mainpage
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Current;
        public static AppWindow? MyAppWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            rootFrame.Navigate(typeof(MainPage));

            /*
            Current = this;
            MyAppWindow = this.AppWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            */
        }
    }
}
