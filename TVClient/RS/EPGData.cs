using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public class EPGData
    {
        public string ChannelID { get; set; }
        public string ChannelName { get; set; }
        public string EventId { get; set; }
        public string PDC { get; set; }
        public string Title { get; set; }
        public string Event { get; set; }
        public string Description { get; set; }
        public string Time { get; private set; }

        public DateTime Start { get; private set; }
        public DateTime Stop { get; private set; }

        // Konvertiert die Zeiten aus der EPG-XML in folgendes Format: Mo 19:30  -  20:00
        public void SetTime(string start, string stop)
        {
            string year = start.Substring(0, 4);
            string month = start.Substring(4, 2);
            string day = start.Substring(6, 2);
            string hour = start.Substring(8, 2);
            string minutes = start.Substring(10, 2);
            string secounds = start.Substring(12, 2);
            string year1 = stop.Substring(0, 4);
            string month1 = stop.Substring(4, 2);
            string day1 = stop.Substring(6, 2);
            string hour1 = stop.Substring(8, 2);
            string minutes1 = stop.Substring(10, 2);
            string secounds1 = stop.Substring(12, 2);

            DateTime dtStart = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day),
                                 Convert.ToInt32(hour), Convert.ToInt32(minutes), Convert.ToInt32(secounds));
            DateTime dtStop = new DateTime(Convert.ToInt32(year1), Convert.ToInt32(month1), Convert.ToInt32(day1),
                                 Convert.ToInt32(hour1), Convert.ToInt32(minutes1), Convert.ToInt32(secounds1));
            Start = dtStart;
            Stop = dtStop;
            Time = dtStart.ToString("ddd HH:mm") + "  -  " + dtStop.ToString("HH:mm");

        }

        public EPGData(string channelID, string channelName, string eventid, string pdc, string title, string _event, string description, string start, string stop)
        {
            ChannelID = channelID;
            ChannelName = channelName;
            EventId = eventid;
            PDC = pdc;
            Title = title;
            Event = _event;
            Description = description;
            SetTime(start, stop);

        }
    }
}
