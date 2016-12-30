using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
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
    public sealed partial class RecordingViewPage : Page
    {
        private MainPage mainpage;
        private bool mobileViewState = false;
        private InputPane inputPane;
        private DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        private DisplayInformation displayInformation;

        public RecordingViewPage()
        {
            this.InitializeComponent();
            inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;
            mediaPlayer.IsFullScreenEnabled = true;
            mediaPlayer.IsFullScreenVisible = true;
            mediaPlayer.IsStopEnabled = true;
            mediaPlayer.IsStopVisible = true;
            mediaPlayer.Stopped += MediaPlayer_Stopped;
            mediaPlayer.IsFullScreenChanged += MediaPlayer_IsFullScreenChanged;
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
        }

        private void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            if (sender.CurrentOrientation == DisplayOrientations.Landscape)
            {
                mediaPlayer.IsFullScreen = true;
            }
            else if (sender.CurrentOrientation == DisplayOrientations.Portrait)
            {
                mediaPlayer.IsFullScreen = false;
            }
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            recordingsGrid.Height = double.NaN;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            recordingsGrid.Height = Windows.UI.Xaml.Window.Current.Bounds.Height - sender.OccludedRect.Height;
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.CurrentState == MediaElementState.Playing)
            {
                if (mediaPlayer.IsAudioOnly)
                {
                    mediaPlayer.IsInteractive = true;
                    mediaPlayer.AutoHide = false;
                    mediaPlayer.IsPlayPauseVisible = false;
                    mediaPlayer.IsFullScreenVisible = false;
                }
            }
            else if(mediaPlayer.CurrentState == MediaElementState.Opening)
            {
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                {
                    col1.Width = new GridLength(1, GridUnitType.Star);
                    col2.Width = new GridLength(0);
                    row1.Height = GridLength.Auto;
                    row2.Height = GridLength.Auto;
                    row3.Height = new GridLength(1, GridUnitType.Star);
                    mobileViewState = true;
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;
                    displayInformation = DisplayInformation.GetForCurrentView();
                    displayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
                }
            }
            else
            {
                mediaPlayer.AutoHide = true;
                mediaPlayer.IsPlayPauseVisible = true;
                mediaPlayer.IsFullScreenVisible = true;
            }
        }

        private void MediaPlayer_IsFullScreenChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (mediaPlayer.IsFullScreen)
            {
                mainpage.FullScreen(true);
                viewBox.Margin = new Thickness(0, 0, 0, 0);
                col1.Width = new GridLength(1, GridUnitType.Star);
                col2.Width = new GridLength(0);
                row1.Height = new GridLength(1, GridUnitType.Star);
                row2.Height = new GridLength(0);
                row3.Height = new GridLength(0);
                mainGrid.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                _timer.Tick += _timer_Tick;
                this.PointerMoved += RecordingViewPage_PointerMoved;
                _timer.Start();
            }
            else
            {
                mainpage.FullScreen(false);
                mainGrid.Background = new SolidColorBrush(Color.FromArgb(255, 5, 0, 53));
                viewBox.Margin = new Thickness(10, 10, 10, 0);
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                {
                    col1.Width = new GridLength(1, GridUnitType.Star);
                    col2.Width = new GridLength(0);
                    row1.Height = GridLength.Auto;
                    row2.Height = GridLength.Auto;
                    row3.Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    col1.Width = new GridLength(1, GridUnitType.Star);
                    col2.Width = new GridLength(450);
                    row1.Height = GridLength.Auto;
                    row2.Height = GridLength.Auto;
                    row3.Height = new GridLength(1, GridUnitType.Star);
                }
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                _timer.Tick -= _timer_Tick;
                this.PointerMoved -= RecordingViewPage_PointerMoved;
                _timer.Stop();
            }
        }

        private void RecordingViewPage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            _timer.Start();
        }

        private void _timer_Tick(object sender, object e)
        {
            Window.Current.CoreWindow.PointerCursor = null;
        }

        private void MediaPlayer_Stopped(object sender, EventArgs e)
        {
            (this.DataContext as RecordingViewModel).StreamSource = null;
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += RecordingViewPage_BackRequested;
            mainpage = (MainPage)e.Parameter;
            base.OnNavigatedTo(e);
            SetDataContext();
        }

        private async void SetDataContext()
        {
            List<RS.Recording> recs = new List<RS.Recording>();
            try
            {
                recs = await Service.RS.GetRecordings();
                DataContext = new RecordingViewModel(recs);
            }
            catch(Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message);
                await dlg.ShowAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= RecordingViewPage_BackRequested;
            inputPane.Showing -= InputPane_Showing;
            inputPane.Hiding -= InputPane_Hiding;
            base.OnNavigatedFrom(e);
        }

        private void RecordingViewPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (mobileViewState)
            {
                mediaPlayer.IsFullScreen = false;
                col1.Width = new GridLength(0);
                col2.Width = new GridLength(1, GridUnitType.Star);
                (this.DataContext as RecordingViewModel).StreamSource = null;
                mobileViewState = false;
                e.Handled = true;
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
            else
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
        }
    }
}
