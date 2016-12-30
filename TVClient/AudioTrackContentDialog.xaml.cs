using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class AudioTrackContentDialog : ContentDialog
    {
        public RS.SubChannel SelectedSubChannel { get; private set; }

        public AudioTrackContentDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private void audioTrackListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSubChannel = ((RS.Channel)DataContext).SubChannels[audioTrackListBox.SelectedIndex];
            this.Hide();
        }
    }
}
