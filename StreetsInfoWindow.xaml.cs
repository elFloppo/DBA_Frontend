using DBA_Frontend.Extensions;
using DBA_Frontend.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace DBA_Frontend
{
    public partial class StreetsInfoWindow : Window
    {
        private static readonly string _apiAddress = ConfigurationManager.AppSettings["ApiAddress"];
        private static readonly HttpClient _client = new HttpClient() { BaseAddress = new Uri(_apiAddress) };

        private readonly ObservableCollection<AbonentsOnStreetCountVM> _abonentsOnStreetsCountList = new ObservableCollection<AbonentsOnStreetCountVM>();

        public StreetsInfoWindow()
        {
            InitializeComponent();
        }

        private async void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            AbonentsOnStreetsListDg.ItemsSource = _abonentsOnStreetsCountList;
            await SetAbonentsOnStreetsCountFromAPI();
        }

        private async Task SetAbonentsOnStreetsCountFromAPI()
        {
            DataLoadingFailedTextBlock.Visibility = Visibility.Hidden;
            UpdateListBtn.IsEnabled = false;
            SearchingPb.Visibility = Visibility.Visible;
            
            try
            {
                var response = await _client.GetAsync($"Abonents/AbonentsOnStreetsCount");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Data unreachable");

                var abonentsOnStreetsCountList = await response.Content.ReadAsJsonAsync<List<AbonentsOnStreetCountVM>>();

                _abonentsOnStreetsCountList.Clear();
                foreach (var abonent in abonentsOnStreetsCountList)
                    _abonentsOnStreetsCountList.Add(abonent);
            }
            catch
            {
                DataLoadingFailedTextBlock.Visibility = Visibility.Visible;
            }

            UpdateListBtn.IsEnabled = true;
            SearchingPb.Visibility = Visibility.Hidden;
        }

        private async void UpdateListBtn_Click(object sender, RoutedEventArgs e)
        {          
            await SetAbonentsOnStreetsCountFromAPI();
        }
    }
}
