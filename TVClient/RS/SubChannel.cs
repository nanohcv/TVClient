using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public class SubChannel : IChannel
    {
        public string ChannelName { get; private set; }
        public string ChannelID { get; private set; }
        public string Flags { get; private set; }


        public SubChannel(string name, string id, string flags)
        {
            ChannelName = name;
            ChannelID = id;
            Flags = flags;
        }

        public override string ToString()
        {
            return ChannelName;
        }
    }
}
