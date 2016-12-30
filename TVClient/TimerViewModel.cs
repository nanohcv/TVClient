using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace TVClient
{
    public class TimerViewModel : INotifyPropertyChanged
    {
        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        List<RS.ChannelsRoot> channelList;
        RS.ChannelsRoot selectedChannelRoot;

        public List<RS.ChannelsRoot> ChannelList
        {
            get { return channelList; }
        }

        public RS.ChannelsRoot SelectedChannelRoot
        {
            get
            {
                return selectedChannelRoot;
            }
            set
            {
                if(value!=null)
                {
                    selectedChannelRoot = value;
                    channelGroups = selectedChannelRoot.ChannelGroups;
                    selectedChannelGroup = channelGroups[0];
                    channels = selectedChannelGroup.Channels;
                    selectedChannel = channels[0];
                    timer.ChannelID = selectedChannel.ChannelID;
                    NotifyPropertyChanged("ChannelGroups");
                    NotifyPropertyChanged("SelectedChannelGroup");
                    NotifyPropertyChanged("Channels");
                    NotifyPropertyChanged("SelectedChannel");
                }
            }
        }

        List<RS.ChannelGroup> channelGroups;
        RS.ChannelGroup selectedChannelGroup;

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
                if(value != null)
                {
                    selectedChannelGroup = value;
                    channels = selectedChannelGroup.Channels;
                    selectedChannel = channels[0];
                    timer.ChannelID = selectedChannel.ChannelID;
                    NotifyPropertyChanged("Channels");
                    NotifyPropertyChanged("SelectedChannel");
                }
            }
        }

        List<RS.Channel> channels;
        RS.Channel selectedChannel;

        public List<RS.Channel> Channels
        {
            get
            {
                return channels;
            }
        }

        public RS.Channel SelectedChannel
        {
            get
            {
                return selectedChannel;
            }
            set
            {
                if(value != null)
                {
                    selectedChannel = value;
                    timer.ChannelID = selectedChannel.ChannelID;
                }
            }
        }

        RS.Timer timer;

        public RS.Timer Timer
        {
            get
            {
                return timer;
            }
        }

        Visibility channelsVisible  = Visibility.Visible;

        public Visibility ChannelsVisible
        {
            get
            {
                return channelsVisible;
            }
        }

        public List<string> TimerActions
        {
            get
            {
                List<string> actions = new List<string>();
                actions.Add(resourceLoader.GetString("actionNone"));
                actions.Add(resourceLoader.GetString("actionPowerOff"));
                actions.Add(resourceLoader.GetString("actionStandby"));
                actions.Add(resourceLoader.GetString("actionHilbernate"));
                return actions;
            }
        }

        public TimerViewModel(List<RS.ChannelsRoot> chList, RS.Timer _timer)
        {
            channelList = chList;
            if(channelList != null)
            {
                selectedChannelRoot = channelList[0];
                channelGroups = selectedChannelRoot.ChannelGroups;
                selectedChannelGroup = channelGroups[0];
                channels = selectedChannelGroup.Channels;
                selectedChannel = channels[0];
            }
            if(_timer != null)
            {
                timer = _timer;
                if (timer.TimerID == null)
                {
                    channelsVisible = Visibility.Collapsed;
                }
                else
                {
                    bool found = false;
                    for(int i = 0; i<channelList.Count; i++)
                    {
                        for(int j = 0; j<channelList[i].ChannelGroups.Count; j++)
                        {
                            for(int k = 0; k < channelList[i].ChannelGroups[j].Channels.Count; k++)
                            {
                                if(channelList[i].ChannelGroups[j].Channels[k].ChannelID == timer.ChannelID)
                                {
                                    
                                    selectedChannelRoot = channelList[i];
                                    channelGroups = selectedChannelRoot.ChannelGroups;
                                    selectedChannelGroup = channelList[i].ChannelGroups[j];
                                    channels = selectedChannelGroup.Channels;
                                    selectedChannel = channelList[i].ChannelGroups[j].Channels[k];
                                    found = true;
                                    break;
                                }
                            }
                            if (found)
                                break;
                        }
                        if (found)
                            break;
                    }
                }
            }
            else
            {
                timer = new RS.Timer();
                try
                {
                    timer.ChannelID = selectedChannel.ChannelID;
                }
                catch { }
            }
        }
    }
}
