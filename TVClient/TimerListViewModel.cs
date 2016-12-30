using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace TVClient
{
    public class TimerListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        RS.RecordingService recsvc;
        List<RS.Timer> timers;

        public List<RS.Timer> Timers
        {
            get { return timers; }
        }

        public TimerListViewModel(RS.RecordingService rs)
        {
            recsvc = rs;
            SetTimers();
        }

        public async void SetTimers()
        {
            try
            {
                timers = await recsvc.GetTimers();
                NotifyPropertyChanged("Timers");
            }
            catch(Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message);
                await dlg.ShowAsync();
            }
        }
    }
}
