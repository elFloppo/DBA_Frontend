using DBA_Frontend.Extensions;
using DBA_Frontend.Models.DatabaseModels;
using DBA_Frontend.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace DBA_Frontend
{
    public partial class SearchByNumberWindow : Window
    {
        private static readonly string _apiAddress = ConfigurationManager.AppSettings["ApiAddress"];
        private static readonly HttpClient _client = new HttpClient() { BaseAddress = new Uri(_apiAddress) };

        private readonly ObservableCollection<FoundByNumberAbonentVM> _abonentsList = new ObservableCollection<FoundByNumberAbonentVM>();

        public SearchByNumberWindow()
        {
            InitializeComponent();
        }

        private void FoundAbonentsListDg_Loaded(object sender, RoutedEventArgs e) { FoundAbonentsListDg.ItemsSource = _abonentsList; }

        private async void FindByNumberBtn_Click(object sender, RoutedEventArgs e) { await FindAbonentsByPhoneNumberFromAPI(FindByNumberTextBox.Text); }

        private async Task FindAbonentsByPhoneNumberFromAPI(string phoneNumber)
        {
            DataLoadingFailedTextBlock.Visibility = Visibility.Hidden;
            FindByNumberBtn.IsEnabled = false;
            SearchPb.Visibility = Visibility.Visible;
            try
            {
                var response = await _client.GetAsync($"Abonents/FindAbonentsByPhoneNumber?phoneNumber={phoneNumber}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Data unreachable");

                var abonentsList = await response.Content.ReadAsJsonAsync<List<Abonent>>();
                
                var foundAbonents = abonentsList
                    .SelectMany(a => a.PhoneNumbers, (a, n) => new { Abonent = a, Number = n })
                    .Select(an => new FoundByNumberAbonentVM
                    {
                        Id = an.Abonent.Id,
                        FullName = an.Abonent.FullName,
                        Street = an.Abonent.Address.Street.Name,
                        BuildingNumber = an.Abonent.Address.BuildingNumber,
                        PhoneNumber = an.Number.Number,
                        PhoneNumberType = an.Number.Type.TypeName
                    });

                _abonentsList.Clear();
                foreach (var abonent in foundAbonents)
                    _abonentsList.Add(abonent);
            }
            catch
            {
                DataLoadingFailedTextBlock.Visibility = Visibility.Visible;
            }

            FindByNumberBtn.IsEnabled = true;
            SearchPb.Visibility = Visibility.Hidden;          
        }
    }
}
