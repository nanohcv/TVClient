using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public class Channel : INotifyPropertyChanged, IChannel
    {
        private string epgtitle;
        public string ChannelNr { get; private set; }
        public string ChannelName { get; private set; }
        public string ChannelLogo { get; private set; }
        public string EPGID { get; private set; }
        public string ChannelID { get; private set; }
        public List<SubChannel> SubChannels { get; private set; }
        public string Flags { get; private set; }

        public string EPGTitle
        {
            get
            {
                return this.epgtitle;
            }
            set
            {
                if (value != this.epgtitle)
                {
                    this.epgtitle = value;
                    NotifyPropertyChanged("EPGTitle");
                }
            }
        }

        public List<EPGData> EPGEntries { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public Channel(string nr, string name, string logo, string epgid, string id, string flags)
        {
            ChannelNr = nr;
            ChannelName = name;
            ChannelLogo = logo;
            EPGID = epgid;
            ChannelID = id;
            Flags = flags;
            SubChannels = new List<SubChannel>();
            epgtitle = "";
            EPGEntries = new List<EPGData>();
        }

        public override string ToString()
        {
            return ChannelName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Channel ch = obj as Channel;
            if(ch == null)
            {
                return false;
            }

            List<string> ids = new List<string>();
            ids.Add(this.ChannelID);
            foreach(SubChannel subch in this.SubChannels)
            {
                ids.Add(subch.ChannelID);
            }
            List<string> chids = new List<string>();
            chids.Add(ch.ChannelID);
            foreach(SubChannel chSubCh in ch.SubChannels)
            {
                chids.Add(chSubCh.ChannelID);
            }

            if (ids.Intersect(chids).ToList().Count > 0)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
