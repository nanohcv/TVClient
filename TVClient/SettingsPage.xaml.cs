using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TVClient
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private MainPage mainPage;
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainPage = (MainPage)e.Parameter;
            BackButtonVisibility = base.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += SettingsPage_BackRequested;
            DataContext = new SettingsPageViewModel(mainPage.MainFrame);
            base.OnNavigatedTo(e);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= SettingsPage_BackRequested;
            Service.Settings.LanIP = await resolveDNS(Service.Settings.LanAddress);
            Service.Settings.ConnectionChanged = true;
            if (Service.LocalSettings)
            {
                SettingsSerializer.WriteToLocalSettings(Service.Settings);
            }
            else
            {
                SettingsSerializer.WriteToRoamingSettings(Service.Settings);
            }
            Service.LocalSettings = (this.DataContext as SettingsPageViewModel).LocalSettings;
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["LocalSettings"] = Service.LocalSettings;
        }

        private async Task<string> resolveDNS(string hostname)
        {

            try
            {
                HostName host = new HostName(hostname);
                IReadOnlyList<EndpointPair> data = await DatagramSocket.GetEndpointPairsAsync(host, "0");
                if (data != null && data.Count > 0)
                {
                    foreach (EndpointPair item in data)
                    {
                        if (item != null && item.RemoteHostName != null &&
                                      item.RemoteHostName.Type == HostNameType.Ipv4)
                        {
                            return item.RemoteHostName.CanonicalName;
                        }
                    }
                }
            }
            catch { }
            return hostname;
        }

        private void SettingsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (base.Frame.CanGoBack)
            {
                e.Handled = true;
                base.Frame.GoBack();
            }
            else
            {
                e.Handled = true;
                mainPage.MainFrame.Navigate(typeof(TVPage));
            }
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }
    }
}
