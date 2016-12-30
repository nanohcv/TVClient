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
using Windows.UI.ViewManagement;
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
    public sealed partial class TimerListPage : Page
    {
        public TimerListPage()
        {
            this.InitializeComponent();
            this.DataContext = new TimerListViewModel(Service.RS);
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButtonVisibility = base.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += TimerListPage_BackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= TimerListPage_BackRequested;
        }

        private void TimerListPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (base.Frame.CanGoBack)
            {
                e.Handled = true;
                base.Frame.GoBack();
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            this.timerFrame.Navigate(typeof(TimerPage));
            this.timerFrame.BackStack.Clear();
            this.timerFrameGrid.Visibility = Visibility.Visible;
            setVisibility();
        }

        private async void removeButton_Click(object sender, RoutedEventArgs e)
        {
            TimerListViewModel tlvm = (TimerListViewModel)this.DataContext;
            if(tlvm != null && tlvm.Timers != null)
            {
                foreach(RS.Timer timer in tlvm.Timers)
                {
                    if(timer.Selected)
                    {
                        try
                        {
                            Service.RS.DeleteTimer(timer.TimerID);
                        }
                        catch(Exception ex)
                        {
                            MessageDialog dlg = new MessageDialog(ex.Message);
                            await dlg.ShowAsync();
                        }
                    }
                        
                }
                tlvm.SetTimers();
            }
        }

        private void lbx_timers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbx_timers.SelectedIndex != -1)
            {
                this.timerFrame.Navigate(typeof(TimerPage), lbx_timers.SelectedItem);
                this.timerFrame.BackStack.Clear();
                lbx_timers.SelectedIndex = -1;
                this.timerFrameGrid.Visibility = Visibility.Visible;
                setVisibility();
            }
        }

        public void HideTimerGrid()
        {
            timerFrameGrid.Visibility = Visibility.Collapsed;
            setVisibility();
        }

        private void setVisibility()
        {
            if (this.ActualWidth < 900 && timerFrameGrid.Visibility == Visibility.Visible)
            {
                timerListGrid.Visibility = Visibility.Collapsed;
                timerListCol.Width = new GridLength(0);
                timerCol.Width = new GridLength(1, GridUnitType.Star);
            }
            else if(this.ActualWidth < 900 && timerFrameGrid.Visibility == Visibility.Collapsed)
            {
                timerListGrid.Visibility = Visibility.Visible;
                timerListCol.Width = new GridLength(1, GridUnitType.Star);
                timerCol.Width = new GridLength(0);
            }
            else
            {
                timerListGrid.Visibility = Visibility.Visible;
                timerListCol.Width = new GridLength(1, GridUnitType.Star);
                timerCol.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setVisibility();
        }
    }
}
