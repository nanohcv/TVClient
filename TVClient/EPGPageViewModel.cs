using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace TVClient
{
    public class EPGPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private List<EPGDay> days;
        private EPGDay selectedDay;
        
        private List<RS.ChannelsRoot> channelList;
        private RS.ChannelsRoot selectedChannelList;

        private List<RS.ChannelGroup> channelGroups;
        private RS.ChannelGroup selectedChannelGroup;

        private EPGViewParameter parameter;

        private RS.RecordingService rs;

        public EPGPageViewModel(List<RS.ChannelsRoot> chList, RS.RecordingService recsvc)
        {
            days = new List<EPGDay>();
            
            DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime end = start.AddDays(1) + new TimeSpan(3, 59, 59);
            days.Add(new EPGDay(DateTime.Now, end));
            days.Add(new EPGDay(start.AddDays(1), end.AddDays(1)));
            days.Add(new EPGDay(start.AddDays(2), end.AddDays(2)));
            days.Add(new EPGDay(start.AddDays(3), end.AddDays(3)));
            days.Add(new EPGDay(start.AddDays(4), end.AddDays(4)));
            days.Add(new EPGDay(start.AddDays(5), end.AddDays(5)));
            days.Add(new EPGDay(start.AddDays(6), end.AddDays(6)));
            channelList = chList;
            if(channelList != null)
            {
                selectedChannelList = channelList[0];
                channelGroups = channelList[0].ChannelGroups;
                selectedChannelGroup = channelGroups[0];
            }
            selectedDay = days[0];
            parameter = null;
            rs = recsvc;
            if(channelList!=null)
                GetChannels();
        }

        public List<RS.ChannelsRoot> ChannelList
        {
            get
            {
                return channelList;
            }
        }

        public RS.ChannelsRoot SelectedChannelList
        {
            get
            {
                return selectedChannelList;
            }
            set
            {
                if (value != null)
                {
                    selectedChannelList = value;
                    channelGroups = selectedChannelList.ChannelGroups;
                    selectedChannelGroup = channelGroups[0];
                    NotifyPropertyChanged("ChannelGroups");
                    NotifyPropertyChanged("SelectedChannelGroup");
                }
            }
        }

        public List<RS.ChannelGroup> ChannelGroups
        {
            get
            {
                return channelGroups;
            }
        }

        public RS.ChannelGroup SelectedChannelGroup
        {
            get
            {
                return selectedChannelGroup;
            }
            set
            {
                if (value != null)
                {
                    selectedChannelGroup = value;
                    GetChannels();
                }
            }
        }

        public EPGViewParameter EPGParameter
        {
            get
            {
                return parameter;
            }
        }

        public List<EPGDay> Days
        {
            get
            {
                return days;
            }
        }

        public EPGDay SelectedDay
        {
            get
            {
                return selectedDay;
            }
            set
            {
                if (value != null)
                {
                    selectedDay = value;
                    GetChannels();
                }
            }
        }

        private async void GetChannels()
        {
            parameter = null;
            NotifyPropertyChanged("EPGParameter");
            try
            {
                List<RS.Channel> channels = await rs.GetChannelsWithEPG(selectedChannelGroup.Channels, selectedDay);
                parameter = new EPGViewParameter(channels, selectedDay);
                NotifyPropertyChanged("EPGParameter");
            }
            catch(Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message);
                await dlg.ShowAsync();
            }
        }

    }
}
