using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public class ChannelGroup
    {
        public string GroupName { get; private set; }
        public List<Channel> Channels { get; private set; }

        public ChannelGroup(string groupName)
        {
            GroupName = groupName;
            Channels = new List<Channel>();
        }

        public override string ToString()
        {
            return GroupName;
        }
    }
}
