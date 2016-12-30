using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace TVClient
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static string GetAppVersion()
        {

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("V{0}.{1}.{2}", version.Major, version.Minor, version.Build);
           

        }

        public MainPage()
        {
            this.InitializeComponent();
            this.mainPageTitle.Text = "TV Client " + GetAppVersion();
            Global.MainFrame = this.mainFrame;
        }

        public void FullScreen(bool fullscreen)
        {
            if (fullscreen)
            {
                row1.Height = new GridLength(0);
                mainSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
                mainSplitView.IsPaneOpen = false;
            }
            else
            {
                row1.Height = GridLength.Auto;
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                {
                    mainSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
                    mainSplitView.IsPaneOpen = false;
                }
                else
                {
                    mainSplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                }
            }
        }

        private void hamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = this.mainSplitView.IsPaneOpen ? false : true;
        }

        private void SettingsButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.mainFrame.Navigate(typeof(SettingsPage), this);
            this.SettingsButton.IsChecked = false;

        }

        private void TVButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.TVButton.IsChecked = false;
            this.mainFrame.Navigate(typeof(TVPage), this);
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            object obj = roamingSettings.Values["LocalSettings"];
            if(obj == null)
            {
                this.mainFrame.Navigate(typeof(SettingsPage), this);
            }
            else
            {
                bool ls = (bool)obj;
                if(ls)
                {
                    Service.Settings = SettingsSerializer.ReadFromLocalSettings();
                }
                else
                {
                    Service.Settings = SettingsSerializer.ReadFromRoamingSettings();
                }
                Service.LocalSettings = ls;
                this.mainFrame.Navigate(typeof(TVPage), this);
            }
            
        }

        private void EPGButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.mainFrame.Navigate(typeof(EPGPage), this);
            this.EPGButton.IsChecked = false;
        }

        private void TimerListButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.mainFrame.Navigate(typeof(TimerListPage), this);
            this.TimerListButton.IsChecked = false;
        }

        private void RemoteButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.mainFrame.Navigate(typeof(RemotePage), this);
            this.RemoteButton.IsChecked = false;
        }

        private void RecordingsButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.RecordingsButton.IsChecked = false;
            this.mainFrame.Navigate(typeof(RecordingViewPage), this);
        }

        private void HelpButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.HelpButton.IsChecked = false;
            this.mainFrame.Navigate(typeof(HelpPage), this);
        }

        public Frame MainFrame
        {
            get
            {
                return this.mainFrame;
            }
        }

        private void TasksButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.TasksButton.IsChecked = false;
            this.mainFrame.Navigate(typeof(TasksPage), this);
        }

        private void EPGSearchButton_Checked(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = false;
            this.EPGSearchButton.IsChecked = false;
            this.mainFrame.Navigate(typeof(EPGSearchPage), this);
        }
    }
}
