using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TVClient
{
    public sealed partial class EPGControl : UserControl
    {
        private EPGViewParameter parameter;

        private bool scrolled = false;

        
        public EPGControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if(this.DataContext != null)
            {
                parameter = this.DataContext as EPGViewParameter;
                if (parameter == null)
                    return;

                progressRing.Visibility = Visibility.Collapsed;
                epgviewScrollViewer.Visibility = Visibility.Visible;

                List<RS.Channel> channels = parameter.Channels;

                for(int i = 0; i < channels.Count; i++)
                {
                    ColumnDefinition colDef = new ColumnDefinition();
                    colDef.Width = new GridLength(200);
                    channelsGrid.ColumnDefinitions.Add(colDef);

                    Grid chGrid = new Grid();
                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/channelsGrid.png"));
                    chGrid.Background = brush;
                    ColumnDefinition chGridColDef = new ColumnDefinition();
                    chGridColDef.Width = new GridLength(200);
                    chGrid.ColumnDefinitions.Add(chGridColDef);
                    TextBlock tb = new TextBlock();
                    tb.Text = channels[i].ChannelName;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Bottom;
                    tb.Margin = new Thickness(0, 0, 0, 5);
                    Grid.SetColumn(tb, 0);
                    chGrid.Children.Add(tb);

                    Grid.SetColumn(chGrid, i);
                    channelsGrid.Children.Add(chGrid);

                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(200);
                    epgviewGrid.ColumnDefinitions.Add(cd);

                    
                    foreach(RS.EPGData entry in channels[i].EPGEntries)
                    {
                        Button btn = new Button();
                        btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                        btn.VerticalAlignment = VerticalAlignment.Top;
                        Grid btnGrid = new Grid();
                        RowDefinition rd1 = new RowDefinition();

                        StackPanel sp = new StackPanel();
                        sp.Orientation = Orientation.Vertical;

                        rd1.Height = new GridLength(1, GridUnitType.Star);
                        TextBlock title = new TextBlock();
                        title.Text = entry.Title;
                        title.TextWrapping = TextWrapping.Wrap;
                        title.FontSize = 12;
                        title.FontWeight = Windows.UI.Text.FontWeights.Bold;
                        title.Margin = new Thickness(2, 0, 0, 0);
                        title.VerticalAlignment = VerticalAlignment.Top;
                        title.HorizontalAlignment = HorizontalAlignment.Left;

                        TextBlock tbtime = new TextBlock();
                        tbtime.Text = entry.Time;
                        tbtime.FontSize = 12;
                        tbtime.Margin = new Thickness(2, 0, 0, 0);
                        tbtime.VerticalAlignment = VerticalAlignment.Top;
                        tbtime.HorizontalAlignment = HorizontalAlignment.Left;

                        sp.Children.Add(title);
                        sp.Children.Add(tbtime);

                        btn.Content = sp;
                        btn.DataContext = entry;
                        btn.Click += Btn_Click;
                        btn.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                        btn.BorderThickness = new Thickness(1);

                        TimeSpan timespan = entry.Stop - entry.Start;

                        double height = timespan.TotalMinutes * 2;
                        DateTime dtm = new DateTime(parameter.Day.Start.Year, parameter.Day.Start.Month, parameter.Day.Start.Day);
                        TimeSpan tsm = entry.Start - dtm;
                        btn.Margin = new Thickness(0, tsm.TotalMinutes * 2, 0, 0);
                        Grid.SetRow(btn, 0);

                        if (entry.Start < new DateTime(parameter.Day.Start.Year, parameter.Day.Start.Month, parameter.Day.Start.Day))
                        {
                            DateTime dt = new DateTime(entry.Stop.Year, entry.Stop.Month, entry.Stop.Day);
                            TimeSpan ts = entry.Stop - dt;
                            height = ts.TotalMinutes * 2;
                            btn.Margin = new Thickness(0, 0, 0, 0);
                            Grid.SetRow(btn, 0);
                        }
                        if (entry.Stop > parameter.Day.End)
                        {
                            TimeSpan ts = parameter.Day.End - entry.Start;
                            height = ts.TotalMinutes * 2;
                        }

                        btn.Height = height;
                        Grid.SetColumn(btn, i);
                        epgviewGrid.Children.Add(btn);
                    }
                }
            }
            else
            {
                channelsGrid.Children.Clear();
                channelsGrid.ColumnDefinitions.Clear();
                epgviewGrid.Children.Clear();
                epgviewGrid.ColumnDefinitions.Clear();
                progressRing.Visibility = Visibility.Visible;
                epgviewScrollViewer.Visibility = Visibility.Collapsed;
            }
        }

        private async void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            EPGContentDialog dlg = new EPGContentDialog();
            dlg.DataContext = btn.DataContext;
            ContentDialogResult result = await dlg.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                var parent = VisualTreeHelper.GetParent(this);
                while (!(parent is Page))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                (parent as Page).Frame.Navigate(typeof(TimerPage), new RS.Timer(btn.DataContext as RS.EPGData));
            }
        }

        private void epgviewScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            timelineScrollViewer.ChangeView(timelineScrollViewer.HorizontalOffset, epgviewScrollViewer.VerticalOffset, timelineScrollViewer.ZoomFactor);
            channelScrollViewer.ChangeView(epgviewScrollViewer.HorizontalOffset, channelScrollViewer.VerticalOffset, channelScrollViewer.ZoomFactor);
        }

        private void epgviewScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(!scrolled)
            {
                DateTime now = DateTime.Now;
                double toPos = ((now.Hour * 60) * 2);
                if (toPos > 3360 - epgviewScrollViewer.ActualHeight)
                    toPos = 3360 - epgviewScrollViewer.ActualHeight;
                epgviewScrollViewer.ChangeView(epgviewScrollViewer.HorizontalOffset, toPos, epgviewScrollViewer.ZoomFactor);
                scrolled = true;
            }
            
        }
    }
}
