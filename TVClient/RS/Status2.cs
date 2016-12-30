using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TVClient.RS
{
    public class Status2
    {
        public enum Rights { full, read }
        public class Folder
        {
            public long Size { get; private set; }
            public long Free { get; private set; }
            public string Path { get; private set; }

            public Folder(long size, long free, string path)
            {
                Size = size;
                Free = free;
                Path = path;
            }
        }

        public int TimerCount { get; private set; }
        public int RecCount { get; private set; }
        public int NextTimer { get; private set; }
        public int NextRec { get; private set; }
        public int StreamClientCount { get; private set; }
        public int RTSPClientCount { get; private set; }
        public int UnicastClientCount { get; private set; }
        public int LastUIAccess { get; private set; }
        public int StandbyBlock { get; private set; }
        public int TunerCount { get; private set; }
        public int StreamTunerCount { get; private set; }
        public int RecTunerCount { get; private set; }
        public int EPGUpdate { get; private set; }
        public Rights AccessRights { get; private set; }
        public int RecFiles { get; private set; }
        public List<Folder> RecFolders { get; private set; }

        public Status2(Stream xmlStream)
        {
            RecFolders = new List<Folder>();
            try
            {
                var xml = XDocument.Load(xmlStream);
                foreach (XElement element in xml.Descendants("status"))
                {
                    TimerCount = Convert.ToInt32(element.Element("timercount").Value);
                    RecCount = Convert.ToInt32(element.Element("reccount").Value);
                    NextTimer = Convert.ToInt32(element.Element("nexttimer").Value);
                    NextRec = Convert.ToInt32(element.Element("nextrec").Value);
                    StreamClientCount = Convert.ToInt32(element.Element("streamclientcount").Value);
                    RTSPClientCount = Convert.ToInt32(element.Element("rtspclientcount").Value);
                    UnicastClientCount = Convert.ToInt32(element.Element("unicastclientcount").Value);
                    LastUIAccess = Convert.ToInt32(element.Element("lastuiaccess").Value);
                    StandbyBlock = Convert.ToInt32(element.Element("standbyblock").Value);
                    TunerCount = Convert.ToInt32(element.Element("tunercount").Value);
                    StreamTunerCount = Convert.ToInt32(element.Element("streamtunercount").Value);
                    RecTunerCount = Convert.ToInt32(element.Element("rectunercount").Value);
                    EPGUpdate = Convert.ToInt32(element.Element("epgudate").Value);
                    string rights = element.Element("rights").Value;
                    if (rights == "full")
                        AccessRights = Rights.full;
                    else
                        AccessRights = Rights.read;
                    RecFiles = Convert.ToInt32(element.Element("recfiles").Value);
                    foreach(XElement el in element.Elements("recfolders"))
                    {
                        long size = Convert.ToInt64(el.Element("folder").Attribute("size").Value);
                        long free = Convert.ToInt64(el.Element("folder").Attribute("free").Value);
                        string path = el.Value;
                        RecFolders.Add(new Folder(size, free, path));
                    }
                }
            }
            catch { }
        }

        public override string ToString()
        {
            string activeTimers = "Active timers: ";
            string currentRecordings = "Current recordings: ";
            string nextTimer = "Next timer starts in: ";
            string streamingClients = "Connected streaming clients: ";
            string blockedTuners = "Occupied tuners: ";
            string recFiles = "Number of recording files: ";
            string folders = "Recording folders:";
            string size = "Size: ";
            string free = "Free: ";

            string s = activeTimers + TimerCount + "\r\n";
            s += currentRecordings + RecCount + "\r\n";
            s += nextTimer + NextTimer + "\r\n";
            s += streamingClients + StreamClientCount + "\r\n";
            s += blockedTuners + TunerCount + "\r\n";
            s += recFiles + RecFiles + "\r\n";
            s += folders + "\r\n";
            foreach (Folder f in RecFolders)
            {
                s += "    " + f.Path + "\r\n" +
                           "    " + size + f.Size + " GB    " + free + f.Free + " GB\r\n\r\n";
            }


            return s;
        }
    }
}
