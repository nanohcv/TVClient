using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace TVClient
{
    public static class WOL
    {
        public static void WakeOnLan(string macAddress)
        {
            WakeOnLan(macAddress, IPAddress.Broadcast, 16789);
        }

        public static async void WakeOnLan(string macAddress, IPAddress ip, int port)
        {
            macAddress = macAddress.Trim();
            if (macAddress.Length == 17)
            {
                if (macAddress.Contains("-"))
                {
                    macAddress = macAddress.Replace("-", "");
                }
                if (macAddress.Contains(":"))
                {
                    macAddress = macAddress.Replace(":", "");
                }
            }
            if (macAddress.Length != 12)
            {
                return;
            }

            byte[] mac = new byte[6];
            try
            {
                int j = 0;
                for (int i = 0; i < 6; i++)
                {
                    mac[i] = byte.Parse(macAddress.Substring(j, 2), NumberStyles.HexNumber);
                    j += 2;
                }
            }
            catch { return; }

            var client = new DatagramSocket();
            IPEndPoint remoteEndPoint = new IPEndPoint(ip, port);
            var stream = await client.GetOutputStreamAsync(new HostName(remoteEndPoint.Address.ToString()), remoteEndPoint.Port.ToString());
            DataWriter writer = new DataWriter(stream);

            byte[] packet = new byte[17 * 6];
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];

            writer.WriteBytes(packet);
            await writer.FlushAsync();
            await writer.StoreAsync();

        }
    }
}
