using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace TVClient.RS
{
    public static class RSLocator
    {
        public class RSNetworkSettings
        {
            public string IP { get; private set; }
            public string Port { get; private set; }

            public RSNetworkSettings(string ip, string port)
            {
                IP = ip;
                Port = port;
            }

            public override string ToString()
            {
                return "RS(" + IP + ")";
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                RSNetworkSettings rsn = (RSNetworkSettings)obj;
                if (rsn == null)
                    return false;
                if (rsn.IP == this.IP && rsn.Port == this.Port)
                    return true;
                return false; ;
            }
            public override int GetHashCode()
            {
                return IP.GetHashCode() ^ Port.GetHashCode();
            }
        }

        private static DatagramSocket socket;

        public static async Task<List<RSNetworkSettings>> FindRecordingServices()
        {
            socket = new DatagramSocket();
            List<RSNetworkSettings> recordingServices = new List<RSNetworkSettings>();
            List<string> urls = new List<string>();
            socket.MessageReceived += (sender, args) =>
            {
                DataReader reader = args.GetDataReader();

                uint count = reader.UnconsumedBufferLength;
                string data = reader.ReadString(count);
                if (data.Contains("DVBViewer") && data.Contains("description.xml"))
                {
                    int idx_start = data.IndexOf("LOCATION: ") + 10;
                    int idx_end = data.IndexOf("/description.xml") + 16;
                    string adr = data.Substring(idx_start, idx_end - idx_start);
                    if(!urls.Contains(adr))
                        urls.Add(adr);
                }


            };
            await socket.BindEndpointAsync(null, "");
            HostName hostName = new HostName("239.255.255.250");
            socket.JoinMulticastGroup(hostName);

            string message = "M-SEARCH * HTTP/1.1\r\n" +
                                   "HOST: " + hostName.DisplayName + ":1900\r\n" +
                                   "ST: urn:ses-com:device:SatIPServer:1\r\n" +
                                   "MAN: \"ssdp:discover\"\r\n" +
                                   "MX: 3\r\n\r\n";

            IOutputStream stream = await socket.GetOutputStreamAsync(hostName, "1900");
            var writer = new DataWriter(stream) { UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8 };
            writer.WriteString(message);
            await writer.StoreAsync();
            await Task.Delay(3000);
            foreach(string url in urls)
            {
                try
                {
                    HttpClient client = new HttpClient();
                    string description = await client.GetStringAsync(url);
                    int posURLBase = description.IndexOf("<URLBase>") + 16;
                    int posEndURLBase = description.IndexOf("</URLBase>");
                    string adr = description.Substring(posURLBase, posEndURLBase - posURLBase);
                    var adrParts = adr.Split(':');
                    recordingServices.Add(new RSNetworkSettings(adrParts[0], adrParts[1]));
                }
                catch { }
            }
            return recordingServices;
        }


    }
}
