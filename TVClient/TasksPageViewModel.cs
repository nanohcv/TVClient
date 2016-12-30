using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TVClient
{
    public class TasksPageViewModel : INotifyPropertyChanged
    {
        private ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ObservableCollection<RS.RSTask> Tasks { get; private set; }

        public TasksPageViewModel()
        {
            Tasks = new ObservableCollection<RS.RSTask>();
            Tasks.Add(new RS.RSTask(false, "Shutdown", resourceLoader.GetString("shutdown")));
            Tasks.Add(new RS.RSTask(false, "Hibernate", resourceLoader.GetString("hibernate")));
            Tasks.Add(new RS.RSTask(false, "Standby", resourceLoader.GetString("standby")));
            if(Service.Settings.CustomTaskCmds != null)
            {
                for(int i=0; i<Service.Settings.CustomTaskCmds.Count; i++)
                {
                    Tasks.Add(new RS.RSTask(true, Service.Settings.CustomTaskCmds[i], Service.Settings.CustomTaskCmds[i]));
                }
            }
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if(lb != null)
            {
                if (lb.SelectedIndex != -1)
                {
                    Service.RS.StartTask((lb.SelectedItem as RS.RSTask).Cmd);
                    lb.SelectedIndex = -1;
                }
            }
        }

        public async void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            AddTaskDialog dlg = new AddTaskDialog();
            ContentDialogResult result = await dlg.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                Tasks.Add(new RS.RSTask(true, dlg.TaskCmd, dlg.TaskCmd));
                NotifyPropertyChanged("Tasks");
                Service.Settings.CustomTaskCmds = new List<string>();
                for(int i=3; i<Tasks.Count; i++)
                {
                    Service.Settings.CustomTaskCmds.Add(Tasks[i].Cmd);
                }
                if (Service.LocalSettings)
                {
                    SettingsSerializer.WriteToLocalSettings(Service.Settings);
                }
                else
                {
                    SettingsSerializer.WriteToRoamingSettings(Service.Settings);
                }
            }
        }

        public void OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Tasks.Count; i++)
            {
                if (Tasks[i].IsChecked)
                {
                    Tasks.Remove(Tasks[i]);
                    i = 0;
                }
            }
            NotifyPropertyChanged("Tasks");
            Service.Settings.CustomTaskCmds = new List<string>();
            for (int i = 3; i < Tasks.Count; i++)
            {
                Service.Settings.CustomTaskCmds.Add(Tasks[i].Cmd);
            }
            if (Service.LocalSettings)
            {
                SettingsSerializer.WriteToLocalSettings(Service.Settings);
            }
            else
            {
                SettingsSerializer.WriteToRoamingSettings(Service.Settings);
            }
        }
    }
}
