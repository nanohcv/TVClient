using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TVClient
{
    public class RecordingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

        private int jumpTo = 0;

        public TimeSpan CurrentPosition { get; set; }

        private Uri streamSource = null;
        public Uri StreamSource
        {
            get
            {
                return streamSource;
            }
            set
            {
                streamSource = value;
                NotifyPropertyChanged("StreamSource");
            }
        }

        public class JumpOption
        {
            public string Name { get; private set; }
            public int Value { get; private set; }

            public JumpOption(string name, int val)
            {
                Name = name;
                Value = val;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public List<JumpOption> JumpOptions { get; private set; }

        public JumpOption SelectedJumpOption { get; set; }

        private ICommand jumpCommand;

        public ICommand JumpCommand
        {
            get
            {
                if (jumpCommand == null)
                {
                    jumpCommand = new RelayCommand(
                        param => this.JumpTo(),
                        param => true
                    );
                }
                return jumpCommand;
            }
        }

        private List<AlphaKeyGroup<RS.Recording>> recordings;
        public List<AlphaKeyGroup<RS.Recording>> Recordings
        {
            get { return recordings; }
        }

        private RS.Recording selectedRecording;
        public RS.Recording SelectedRecording
        {
            get
            {
                return selectedRecording;
            }
            set
            {
                selectedRecording = value;
                NotifyPropertyChanged("SelectedRecording");
                NotifyPropertyChanged("ShowLine");
                if(selectedRecording != null)
                {
                    jumpTo = 0;
                    CurrentPosition = new TimeSpan(0);
                    Stream();
                }
            }
        }

        public Visibility ShowLine
        {
            get
            {
                if(selectedRecording == null)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public RecordingViewModel(List<RS.Recording> records)
        {
            recordings = AlphaKeyGroup<RS.Recording>.CreateGroups(records,
                CultureInfo.CurrentUICulture,
                s => s.Title, true);
            JumpOptions = new List<JumpOption>();
            string minutes = resourceLoader.GetString("minutes");
            string minute = resourceLoader.GetString("minute");
            string seconds = resourceLoader.GetString("seconds");
            JumpOptions.Add(new JumpOption("-60 " + minutes, -3600));
            JumpOptions.Add(new JumpOption("-30 " + minutes, -1800));
            JumpOptions.Add(new JumpOption("-15 " + minutes, -900));
            JumpOptions.Add(new JumpOption("-10 " + minutes, -600));
            JumpOptions.Add(new JumpOption("-5 " + minutes, -300));
            JumpOptions.Add(new JumpOption("-2 " + minutes, -120));
            JumpOptions.Add(new JumpOption("-1 " + minute, -60));
            JumpOptions.Add(new JumpOption("-30 " + seconds, -30));
            JumpOptions.Add(new JumpOption("+30 " + seconds, 30));
            JumpOptions.Add(new JumpOption("+1 " + minute, 60));
            JumpOptions.Add(new JumpOption("+2 " + minutes, 120));
            JumpOptions.Add(new JumpOption("+5 " + minutes, 300));
            JumpOptions.Add(new JumpOption("+10 " + minutes, 600));
            JumpOptions.Add(new JumpOption("+15 " + minutes, 900));
            JumpOptions.Add(new JumpOption("+30 " + minutes, 1800));
            JumpOptions.Add(new JumpOption("+60 " + minutes, 3600));
            SelectedJumpOption = JumpOptions[11];
            SetStatus2();
        }

        public async void SetStatus2()
        {
            try
            {
                RS.Status2 status2 = await Service.RS.GetStatus2();
                RS.Recording rec = new RS.Recording();
                string currentRecordings = resourceLoader.GetString("currentRecordings");
                string recFiles = resourceLoader.GetString("recFiles");
                string folders = resourceLoader.GetString("folders");
                string size = resourceLoader.GetString("size");
                string free = resourceLoader.GetString("free");
                rec.Title = currentRecordings + status2.RecCount;
                rec.Info = recFiles + status2.RecFiles;
                folders += "\r\n";
                foreach (RS.Status2.Folder f in status2.RecFolders)
                {
                    folders += "    " + f.Path + "\r\n" +
                               "    " + size + (f.Size / (1024 * 1024 * 1024)) + " GB    " + free + (f.Free / (1024 * 1024 * 1024)) + " GB\r\n\r\n";
                }
                rec.Description = folders;
                selectedRecording = rec;
                NotifyPropertyChanged("SelectedRecording");
                NotifyPropertyChanged("ShowLine");
            }
            catch(Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message);
                await dlg.ShowAsync();
            }
        }

        public async void Stream()
        {
            jumpTo = (int)CurrentPosition.TotalSeconds + jumpTo;
            if (jumpTo < 0)
                jumpTo = 0;
            string url = await Service.Settings.GetRecordingStreamingUri(SelectedRecording, jumpTo);
            StreamSource = new Uri(url);
        }

        public void JumpTo()
        {
            jumpTo += SelectedJumpOption.Value;
            Stream();
        }
    }
}
