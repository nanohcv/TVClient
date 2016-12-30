using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Resources;

namespace TVClient.RS
{
    public class RecordingService
    {
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60;
        private const int MillisPerHour = MillisPerMinute * 60;
        private const int MillisPerDay = MillisPerHour * 24;
        private const int DaysPerYear = 365;
        private const int DaysPer4Years = DaysPerYear * 4 + 1;
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        private const long DoubleDateOffset = DaysTo1899 * TicksPerDay;
        private const long OADateMinAsTicks = (DaysPer100Years - DaysPerYear) * TicksPerDay;
        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        RS.RSSettings settings;

        public RecordingService(RS.RSSettings settings)
        {
            this.settings = settings;
        }

        public async Task<List<ChannelsRoot>> GetChannelList()
        {
            List<ChannelsRoot> channelList = new List<ChannelsRoot>();
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient httpclient = new HttpClient(handler);
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("GetChannelList: GetServerUri faild...\r\n\r\n" + ex.Message);
            }
            string uriFavourites = url + "/api/getfavourites.html";
            string logourl = url;
            url = url + "/api/getchannelsxml.html?logo=1&subchannels=1";
            string xmlChannelList = "";
            try
            {
                xmlChannelList = await httpclient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception("GetChannelList: GetXMLChannelList faild...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
            string xmlFavoritesString = null;
            try
            {
                xmlFavoritesString = await httpclient.GetStringAsync(uriFavourites);
            }
            catch { }
            var xml = XDocument.Parse(xmlChannelList);

            try
            {
                foreach (XElement element in xml.Descendants("root"))
                {
                    string root = element.Attribute("name").Value;
                    ChannelsRoot chroot = new ChannelsRoot(root);

                    foreach (XElement groupElement in element.Descendants("group"))
                    {
                        string group = groupElement.Attribute("name").Value;
                        ChannelGroup chgroup = new ChannelGroup(group);

                        foreach (XElement channelElement in groupElement.Descendants("channel"))
                        {
                            string nr = channelElement.Attribute("nr").Value;
                            string name = channelElement.Attribute("name").Value;
                            string epgid = channelElement.Attribute("EPGID").Value;
                            string flags = channelElement.Attribute("flags").Value;
                            string id = channelElement.Attribute("ID").Value;
                            XElement logoEl = channelElement.Element("logo");
                            string logo = "";
                            if (logoEl != null)
                            {
                                logo = logourl + "/" + logoEl.Value;
                            }

                            Channel channel = new Channel(nr, name, logo, epgid, id, flags);
                            foreach (XElement subchannelElement in channelElement.Descendants("subchannel"))
                            {
                                string subChName = subchannelElement.Attribute("name").Value;
                                string subChId = subchannelElement.Attribute("ID").Value;
                                SubChannel subch = new SubChannel(subChName, subChId, flags);
                                channel.SubChannels.Add(subch);
                            }
                            chgroup.Channels.Add(channel);
                        }
                        chroot.ChannelGroups.Add(chgroup);
                    }
                    channelList.Add(chroot);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("GetChannelList: XMLChannelList parse error...\r\n\r\n" + ex.Message);
            }

            if (xmlFavoritesString != null)
            {
                try
                {
                    var xmlFavorites = XDocument.Parse(xmlFavoritesString);
                    List<string> favorites = new List<string>();
                    foreach (XElement elm in xmlFavorites.Descendants("entry"))
                    {
                        favorites.Add(elm.Value.Split('|')[0]);
                    }
                    if (favorites.Count > 0)
                    {
                        ChannelsRoot favRoot = new ChannelsRoot(resourceLoader.GetString("favorites"));
                        ChannelGroup favGroup = new ChannelGroup(resourceLoader.GetString("favorites"));
                        foreach (string chid in favorites)
                        {
                            foreach (ChannelsRoot chrt in channelList)
                            {
                                foreach (ChannelGroup chgrp in chrt.ChannelGroups)
                                {
                                    foreach (Channel ch in chgrp.Channels)
                                    {
                                        List<string> chids = new List<string>();
                                        chids.Add(ch.ChannelID);
                                        foreach (SubChannel subch in ch.SubChannels)
                                        {
                                            chids.Add(subch.ChannelID);
                                        }
                                        if (chids.Contains(chid))
                                        {
                                            if (!favGroup.Channels.Contains(ch))
                                            {
                                                favGroup.Channels.Add(ch);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        favRoot.ChannelGroups.Add(favGroup);
                        channelList.Add(favRoot);
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception("GetChannelList: Favorites parse error...\r\n\r\n" + ex.Message);
                }
            }

            return channelList;
        }

        public async void UpdateEPG(List<Channel> channels)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient httpclient = new HttpClient(handler);
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("UpdateEPG: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            foreach(Channel channel in channels)
            {
                string churl = url + "/api/epg.html?lvl=2&channel=" + channel.EPGID + "&start=" + ToOADate(DateTime.Now).ToString() + "&end=" + ToOADate(DateTime.Now).ToString();
                byte[] buffer = null;
                try
                {
                    buffer = await httpclient.GetByteArrayAsync(churl);
                }
                catch(Exception ex)
                {
                    throw new Exception("UpdateEPG: GetByteArrayAsync failed...\r\n" + churl + "\r\n\r\n" + ex.Message);
                }
                string epgstring = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                try
                {
                    var epgxml = XDocument.Parse(epgstring);
                    foreach (XElement elementEPG in epgxml.Descendants("title"))
                    {
                        channel.EPGTitle = elementEPG.Value;
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception("UpdateEPG: EPGXml parse error...\r\n\r\n" + ex.Message);
                }
            }
        }

        public async Task<EPGData> GetEPGEntry(Channel channel)
        {
            List<EPGData> epgEntries = await GetEPGEntries(channel, DateTime.Now, DateTime.Now);
            if (epgEntries.Count > 0)
                return epgEntries[0];
            return null;
        }

        public async Task<List<EPGData>> GetEPGEntries(Channel channel, DateTime start, DateTime stop)
        {
            return await GetEPGEntries(channel, start, stop, null, null);
        }

        public async Task<List<EPGData>> GetEPGEntries(Channel channel, DateTime start, DateTime stop, string search, string options)
        {
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("GetEPGEntries: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            url += "/api/epg.html?lvl=2";
            string channelid = null;
            bool break_channel_search = false;
            if(channel != null)
            {
                url += "&channel=" + channel.EPGID;
                channelid = channel.ChannelID;
            }
            if(start != DateTime.MinValue && stop != DateTime.MinValue)
            {
                url += "&start=" + ToOADate(start).ToString() + "&end=" + ToOADate(stop).ToString();
            }
            if(search != null && search != "")
            {
                url += "&search=" + WebUtility.UrlEncode(search);
            }
            if(search != null && search != "" && options != null && options != "")
            {
                url += "&options=" + options;
            }

            Stream answer = null;
            try
            {
                answer = await client.GetStreamAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("GetEPGEntries: GetStreamAsync failed...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
            try
            {
                var xml = XDocument.Load(answer);
                EPGData epgdata = null;
                List<EPGData> epgEntries = new List<EPGData>();
                foreach (XElement element in xml.Descendants("programme"))
                {
                    string starttime = element.Attribute("start").Value;
                    string stoptime = element.Attribute("stop").Value;
                    string epgid = element.Attribute("channel").Value;
                    string eventid = "";
                    if (element.Element("eventid") != null)
                    {
                        eventid = element.Element("eventid").Value;
                    }
                    string pdc = "";
                    if (element.Element("pdc") != null)
                        pdc = element.Element("pdc").Value;
                    string title = "";
                    if (element.Element("titles") != null)
                        title = element.Element("titles").Element("title").Value;
                    string _event = "";
                    if (element.Element("events") != null)
                    {
                        _event = element.Element("events").Element("event").Value;
                    }
                    string description = "";
                    if (element.Element("descriptions") != null)
                    {
                        description = element.Element("descriptions").Element("description").Value;
                    }
                    string channelname = epgid;
                    if(Service.ChannelList != null)
                    {
                        foreach(ChannelsRoot chroot in Service.ChannelList)
                        {
                            foreach(ChannelGroup chgrp in chroot.ChannelGroups)
                            {
                                foreach(Channel ch in chgrp.Channels)
                                {
                                    if(ch.EPGID == epgid)
                                    {
                                        channelname = ch.ChannelName;
                                        if(channelid == null)
                                            channelid = ch.ChannelID;
                                        break_channel_search = true;
                                        break;
                                    }
                                }
                                if (break_channel_search)
                                    break;
                            }
                            if (break_channel_search)
                                break;
                        }
                    }
                    break_channel_search = false;
                    epgdata = new EPGData(channelid, channelname, eventid, pdc, title, _event, description, starttime, stoptime);
                    epgEntries.Add(epgdata);
                }
                return epgEntries;
            }
            catch(Exception ex)
            {
                throw new Exception("GetEPGEntries: EPGXML parse error...\r\n\r\n" + ex.Message);
            }
        }

        public async Task<List<Channel>> GetChannelsWithEPG(List<Channel> channels, EPGDay day)
        {
            foreach(Channel channel in channels)
            {
                List<EPGData> epg = await GetEPGEntries(channel, day.Start, day.End);
                channel.EPGEntries = epg;
            }
            return channels;
        }

        public async void SetTimer(Timer timer)
        {
            string url = await settings.GetServerUri();
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            if (timer.TimerID != null)
            {
                url += "/api/timeredit.html?id=" + timer.TimerID + "&ch=" + timer.ChannelID;
            }
            else
            {
                url += "/api/timeradd.html?ch=" + timer.ChannelID;
            }

            string enabled = timer.Enabled ? "1" : "0";
            string title = WebUtility.UrlEncode(timer.Name);
            string encoding = "255";
            string dor = ((int)ToOADate(timer.Start.Date)).ToString();
            string start = ((int)(timer.Start.TimeOfDay.TotalMinutes - timer.PreRoll)).ToString();
            string stop = ((int)(timer.Start.TimeOfDay.TotalMinutes + (timer.Stop - timer.Start).TotalMinutes + timer.PostRoll)).ToString();
            url += "&dor=" + dor + "&enable=" + enabled + "&start=" + start + "&stop=" + stop + "&title=" + title + "&encoding=" + encoding;
            string days = null;
            if(timer.Days[0] || timer.Days[1] || timer.Days[2] || timer.Days[3] || timer.Days[4] || timer.Days[5] || timer.Days[6])
            {
                days = "";
                for(int i = 0; i<timer.Days.Length; i++)
                {
                    days += timer.Days[i] ? i.ToString() : "-";
                }
            }
            if(days != null)
                url += "&days=" + days;
            if(timer.Series != "")
            {
                url += "&series=" + WebUtility.UrlEncode(timer.Series);
            }
            url += "&endact=" + timer.Action.ToString();
            try
            {
                await client.GetStringAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("SetTimers: GetStringAsync failed...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
        }

        public async Task<List<Timer>> GetTimers()
        {
            List<Timer> timers = new List<Timer>();
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("GetTimers: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            url += "/api/timerlist.html?utf8=1&datetime=" + ToOADate(DateTime.Now).ToString();
            Stream stream = null;
            try
            {
                stream = await client.GetStreamAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("GetTimers: GetStreamAsync failed...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
            try
            {
                var xml = XDocument.Load(stream);
                foreach (XElement element in xml.Descendants("Timer"))
                {
                    string type = element.Attribute("Type").Value;
                    string enabled = element.Attribute("Enabled").Value;
                    string date = element.Attribute("Date").Value;
                    string start = element.Attribute("Start").Value;
                    string dur = element.Attribute("Dur").Value;
                    string days = null;
                    if (element.Attribute("Days") != null)
                    {
                        days = element.Attribute("Days").Value;
                    }
                    string descr = "";
                    if (element.Element("Descr") != null)
                    {
                        descr = element.Element("Descr").Value;
                    }
                    string series = "";
                    if (element.Element("Series") != null)
                    {
                        series = element.Element("Series").Value;
                    }
                    string id = "0";
                    if (element.Element("ID") != null)
                    {
                        id = element.Element("ID").Value;
                    }

                    string channel = "";
                    string channelid = "";
                    if (element.Element("Channel") != null)
                    {
                        channel = element.Element("Channel").Attribute("ID").Value;
                        try
                        {
                            channelid = channel.Split(new char[] { '|' }, 2)[0];
                            channel = channel.Split(new char[] { '|' }, 2)[1];
                        }
                        catch { }
                    }
                    Timer t = new Timer();
                    t.ChannelID = channelid;
                    t.ChannelName = channel;
                    t.TimerID = id;
                    t.Series = series;
                    t.Name = descr;
                    t.Enabled = enabled == "-1" ? true : false;
                    t.Start = new DateTimeOffset(DateTime.ParseExact(date + " " + start, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                    t.Stop = t.Start + TimeSpan.FromMinutes(Convert.ToInt32(dur));
                    if (days != null)
                    {
                        for (int i = 0; i < days.Length; i++)
                        {
                            t.Days[i] = days[i] == '-' ? false : true;
                        }
                    }
                    timers.Add(t);
                }
                return timers;
            }
            catch(Exception ex)
            {
                throw new Exception("GetTimers: TimerXML parse error...\r\n\r\n" + ex.Message);
            }
        }
        
        public async void DeleteTimer(string timerId)
        {
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("DeleteTimer: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            url += "/api/timerdelete.html?id=" + timerId;
            try
            {
                await client.GetStringAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("DeleteTimer: GetStringAsync failed...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
        }

        public async Task<List<string>> GetComputerNames()
        {
            List<string> computerNames = new List<string>();
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("GetComputerNames: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            url += "/api/dvbcommand.html?time=" + ToOADate(DateTime.Now).ToString();
            Stream stream = null;
            try
            {
                stream = await client.GetStreamAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("GetComputerNames: GetStreamAsync failed...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
            try
            {
                var xml = XDocument.Load(stream);
                foreach (XElement element in xml.Descendants("target"))
                {
                    computerNames.Add(element.Value);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("GetComputerNames: XMLComputerNames parse error...\r\n\r\n" + ex.Message);
            }
            return computerNames;
        }

        public async void SendCommand(string computerName, string cmd)
        {
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("SendCommand: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            url += "/api/dvbcommand.html?target=" + computerName + "&cmd=" + "-x" + cmd + "&time=" + ToOADate(DateTime.Now).ToString();
            try
            {
                await client.GetStringAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("SendCommand: GetStringAsync failed...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
        }

        public async void StartTask(string task)
        {
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch (Exception ex)
            {
                throw new Exception("StartTask: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            url += "/api/tasks.html?task=" + task;
            try
            {
                await client.GetStringAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("StartTask: GetStringAsync failed...\r\n\r\n" + ex.Message);
            }
        }

        public async Task<Status2> GetStatus2()
        {
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("GetStatus2: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            url += "/api/status2.html?ticks=" + DateTime.Now.Ticks;
            Stream stream = null;
            try
            {
                stream = await client.GetStreamAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("GetStatus2: GetStreamAsync failed...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
            Status2 status2 = new Status2(stream);
            return status2;
        }

        public async Task<List<Recording>> GetRecordings()
        {
            List<Recording> recs = new List<Recording>();
            string url = "";
            try
            {
                url = await settings.GetServerUri();
            }
            catch(Exception ex)
            {
                throw new Exception("GetRecordings: GetServerUri failed...\r\n\r\n" + ex.Message);
            }
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(settings.User, settings.Password);
            HttpClient client = new HttpClient(handler);
            url += "/api/recordings.html?ticks=" + DateTime.Now.Ticks.ToString();
            Stream stream = null;
            try
            {
                stream = await client.GetStreamAsync(url);
            }
            catch(Exception ex)
            {
                throw new Exception("GetRecordings: GetStreamAsync failed...\r\n" + url + "\r\n\r\n" + ex.Message);
            }
            try
            {
                var xml = XDocument.Load(stream);
                foreach (XElement element in xml.Descendants("recording"))
                {
                    Recording rec = new Recording(element.Attribute("id").Value,
                                                  element.Element("title").Value);
                    if (element.Element("info") != null)
                    {
                        rec.Info = element.Element("info").Value;
                    }
                    if (element.Element("desc") != null)
                    {
                        rec.Description = element.Element("desc").Value;
                    }
                    if (element.Element("channel") != null)
                    {
                        rec.ChannelName = element.Element("channel").Value;
                    }
                    recs.Add(rec);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("GetRecordings: XMLRecordings parse error...\r\n\r\n" + ex.Message);
            }
            return recs;
        }

        private double ToOADate(DateTime datetime)
        {
            long value = datetime.Ticks;
            if (value == 0)
                return 0.0;
            if (value < TicksPerDay)
                value += DoubleDateOffset;
            if (value < OADateMinAsTicks)
                throw new OverflowException("Arg_OleAutDateInvalid");
            long millis = (value - DoubleDateOffset) / TicksPerMillisecond;
            if (millis < 0)
            {
                long frac = millis % MillisPerDay;
                if (frac != 0) millis -= (MillisPerDay + frac) * 2;
            }
            return (double)millis / MillisPerDay;
        }
    }
}
