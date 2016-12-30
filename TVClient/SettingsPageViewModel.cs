using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TVClient
{
    public class SettingsPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private Frame mainFrame;
        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

        public bool LocalSettings { get; set; }

        public RS.RSSettings Settings { get; private set; }

        private List<string> profiles;
        public List<string> Profiles
        {
            get
            {
                return profiles;
            }
            set
            {
                profiles = value;
                NotifyPropertyChanged("ProfilesSet");
                if(profiles != null)
                {
                    if (profiles.Count > 0)
                    {
                        NotifyPropertyChanged("Profiles");
                        NotifyPropertyChanged("Settings");
                    }
                }    
            }
        }

        public bool ProfilesSet
        {
            get
            {
                if(profiles!=null)
                {
                    if(profiles.Count>0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private ICommand searchRSCommand;

        public ICommand SearchRSCommand
        {
            get
            {
                if (searchRSCommand == null)
                {
                    searchRSCommand = new RelayCommand(
                        param => this.showSearchRSDialog(),
                        param => true
                    );
                }
                return searchRSCommand;
            }
        }

        private ICommand checkSettingsCommand;

        public ICommand CheckSettingsCommand
        {
            get
            {
                if (checkSettingsCommand == null)
                {
                    checkSettingsCommand = new RelayCommand(
                        param => this.checkSettings(),
                        param => true
                    );
                }
                return checkSettingsCommand;
            }
        }

        private ICommand saveSettingsCommand;
        public ICommand SaveSettingsCommand
        {
            get
            {
                if(saveSettingsCommand == null)
                {
                    saveSettingsCommand = new RelayCommand(
                        parm => this.saveSettings(),
                        parm => true
                    );
                }
                return saveSettingsCommand;
            }
        }

        public SettingsPageViewModel(Frame mainframe)
        {
            if(Service.Settings == null)
            {
                Service.Settings = new RS.RSSettings();
            }
            Settings = Service.Settings;
            LocalSettings = Service.LocalSettings;
            mainFrame = mainframe;
        }

        private async void showSearchRSDialog()
        {
            RSsContentDialog dialog = new RSsContentDialog();
            await dialog.ShowAsync();
            if (dialog.Result != null)
            {
                Settings.LanAddress = dialog.Result.IP;
                Settings.LanPort = dialog.Result.Port;
                NotifyPropertyChanged("Settings");
            }
        }

        private async Task<bool> checkAddress(string host, string port)
        {
            StreamSocket client = new StreamSocket();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            Windows.Foundation.IAsyncAction op = client.ConnectAsync(new Windows.Networking.HostName(host), port);
            Task t = op.AsTask(cts.Token);
            try
            {
                await t;
            }
            catch
            {
                return false;
            }
            return true;
        }

        private async Task<string> resolveDNS(string hostname)
        {

            try
            {
                HostName host = new HostName(hostname);
                IReadOnlyList<EndpointPair> data = await DatagramSocket.GetEndpointPairsAsync(host, "0");
                if (data != null && data.Count > 0)
                {
                    foreach (EndpointPair item in data)
                    {
                        if (item != null && item.RemoteHostName != null &&
                                      item.RemoteHostName.Type == HostNameType.Ipv4)
                        {
                            return item.RemoteHostName.CanonicalName;
                        }
                    }
                }
            }
            catch { }
            return hostname;
        }

        private async void checkSettings()
        {
            string errors = "";
            bool lan = false;
            bool wan = false;
            Profiles = null;

            Settings.LanIP = await resolveDNS(Settings.LanAddress);

            try
            {
                lan = await checkAddress(Settings.LanIP, Settings.LanPort);
            }
            catch
            {
                try
                {
                    wan = await checkAddress(Settings.WanAddress, Settings.WanPort);
                }
                catch { }
            }
            
            
            bool credentials = false;
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(Settings.User, Settings.Password);
            HttpClient client = new HttpClient(handler);
            try
            {
                if (lan)
                {
                    string version = await client.GetStringAsync("http://" + Settings.LanIP + ":" + Settings.LanPort + "/api/version.html");
                }
                else
                {
                    string version = await client.GetStringAsync("http://" + Settings.WanAddress + ":" + Settings.WanPort + "/api/version.html");
                }

                credentials = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Access Denied"))
                {
                    credentials = false;
                }
            }
            if (!lan)
            {

                errors += " - " + resourceLoader.GetString("checkLan") + "\r\n";
            }
            if (!wan)
            {
                errors += " - " + resourceLoader.GetString("checkWan") + "\r\n";
            }
            if (!credentials)
            {
                errors += " - " + resourceLoader.GetString("checkCredentials") + "\r\n";
            }

            if((lan || wan) && credentials)
            {
                string url = "";
                if (lan)
                    url = "http://" + Settings.LanIP + ":" + Settings.LanPort + "/api/getconfigfile.html?file=Config%5Cffmpegprefs.ini";
                else
                    url = "http://" + Settings.WanAddress + ":" + Settings.WanPort + "/api/getconfigfile.html?file=Config%5Cffmpegprefs.ini";
                try
                {
                    string ffmpegprefs = await client.GetStringAsync(url);

                    Regex r = new Regex(@"[\s\t]*\[([^\[\]]*)\][\s\t]*\r?\n[\s\t]*Cmd=");
                    MatchCollection matches = r.Matches(ffmpegprefs);
                    List<string> profiles = new List<string>();
                    for (int i = 0; i < matches.Count; i++)
                    {
                        profiles.Add(matches[i].Groups[1].Value);
                    }
                    Profiles = profiles;
                }
                catch (Exception ex)
                {
                    MessageDialog dialog = new MessageDialog(ex.Message);
                    await dialog.ShowAsync();
                }
            }
            else
            {
                Profiles = null;
                MessageDialog dialog = new MessageDialog(errors);
                await dialog.ShowAsync();
            }
        }

        private void saveSettings()
        {
            if (LocalSettings)
            {
                SettingsSerializer.WriteToLocalSettings(Settings);
            }
            else
            {
                SettingsSerializer.WriteToRoamingSettings(Settings);
            }
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["LocalSettings"] = LocalSettings;
            Service.LocalSettings = LocalSettings;

            mainFrame.Navigate(typeof(TVPage));
        }

    }
}
