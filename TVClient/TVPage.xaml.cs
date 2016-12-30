using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TVClient
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class TVPage : Page
    {
        private MainPage mainpage;
        private RS.Channel currentChannel;
        private List<RS.Channel> currentChannels;
        private DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        private DispatcherTimer epgtimer = new DispatcherTimer();
        private DispatcherTimer currentChannelsEPGTimer = new DispatcherTimer();
        private DispatcherTimer wolTimer = new DispatcherTimer();
        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        private DisplayInformation displayInformation;

        private bool mobileViewState = false;
        InputPane inputPane;
        public TVPage()
        {
            this.InitializeComponent();
            mediaPlayer.IsFullScreenEnabled = true;
            mediaPlayer.IsFullScreenVisible = true;
            mediaPlayer.IsStopEnabled = true;
            mediaPlayer.IsStopVisible = true;
            mediaPlayer.Stopped += MediaPlayer_Stopped;
            mediaPlayer.IsFullScreenChanged += MediaPlayer_IsFullScreenChanged;
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/tv-960x540.png"));
            mediaPlayer.Background = brush;
            inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;
            epgtimer.Tick += Epgtimer_Tick;
            wolTimer.Tick += WolTimer_Tick;
            currentChannelsEPGTimer.Tick += CurrentChannelsEPGTimer_Tick;
            currentChannelsEPGTimer.Interval = new TimeSpan(0, 5, 0);
            DataContext = Service.Settings;
        }

        private void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            if(sender.CurrentOrientation == DisplayOrientations.Landscape)
            {
                mediaPlayer.IsFullScreen = true;
            }
            else if(sender.CurrentOrientation == DisplayOrientations.Portrait)
            {
                mediaPlayer.IsFullScreen = false;
            }
        }

        private void WolTimer_Tick(object sender, object e)
        {
            setChannelList();
            wolTimer.Stop();
        }

        private void CurrentChannelsEPGTimer_Tick(object sender, object e)
        {
            try
            {
                Service.RS.UpdateEPG(currentChannels);
            }
            catch { }
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            channelsGrid.Height = double.NaN;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            channelsGrid.Height = Windows.UI.Xaml.Window.Current.Bounds.Height - sender.OccludedRect.Height;
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(mediaPlayer.CurrentState.ToString());
            if(mediaPlayer.CurrentState == MediaElementState.Playing)
            {
                if(mediaPlayer.IsAudioOnly)
                {
                    mediaPlayer.IsInteractive = true;
                    mediaPlayer.AutoHide = false;
                    mediaPlayer.IsPlayPauseVisible = false;
                    mediaPlayer.IsFullScreenVisible = false;
                }
            }
            else
            {
                mediaPlayer.AutoHide = true;
                mediaPlayer.IsPlayPauseVisible = true;
                mediaPlayer.IsFullScreenVisible = true;
            }
        }

        private void MediaPlayer_Stopped(object sender, EventArgs e)
        {
            mediaPlayer.Source = null;
        }

        private void MediaPlayer_IsFullScreenChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if(mediaPlayer.IsFullScreen)
            {
                mainpage.FullScreen(true);
                viewBox.Margin = new Thickness(0, 0, 0, 0);
                col1.Width = new GridLength(1, GridUnitType.Star);
                col2.Width = new GridLength(0);
                row1.Height = new GridLength(1, GridUnitType.Star);
                row2.Height = new GridLength(0);
                mainGrid.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                _timer.Tick += _timer_Tick;
                this.PointerMoved += TVPage_PointerMoved;
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
                    row2.Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    col1.Width = new GridLength(1, GridUnitType.Star);
                    col2.Width = new GridLength(450);
                    row1.Height = GridLength.Auto;
                    row2.Height = new GridLength(1, GridUnitType.Star);
                }
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                _timer.Tick -= _timer_Tick;
                this.PointerMoved -= TVPage_PointerMoved;
                _timer.Stop();
            }
        }

        private void TVPage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            _timer.Start();
        }

        private void _timer_Tick(object sender, object e)
        {
            Window.Current.CoreWindow.PointerCursor = null;
        }

        private async void getStatus2()
        {
            try
            {
                RS.Status2 status2 = await Service.RS.GetStatus2();
                string streamingClients = resourceLoader.GetString("streamingClients");
                string blockedTuners = resourceLoader.GetString("blockedTuners");
                titleTextBlock.Text = streamingClients + status2.StreamClientCount.ToString();
                timeTextBlock.Text = blockedTuners + status2.TunerCount;
            }
            catch { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainpage = (MainPage)e.Parameter;
            getStatus2();
            BackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += TVPage_BackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= TVPage_BackRequested;
            inputPane.Showing -= InputPane_Showing;
            inputPane.Hiding -= InputPane_Hiding;
            epgtimer.Stop();
            currentChannelsEPGTimer.Stop();

        }

        

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        private void TVPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if(mobileViewState)
            {
                mediaPlayer.IsFullScreen = false;
                col1.Width = new GridLength(0);
                col2.Width = new GridLength(1, GridUnitType.Star);
                mediaPlayer.Source = null;
                mobileViewState = false;
                e.Handled = true;
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            }
            else
            {
                App.Current.Exit();
            }
            
        }

        

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            setChannelList();
            
        }

        private async void setChannelList()
        {
            try
            {
                if (Service.ChannelList == null)
                {
                    Service.ChannelList = await Service.RS.GetChannelList();
                }
                rootComboBox.ItemsSource = Service.ChannelList;
                if (Service.ChannelList.Count > Service.Settings.LastChannelRootIndex)
                {
                    rootComboBox.SelectedIndex = Service.Settings.LastChannelRootIndex;
                }
                else
                {
                    rootComboBox.SelectedIndex = 0;
                }
            }
            catch(Exception ex)
            {
                WolContentDialog dlg = new WolContentDialog(ex.Message);
                var result = await dlg.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    WOL.WakeOnLan(Service.Settings.MAC_Lan);
                    wolTimer.Interval = TimeSpan.FromSeconds(30);
                    try
                    {
                        wolTimer.Interval = TimeSpan.FromSeconds(Convert.ToDouble(Service.Settings.StartupTime));
                    }
                    catch { }
                    wolTimer.Start();
                }
            }
        }

        private void ChannelGroupsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RS.ChannelGroup group = (RS.ChannelGroup)ChannelGroupsPivot.SelectedItem;
            try
            {
                Service.RS.UpdateEPG(group.Channels);
                currentChannels = group.Channels;
                currentChannelsEPGTimer.Start();
            }
            catch { }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            RS.Channel channel = (RS.Channel)lb.SelectedItem;
            if(channel != null)
            {
                lb.SelectedIndex = -1;
                ViewChannel(channel, null);
            }
        }

        private async void ViewChannel(RS.Channel channel, RS.SubChannel subchannel)
        {
            currentChannel = channel;
            epgtimer.Stop();
            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
            {
                col1.Width = new GridLength(1, GridUnitType.Star);
                col2.Width = new GridLength(0);
                row1.Height = GridLength.Auto;
                row2.Height = new GridLength(1, GridUnitType.Star);
                mobileViewState = true;
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;
                displayInformation = DisplayInformation.GetForCurrentView();
                displayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            }
            RS.EPGData epg = null;
            try
            {
                epg = await Service.RS.GetEPGEntry(channel);
            }
            catch (Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message);
                await dlg.ShowAsync();
                return;
            }
            if (epg != null)
            {
                titleTextBlock.Text = epg.Title;
                timeTextBlock.Text = epg.Time;
                eventTextBlock.Text = epg.Event;
                descriptionTextBlock.Text = epg.Description;
                epgtimer.Interval = epg.Stop.AddSeconds(1) - DateTime.Now;
                epgtimer.Start();

            }
            ImageBrush brush = new ImageBrush();
            try
            {
                brush.ImageSource = new BitmapImage(new Uri(channel.ChannelLogo));
            }
            catch { }
            mediaPlayer.Background = brush;
            if(subchannel != null)
            {
                string url = await Service.Settings.GetStreamingUri(subchannel);
                mediaPlayer.Source = new Uri(url);
            }
            else
            {
                string url = await Service.Settings.GetStreamingUri(channel);
                mediaPlayer.Source = new Uri(url);
            }
            
        }

        private async void Epgtimer_Tick(object sender, object e)
        {
            try
            {
                var epg = await Service.RS.GetEPGEntry(currentChannel);
                if (epg != null)
                {
                    titleTextBlock.Text = epg.Title;
                    timeTextBlock.Text = epg.Time;
                    eventTextBlock.Text = epg.Event;
                    descriptionTextBlock.Text = epg.Description;
                    epgtimer.Interval = epg.Stop.AddSeconds(1) - DateTime.Now;
                }
            }
            catch { }
        }

        private void channelSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(channelSearchTextBox.Text == "")
            {
                ChannelGroupsPivot.Visibility = Visibility.Visible;
                searchListBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                ChannelGroupsPivot.Visibility = Visibility.Collapsed;
                searchListBox.Visibility = Visibility.Visible;
                var channelRoot = (RS.ChannelsRoot)rootComboBox.SelectedItem;
                if(channelRoot != null)
                {
                    var channelList = channelRoot.GetAllChannels();
                    var filteredChannels = channelList.Where(x => x.ChannelName.StartsWith(channelSearchTextBox.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                    try
                    {
                        Service.RS.UpdateEPG(filteredChannels);
                        currentChannels = filteredChannels;
                        currentChannelsEPGTimer.Start();
                    }
                    catch { }
                    searchListBox.ItemsSource = filteredChannels;
                }
            }
        }

        private async void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
            Grid g = sender as Grid;
            RS.Channel channel = g.DataContext as RS.Channel;
            AudioTrackContentDialog dlg = new AudioTrackContentDialog();
            dlg.DataContext = channel;
            ContentDialogResult result = await dlg.ShowAsync();
            if(dlg.SelectedSubChannel != null)
            {
                ViewChannel(channel, dlg.SelectedSubChannel);
            }
        }
    }
}
