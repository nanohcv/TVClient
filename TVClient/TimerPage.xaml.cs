using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class TimerPage : Page
    {
        public TimerPage()
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
            SystemNavigationManager.GetForCurrentView().BackRequested += TimerPage_BackRequested;
            if (e.Parameter != null)
            {
                BackButtonVisibility = base.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                RS.Timer timer = e.Parameter as RS.Timer;
                this.DataContext = new TimerViewModel(Service.ChannelList, timer);
            }
            else
            {
                this.DataContext = new TimerViewModel(Service.ChannelList, null);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= TimerPage_BackRequested;
        }

        private void TimerPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if(base.Frame != null)
            {
                if (base.Frame.CanGoBack)
                {
                    e.Handled = true;
                    base.Frame.GoBack();
                }
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
                Frame.Content = null;
            var parent = VisualTreeHelper.GetParent(Frame);
            while (!(parent is Page))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            TimerListPage tlp = parent as TimerListPage;
            if (tlp != null)
            {
                tlp.HideTimerGrid();
            }
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            TimerViewModel tvm = (TimerViewModel)this.DataContext;
            if(tvm != null)
            {
                try
                {
                    Service.RS.SetTimer(tvm.Timer);
                }
                catch(Exception ex)
                {
                    MessageDialog dlg = new MessageDialog(ex.Message);
                    await dlg.ShowAsync();
                }
            }
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
                Frame.Content = null;


            var parent = VisualTreeHelper.GetParent(Frame);
            while (!(parent is Page))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            TimerListPage tlp = parent as TimerListPage;
            if(tlp != null)
            {
                tlp.HideTimerGrid();
            }
            TimerListViewModel tlvm = ((parent as Page).DataContext as TimerListViewModel);
            if(tlvm != null)
            {
                tlvm.SetTimers();
            }
        }
    }
}
