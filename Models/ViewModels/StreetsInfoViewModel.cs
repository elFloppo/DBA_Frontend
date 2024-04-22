using System.Collections.ObjectModel;
using System.Configuration;
using System.Net.Http;
using System;
using DBA_Frontend.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DBA_Frontend.Commands.StreetsInfoCommands;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBA_Frontend.Models.ViewModels
{
    public class StreetsInfoViewModel : INotifyPropertyChanged
    {
        private static readonly string _apiAddress = ConfigurationManager.AppSettings["ApiAddress"];
        private static readonly HttpClient _client = new HttpClient() { BaseAddress = new Uri(_apiAddress) };

        public Task Initialization { get; private set; }
        private async Task InitializeAsync() { await SetAbonentsOnStreetsCountFromAPI(); }

        public ObservableCollection<AbonentsOnStreetCountVM> AbonentsOnStreetsCountList { get; }
        public StreetsInfoViewModel()
        {
            AbonentsOnStreetsCountList = new ObservableCollection<AbonentsOnStreetCountVM>();
            Initialization = InitializeAsync();
        }

        private bool _updateListButtonEnabled = false;

        private Visibility _errorMessageVisibility = Visibility.Hidden;
        public Visibility ErrorMessageVisibility
        {
            get { return _errorMessageVisibility; }
            set
            {
                _errorMessageVisibility = value;
                OnPropertyChanged("ErrorMessageVisibility");
            }
        }

        private Visibility _progressBarVisibility = Visibility.Visible;
        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set
            {
                _progressBarVisibility = value;
                OnPropertyChanged("ProgressBarVisibility");
            }
        }       

        private UpdateListCommand _updateListCommand;
        public UpdateListCommand UpdateListCommand
        {
            get
            {
                return _updateListCommand ??
                    (_updateListCommand = new UpdateListCommand(
                        async p =>
                        {
                            await SetAbonentsOnStreetsCountFromAPI();
                        },
                        p => _updateListButtonEnabled)
                    );
            }
        }

        private async Task SetAbonentsOnStreetsCountFromAPI()
        {
            _updateListButtonEnabled = false;
            ErrorMessageVisibility = Visibility.Hidden;           
            ProgressBarVisibility = Visibility.Visible;

            try
            {
                var response = await _client.GetAsync($"Abonents/AbonentsOnStreetsCount");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Data unreachable");

                var abonentsOnStreetsCountList = await response.Content.ReadAsJsonAsync<List<AbonentsOnStreetCountVM>>();

                AbonentsOnStreetsCountList.Clear();
                foreach (var abonent in abonentsOnStreetsCountList)
                    AbonentsOnStreetsCountList.Add(abonent);
            }
            catch
            {
                ErrorMessageVisibility = Visibility.Visible;
            }
            
            ProgressBarVisibility = Visibility.Hidden;
            _updateListButtonEnabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class AbonentsOnStreetCountVM
    {
        public string StreetName { get; set; }
        public int AbonentsCount { get; set; }
    }
}
