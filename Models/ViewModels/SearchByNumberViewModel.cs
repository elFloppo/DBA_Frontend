using System.Collections.ObjectModel;
using System.Configuration;
using System.Net.Http;
using System;
using DBA_Frontend.Extensions;
using DBA_Frontend.Models.DatabaseModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DBA_Frontend.Commands.SearchByNumberCommands;
using System.Windows;

namespace DBA_Frontend.Models.ViewModels
{
    public class SearchByNumberViewModel : INotifyPropertyChanged
    {
        private static readonly string _apiAddress = ConfigurationManager.AppSettings["ApiAddress"];
        private static readonly HttpClient _client = new HttpClient() { BaseAddress = new Uri(_apiAddress) };

        public ObservableCollection<FoundByNumberAbonentVM> AbonentsList { get; }
        public SearchByNumberViewModel()
        {
            AbonentsList = new ObservableCollection<FoundByNumberAbonentVM>();
        }

        private string _phoneNumber = null;
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                _phoneNumber = value;
                OnPropertyChanged("PhoneNumber");
            }
        }

        private bool _searchInProgress = false;

        private Visibility _dataLoadingFailedTextVisibility = Visibility.Hidden;
        public Visibility DataLoadingFailedTextVisibility
        {
            get { return _dataLoadingFailedTextVisibility; }
            set
            {
                _dataLoadingFailedTextVisibility = value;
                OnPropertyChanged("DataLoadingFailedTextVisibility");
            }
        }

        private Visibility _searchProgressBarVisibility = Visibility.Hidden;
        public Visibility SearchProgressBarVisibility
        {
            get { return _searchProgressBarVisibility; }
            set
            {
                _searchProgressBarVisibility = value;
                OnPropertyChanged("SearchProgressBarVisibility");
            }
        }

        private FindByNumberCommand _findByNumberCommand;
        public FindByNumberCommand FindByNumberCommand
        {
            get
            {
                return _findByNumberCommand ??
                    (_findByNumberCommand = new FindByNumberCommand(
                        async p =>
                        {
                            await FindAbonentsByPhoneNumberFromAPI(PhoneNumber);
                        },
                        p => !string.IsNullOrWhiteSpace(PhoneNumber?.Trim()) && !_searchInProgress));
            }
        }

        private async Task FindAbonentsByPhoneNumberFromAPI(string phoneNumber)
        {
            _searchInProgress = true;
            DataLoadingFailedTextVisibility = Visibility.Hidden;            
            SearchProgressBarVisibility = Visibility.Visible;

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

                AbonentsList.Clear();
                foreach (var abonent in foundAbonents)
                    AbonentsList.Add(abonent);
            }
            catch
            {
                DataLoadingFailedTextVisibility = Visibility.Visible;
            }

            SearchProgressBarVisibility = Visibility.Hidden;
            _searchInProgress = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class FoundByNumberAbonentVM
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberType { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
    }
}
