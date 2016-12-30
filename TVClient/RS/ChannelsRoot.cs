using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public class ChannelsRoot
    {
        public string RootName { get; private set; }
        public List<ChannelGroup> ChannelGroups { get; private set; }

        public ChannelsRoot(string rootName)
        {
            RootName = rootName;
            ChannelGroups = new List<ChannelGroup>();
        }

        public List<Channel> GetAllChannels()
        {
            List<Channel> channels = new List<Channel>();

            foreach(ChannelGroup group in ChannelGroups)
            {
                channels = channels.Concat(group.Channels).ToList();
            }
            return channels;
        }

        public override string ToString()
        {
            return RootName;
        }
    }
}
