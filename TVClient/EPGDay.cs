using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace TVClient
{
    public class EPGDay
    {
        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public EPGDay(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            if(DateTime.Now.Year == Start.Year && 
                DateTime.Now.Month == Start.Month && 
                DateTime.Now.Day == Start.Day)
            {
                return resourceLoader.GetString("today");
            }
            DateTime tomorrow = DateTime.Now.AddDays(1);
            if(tomorrow.Year == Start.Year &&
                tomorrow.Month == Start.Month &&
                tomorrow.Day == Start.Day)
            {
                return resourceLoader.GetString("tomorrow");
            }

            return Start.ToString("dddd");
        }
    }
}
