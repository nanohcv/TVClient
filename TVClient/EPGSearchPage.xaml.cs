using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TVClient
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class EPGSearchPage : Page
    {
        private EPGSearchViewModel viewModel;
        public EPGSearchViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
            set
            {
                viewModel = value;
                DataContext = value;
            }
        }
        private bool searchHidden = false;
        private bool hasSearchResult = false;
        public EPGSearchPage()
        {
            this.InitializeComponent();
            this.ViewModel = new EPGSearchViewModel();
            this.ViewModel.ShowSearch = ShowSearchResult;
            this.SizeChanged += EPGSearchPage_SizeChanged;
        }

        private void EPGSearchPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(hasSearchResult)
            {
                if(e.NewSize.Width < 900)
                {
                    searchHidden = true;
                    SearchParameterColumn.Width = new GridLength(0, GridUnitType.Pixel);
                    SearchResultColumn.Width = new GridLength(1, GridUnitType.Star);
                    Result_Grid.Visibility = Visibility.Visible;
                    SearchScrollViewer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    searchHidden = false;
                    SearchParameterColumn.Width = new GridLength(1, GridUnitType.Star);
                    SearchResultColumn.Width = new GridLength(1, GridUnitType.Star);
                    Result_Grid.Visibility = Visibility.Visible;
                    SearchScrollViewer.Visibility = Visibility.Visible;
                }
            }
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get { return SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility; }
            set { SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= EPGSearchPage_BackRequested;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += EPGSearchPage_BackRequested;
            base.OnNavigatedTo(e);
        }

        private void EPGSearchPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if(searchHidden)
            {
                searchHidden = false;
                SearchParameterColumn.Width = new GridLength(1, GridUnitType.Star);
                SearchResultColumn.Width = new GridLength(0, GridUnitType.Pixel);
                Result_Grid.Visibility = Visibility.Collapsed;
                SearchScrollViewer.Visibility = Visibility.Visible;
            }
            else
            {
                if (base.Frame != null)
                {
                    if (base.Frame.CanGoBack)
                    {
                        e.Handled = true;
                        base.Frame.GoBack();
                    }
                }
            }       
        }

        private void EPGSearchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            lb.SelectedIndex = -1;
        }

        private void ShowSearchResult()
        {
            hasSearchResult = true;
            if (Result_Grid.Visibility == Visibility.Collapsed)
            {
                searchHidden = true;
                SearchParameterColumn.Width = new GridLength(0, GridUnitType.Pixel);
                SearchResultColumn.Width = new GridLength(1, GridUnitType.Star);
                Result_Grid.Visibility = Visibility.Visible;
                SearchScrollViewer.Visibility = Visibility.Collapsed;
            }
        }
    }
}
