using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class RemotePage : Page
    {
        public RemotePage()
        {
            this.InitializeComponent();
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += RemotePage_BackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= RemotePage_BackRequested;
            base.OnNavigatedFrom(e);
        }

        private void RemotePage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (base.Frame != null)
            {
                if (base.Frame.CanGoBack)
                {
                    e.Handled = true;
                    base.Frame.GoBack();
                }
            }
        }

        private async void KeyEvent(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if(computerComboBox.SelectedItem != null)
            {
                try
                {
                    Service.RS.SendCommand(computerComboBox.SelectedItem as string, btn.Tag as string);
                }
                catch(Exception ex)
                {
                    MessageDialog dlg = new MessageDialog(ex.Message);
                    await dlg.ShowAsync();
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> computerNames = null;
            try
            {
                computerNames = await Service.RS.GetComputerNames();
            }
            catch(Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message);
                await dlg.ShowAsync();
            }
            if(computerNames != null && computerNames.Count>0)
            {
                computerComboBox.ItemsSource = computerNames;
                computerComboBox.SelectedIndex = 0;
            }
        }
    }
}
