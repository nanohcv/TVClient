using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public class Timer
    {
        string timerId = null;
        bool[] days = new bool[7];
        bool enabled = true;
        string name = "";
        string series = "";
        string channelID = "";
        string channelName = "";
        DateTimeOffset start;
        DateTimeOffset stop;

        int preroll = 0;
        int postroll = 0;

        int action = 0;

        bool selected = false;

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }

        public string TimerID
        {
            get { return timerId; }
            set { timerId = value; }
        }

        public string ChannelID
        {
            get { return channelID; }
            set { channelID = value; }
        }

        public string ChannelName
        {
            get { return channelName; }
            set { channelName = value; }
        }

        public bool[] Days
        {
            get
            {
                return days;
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public string Series
        {
            get
            {
                return series;
            }
            set
            {
                series = value;
            }
        }

        public int Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }

        public DateTimeOffset Start
        {
            get { return start; }
            set { start = value; }
        }

        public TimeSpan StartTime
        {
            get
            {
                return start.TimeOfDay;
            }
            set
            {
                start = new DateTimeOffset(new DateTime(start.Year, start.Month, start.Day, value.Hours, value.Minutes, value.Seconds));
            }
        }

        public DateTimeOffset Stop
        {
            get { return stop; }
            set { stop = value; }
        }

        public TimeSpan StopTime
        {
            get
            {
                return stop.TimeOfDay;
            }
            set
            {
                stop = new DateTimeOffset(new DateTime(stop.Year, stop.Month, stop.Day, value.Hours, value.Minutes, value.Seconds));
            }
        }

        public string Time
        {
            get
            {
                return Start.DateTime.ToString("dd.MM.yyyy HH:mm") + " - " + Stop.DateTime.ToString("dd.MM.yyyy HH:mm");
            }
        }

        public int PreRoll
        {
            get
            {
                return preroll;
            }
            set
            {
                preroll = value;
            }
        }

        public int PostRoll
        {
            get
            {
                return postroll;
            }
            set
            {
                postroll = value;
            }
        }

        public Timer()
        {
            start = new DateTimeOffset(DateTime.Now);
            stop = start + new TimeSpan(1, 0, 0);
        }

        public Timer(EPGData epg)
        {
            start = new DateTimeOffset(epg.Start);
            stop = new DateTimeOffset(epg.Stop);
            channelID = epg.ChannelID;
            name = epg.Title;
        }
    }
}
