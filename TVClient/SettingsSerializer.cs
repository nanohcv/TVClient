using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TVClient
{
    public static class SettingsSerializer
    {
        private static string SettingsToXMLString(RS.RSSettings settings)
        {
            XmlSerializer serializer = new XmlSerializer(settings.GetType());
            StringWriter sw = new StringWriter();
            serializer.Serialize(sw, settings);
            return sw.ToString();
        }

        private static RS.RSSettings XMLStringToRSSettings(string xmlstring)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RS.RSSettings));
            StringReader sr = new StringReader(xmlstring);
            return (RS.RSSettings)serializer.Deserialize(sr);
        }

        public static void WriteToLocalSettings(RS.RSSettings settings)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["RSSettings"] = SettingsToXMLString(settings);
        }

        public static void WriteToRoamingSettings(RS.RSSettings settings)
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["RSSettings"] = SettingsToXMLString(settings);
        }

        public static RS.RSSettings ReadFromLocalSettings()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string xmlstring = (string)localSettings.Values["RSSettings"];
            if (xmlstring != null)
                return XMLStringToRSSettings(xmlstring);
            return null;
        }

        public static RS.RSSettings ReadFromRoamingSettings()
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            string xmlstring = (string)roamingSettings.Values["RSSettings"];
            if (xmlstring != null)
                return XMLStringToRSSettings(xmlstring);
            return null;
        }
    }
}
