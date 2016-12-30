using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TVClient
{
    public class EPGSearchViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public delegate void ShowSearchDelegete();
        public ShowSearchDelegete ShowSearch;

        public string SearchText { get; set; }

        public bool TitleChecked { get; set; }
        public bool InfoChecked { get; set; }
        public bool DescChecked { get; set; }

        public bool CaseChecked { get; set; }
        public bool RegExChecked { get; set; }

        public bool DateChecked { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTimeOffset StopDate { get; set; }
        public TimeSpan StopTime { get; set; }

        public bool ChannelChecked { get; set; }

        private List<RS.ChannelsRoot> channelList;
        private RS.ChannelsRoot selectedChannelRoot;
        private List<RS.ChannelGroup> channelGroups;
        private RS.ChannelGroup selectedChannelGroup;
        private List<RS.Channel> channels;

        public List<RS.ChannelsRoot> ChannelList
        {
            get { return channelList; }
        }

        public RS.ChannelsRoot SelectedChannelList
        {
            get
            {
                return selectedChannelRoot;
            }
            set
            {
                selectedChannelRoot = value;
                channelGroups = selectedChannelRoot.ChannelGroups;
                NotifyPropertyChanged("ChannelGroups");
                selectedChannelGroup = channelGroups[0];
                NotifyPropertyChanged("SelecedChannelGroup");
                channels = selectedChannelGroup.Channels;
                NotifyPropertyChanged("Channels");
                SelectedChannel = channels[0];
                NotifyPropertyChanged("SelectedChannel");
            }
        }
        public List<RS.ChannelGroup> ChannelGroups
        {
            get { return channelGroups; }
        }

        public RS.ChannelGroup SelectedChannelGroup
        {
            get { return selectedChannelGroup; }
            set
            {
                selectedChannelGroup = value;
                channels = selectedChannelGroup.Channels;
                NotifyPropertyChanged("Channels");
                SelectedChannel = channels[0];
                NotifyPropertyChanged("SelectedChannel");
            }
        }

        public List<RS.Channel> Channels
        {
            get { return channels; }
        }

        public RS.Channel SelectedChannel { get; set; }

        public List<RS.EPGData> EPGSearchResult { get; private set; }

        public EPGSearchViewModel()
        {
            StartDate = new DateTimeOffset(DateTime.Now);
            StartTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            StopDate = new DateTimeOffset(DateTime.Now);
            StopTime = StartTime;
            TitleChecked = true;
            InfoChecked = true;
            if(Service.ChannelList != null)
            {
                channelList = Service.ChannelList;
                SelectedChannelList = channelList[0];
                NotifyPropertyChanged("SelectedChannelList");
            }
        }

        public async void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            string options = "";
            DateTime start = DateTime.MinValue;
            DateTime stop = DateTime.MinValue;
            if(DateChecked)
            {
                start = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTime.Hours, StartTime.Minutes, StartTime.Seconds);
                stop = new DateTime(StopDate.Year, StopDate.Month, StopDate.Day, StopTime.Hours, StopTime.Minutes, StopTime.Seconds);
            }
            RS.Channel channel = null;
            if(ChannelChecked)
            {
                channel = SelectedChannel;
            }
            if (TitleChecked)
                options += "T";
            if (InfoChecked)
                options += "S";
            if (DescChecked)
                options += "D";
            if (CaseChecked)
                options += "C";
            if (RegExChecked)
                options += "R";
            EPGSearchResult = await Service.RS.GetEPGEntries(channel, start, stop, SearchText, options);
            NotifyPropertyChanged("EPGSearchResult");
            if(ShowSearch != null)
            {
                ShowSearch();
            }
        }
    }
}
