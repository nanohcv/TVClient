using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using System.Net;
using System.Diagnostics;
using Windows.Networking;
using System.Xml.Serialization;

namespace TVClient.RS
{
    public class RSSettings
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string LanProtocol { get; set; }
        public string LanAddress { get; set; }
        public string LanIP { get; set; }
        public string LanPort { get; set; }
        public string WanProtocol { get; set; }
        public string WanAddress { get; set; }
        public string WanPort { get; set; }
        public string DirectStreamingProtocol { get; set; }
        public string DirectStreamingAddress { get; set; }
        public string DirectStreamingPort { get; set; }
        public bool UseDirectStreaming { get; set; }
        private string profileLan;
        private string profileWan;
        private string profileAudio;
        public string ProfileLan
        {
            get { return profileLan; }
            set
            {
                if(value != null)
                {
                    profileLan = value;
                }
            }
        }
        public string ProfileWan
        {
            get { return profileWan; }
            set
            {
                if(value != null)
                {
                    profileWan = value;
                }
            }
        }
        public string ProfileAudio
        {
            get { return profileAudio; }
            set
            {
                if(value != null)
                {
                    profileAudio = value;
                }
            }
        }
        public string MAC_Lan { get; set; }
        public string WOL_Port { get; set; }
        public string StartupTime { get; set; }
        public int LastChannelRootIndex { get; set; }

        public List<string> CustomTaskCmds { get; set; }

        private bool connectionChanged = true;

        [XmlIgnoreAttribute]
        public bool ConnectionChanged
        {
            get { return connectionChanged; }
            set { connectionChanged = value; }
        }

        private string serverUri = null;

        public string UrlEncodedLanProfile()
        {
            return WebUtility.UrlEncode(ProfileLan);
        }

        public string UrlEncodedWanProfile()
        {
            return WebUtility.UrlEncode(ProfileWan);
        }

        public string UrlEncodedAudioProfile()
        {
            return WebUtility.UrlEncode(ProfileAudio);
        }

        public RSSettings()
        {
            User = "DVBUser";
            Password = "";
            LanProtocol = "http://";
            LanAddress = "";
            LanIP = "0.0.0.0";
            LanPort = "8089";
            WanProtocol = "http://";
            WanAddress = "srv.dyndns.org";
            WanPort = "8089";
            DirectStreamingProtocol = "http://";
            DirectStreamingAddress = LanAddress;
            DirectStreamingPort = "7522";
            UseDirectStreaming = false;
            ProfileLan = "TS High 1800 kbit";
            ProfileWan = "TS Low 800 kbit";
            ProfileAudio = "TS Audio 128 kbit";
            MAC_Lan = "00:00:00:00:00:00";
            WOL_Port = "16789";
            StartupTime = "30";
            LastChannelRootIndex = 0;
            Network.InternetConnectionChanged += Network_InternetConnectionChanged;
        }

        private void Network_InternetConnectionChanged(object sender, InternetConnectionChangedEventArgs e)
        {
            connectionChanged = e.IsConnected;
        }

        public async Task<string> GetServerUri()
        {
            if(connectionChanged)
            {
                StreamSocket client = new StreamSocket();
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(1500);
                HostName host = new HostName(LanIP);
                Windows.Foundation.IAsyncAction op = client.ConnectAsync(host, LanPort);
                Task t = op.AsTask(cts.Token);
                try
                {
                    await t;
                }
                catch
                {
                    serverUri = WanProtocol + WanAddress + ":" + WanPort;
                    connectionChanged = false;
                    return serverUri;
                }
                connectionChanged = false;
                serverUri = LanProtocol + LanIP + ":" + LanPort;
                return serverUri;
            }
            return serverUri;
        }

        public async Task<string> GetStreamingUri(IChannel channel)
        {
            StreamSocket client = new StreamSocket();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(1500);
            Windows.Foundation.IAsyncAction op = client.ConnectAsync(new Windows.Networking.HostName(LanIP), LanPort);
            Task t = op.AsTask(cts.Token);
            try
            {
                await t;
            }
            catch
            {
                if(channel.Flags == "16" || channel.Flags == "17" || channel.Flags == "20")
                {
                    return WanProtocol + WanAddress + ":" + WanPort + "/flashstream/stream.ts?Preset=" + UrlEncodedAudioProfile() + "&ffPreset=medium&chid=" + channel.ChannelID;
                }
                return WanProtocol + WanAddress + ":" + WanPort + "/flashstream/stream.ts?Preset=" + UrlEncodedWanProfile() + "&aspect=16%3A9&ffPreset=medium&maxwidth=&maxheight=&chid=" + channel.ChannelID;
            }
            if(UseDirectStreaming)
            {
                return DirectStreamingProtocol + DirectStreamingAddress + ":" + DirectStreamingPort + "/upnp/channelstream/" + channel.ChannelID + ".ts";
            }
            if (channel.Flags == "16" || channel.Flags == "17" || channel.Flags == "20")
            {
                return LanProtocol + LanIP + ":" + LanPort + "/flashstream/stream.ts?Preset=" + UrlEncodedAudioProfile() + "&ffPreset=medium&chid=" + channel.ChannelID;
            }
            return LanProtocol + LanIP + ":" + LanPort + "/flashstream/stream.ts?Preset=" + UrlEncodedLanProfile() + "&aspect=16%3A9&ffPreset=medium&maxwidth=&maxheight=&chid=" + channel.ChannelID;
        }

        public async Task<string> GetRecordingStreamingUri(Recording recording, int start)
        {
            StreamSocket client = new StreamSocket();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(1500);
            Windows.Foundation.IAsyncAction op = client.ConnectAsync(new Windows.Networking.HostName(LanIP), LanPort);
            Task t = op.AsTask(cts.Token);
            try
            {
                await t;
            }
            catch
            {
                return WanProtocol + WanAddress + ":" + WanPort + "/flashstream/stream.ts?Preset=" + UrlEncodedWanProfile() + "&ffPreset=medium&recid=" + recording.ID + "&start=" + start.ToString();
            }
            return LanProtocol + LanIP + ":" + LanPort + "/flashstream/stream.ts?Preset=" + UrlEncodedLanProfile() + "&ffPreset=medium&recid=" + recording.ID + "&start=" + start.ToString();
        }
    }
}
