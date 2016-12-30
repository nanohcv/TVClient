using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient
{
    public class EPGViewParameter
    {
        public List<RS.Channel> Channels { get; private set; }
        public EPGDay Day { get; private set; }

        public EPGViewParameter(List<RS.Channel> channels, EPGDay day)
        {
            Channels = channels;
            Day = day;
        }
    }
}
