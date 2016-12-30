using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public interface IChannel
    {
        string ChannelName { get; }
        string ChannelID { get; }
        string Flags { get; }
    }
}
