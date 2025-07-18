using Microsoft.Security.Authentication.OAuth;
using Microsoft.UI;
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagLib;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LocalToSpotify
{
    // Need to move methods from here to mainpage
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Current;
        public static AppWindow MyAppWindow;
        public static WindowId MyWindowId;

        public MainWindow()
        {
            this.InitializeComponent();

            MyAppWindow = this.AppWindow;
            MyWindowId = GetWindowId();
            // var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        }

        private WindowId GetWindowId()
        {
            // Get the window handle (HWND) for the current window
            var hwnd = WindowNative.GetWindowHandle(this);

            // Convert the HWND to a WindowId
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            // Get the AppWindow for the current window
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // Retrieve the WindowId
            var id = appWindow.Id;
            Debug.WriteLine($"WindowId: {id}");
            return id;
        }

        public void OnUriCallback(Uri responseUri)
        {
            if (!OAuth2Manager.CompleteAuthRequest(responseUri))
            {
                // The response is either invalid or does not correspond to any pending auth requests.
            }
            Debug.WriteLine($"responseUri: {responseUri}");
        }
    }
}
