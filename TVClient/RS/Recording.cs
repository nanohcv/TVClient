using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public class Recording
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Info { get; set; }
        public string Description { get; set; }
        public string ChannelName { get; set; }

        public Recording()
        {
            ID = "";
            Title = "";
            Info = "";
            Description = "";
            ChannelName = "";
        }
        public Recording(string id, string title)
        {
            ID = id;
            Title = title;
            Info = "";
            Description = "";
            ChannelName = "";
        }
    }
}
