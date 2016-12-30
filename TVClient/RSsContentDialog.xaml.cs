using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Inhaltsdialog" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TVClient
{
    public sealed partial class RSsContentDialog : ContentDialog
    {
        List<RS.RSLocator.RSNetworkSettings> settings;

        public RS.RSLocator.RSNetworkSettings Result { get; private set; }

        public RSsContentDialog()
        {
            this.InitializeComponent();
            Result = null;
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            settings = await RS.RSLocator.FindRecordingServices();
            if(settings.Count == 0)
            {
                this.Hide();
            }
            progressRing.IsActive = false;
            progressRing.Visibility = Visibility.Collapsed;
            RSsListBox.Visibility = Visibility.Visible;
            RSsListBox.ItemsSource = settings;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private void RSsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Result = (RS.RSLocator.RSNetworkSettings)RSsListBox.SelectedItem;
            this.Hide();
        }
    }
}
