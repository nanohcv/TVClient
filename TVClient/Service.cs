using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient
{
    public class Service
    {
        private static RS.RSSettings rss = null;
        private static RS.RecordingService rs = null;

        public static List<RS.ChannelsRoot> ChannelList { get; set; } 
        public static RS.RecordingService RS
        {
            get
            {
                return rs;
            }
        }
        public static RS.RSSettings Settings
        {
            get
            {
                return rss;
            }
            set
            {
                rss = value;
                if (rss != null)
                    rs = new RS.RecordingService(rss);
                else
                    rs = null;

            }
        }

        public static bool LocalSettings = false;

    }
}
