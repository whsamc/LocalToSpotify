﻿using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace LocalToSpotify
{

    /// <summary>
    /// Used to start the application and handle single instance activation. Redirects activation to the registered AppInstance if the app is already running.
    /// </summary>

    public class Program
    {
        private static DispatcherQueue dispatcherQueue;

        [STAThread]
        static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            AppInstance.GetCurrent().Activated += OnActivated;

            bool isRedirect = DecideRedirection().GetAwaiter().GetResult();

            if (!isRedirect)
            {
                Microsoft.UI.Xaml.Application.Start((p) =>
                {
                    dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                    SynchronizationContext.SetSynchronizationContext(context);
                    new App();
                });
            }
        }

        private static async Task<bool> DecideRedirection()
        {
            var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("main");
            var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();

            if (!mainInstance.IsCurrent)
            {
                // Redirect the activation (and args) to the "main" instance, and exit.
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);
                return true;
            }

            return false;
        }

        // Handle Redirection
        public static void OnActivated(object sender, AppActivationArguments args)
        {
            if (args.Kind == ExtendedActivationKind.Protocol)
            {
                var protocolArgs = (ProtocolActivatedEventArgs)args.Data;
                dispatcherQueue.TryEnqueue(() =>
                {
                    if (protocolArgs.Uri.Authority == "oauthcallback")
                    {
                        Debug.WriteLine(protocolArgs.Uri);
                        // App.AppWindow.OnUriCallback(protocolArgs.Uri);
                    }
                    SetForegroundWindow(App.WindowHandle);
                });

            }
        }

        // P/Invoke declaration for SetForegroundWindow
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

    }

}
