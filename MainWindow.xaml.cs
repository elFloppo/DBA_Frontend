using DBA_Frontend.Extensions;
using DBA_Frontend.Models.DatabaseModels;
using DBA_Frontend.Models.Enums;
using DBA_Frontend.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DBA_Frontend
{    
    public partial class MainWindow : Window
    {
        private int _pageNumber = 1;
        private int _pageSize = 20;
        private AbonentFiltersModel _filters = new AbonentFiltersModel();
        private SortingFieldsEnum? _sortField = null;
        private bool _sortByDesc = false;
        
        private readonly ObservableCollection<AbonentVM> _abonentsList = new ObservableCollection<AbonentVM>();

        private static readonly string _apiAddress = ConfigurationManager.AppSettings["ApiAddress"];
        private readonly HttpClient _client = new HttpClient() { BaseAddress = new Uri(_apiAddress) };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            AbonentsListDg.ItemsSource = _abonentsList;
            await SetAbonentsFromAPI();
        }

        #region API
        private async Task<IEnumerable<AbonentVM>> GetAbonentsFromAPI(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                var response = await _client.GetAsync($"Abonents/Abonents?pageNumber={pageNumber}&pageSize={pageSize}&Id={_filters.Id}&FullName={_filters.FullName}&Street={_filters.Street}&BuildingNumber={_filters.BuildingNumber}&HomePhoneNumber={_filters.HomePhoneNumber}&WorkPhoneNumber={_filters.WorkPhoneNumber}&MobilePhoneNumber={_filters.MobilePhoneNumber}&sortField={_sortField}&sortByDesc={_sortByDesc}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Data unreachable");

                var abonentsList = await response.Content.ReadAsJsonAsync<List<Abonent>>();
                return abonentsList
                    .Select(an => new AbonentVM
                    {
                        Id = an.Id,
                        FullName = an.FullName,
                        Street = an.Address.Street.Name,
                        BuildingNumber = an.Address.BuildingNumber,
                        HomePhoneNumber = an.PhoneNumbers.FirstOrDefault(n => n.TypeId == PhoneNumberTypesEnum.Home)?.Number,
                        WorkPhoneNumber = an.PhoneNumbers.FirstOrDefault(n => n.TypeId == PhoneNumberTypesEnum.Work)?.Number,
                        MobilePhoneNumber = an.PhoneNumbers.FirstOrDefault(n => n.TypeId == PhoneNumberTypesEnum.Mobile)?.Number
                    });
            }
            catch
            {                
                return null;
            }
        }

        private async Task<int> GetAbonentsCountFromAPI()
        {
            try
            {
                var response = await _client.GetAsync($"Abonents/AbonentsCount?Id={_filters.Id}&FullName={_filters.FullName}&Street={_filters.Street}&BuildingNumber={_filters.BuildingNumber}&HomePhoneNumber={_filters.HomePhoneNumber}&WorkPhoneNumber={_filters.WorkPhoneNumber}&MobilePhoneNumber={_filters.MobilePhoneNumber}&sortField={_sortField}&sortByDesc={_sortByDesc}");

                if (!response.IsSuccessStatusCode)
                    return 0;

                return await response.Content.ReadAsJsonAsync<int>();
            }
            catch
            {
                return 0;
            }
        }

        private async Task SetAbonentsFromAPI()
        {
            var getAbonentsTask = GetAbonentsFromAPI(_pageNumber, _pageSize);
            var getAbonentsCountTask = GetAbonentsCountFromAPI();
            DataLoadingFailedTextBlock.Visibility = Visibility.Hidden;
            AbonentsLoadingPb.Visibility = Visibility.Visible;

            var abonents = await getAbonentsTask;
            _abonentsList.Clear();

            if (abonents != null)
                foreach (var anonent in abonents)
                    _abonentsList.Add(anonent);

            var totalAbonentsCount = await getAbonentsCountTask;
            PageNumberTextBox.Text = _pageNumber.ToString();
            NextPageBtn.IsEnabled = abonents != null && totalAbonentsCount > _pageNumber * _pageSize;
            PrevPageBtn.IsEnabled = abonents != null && _pageNumber > 1 && totalAbonentsCount > _pageSize;
            PageNumberTextBox.IsEnabled = abonents != null;
            PageSizeTextBox.IsEnabled = abonents != null;
            ApplyFiltersBtn.IsEnabled = abonents != null;
            ResetFiltersBtn.IsEnabled = abonents != null;
            GetCSVBtn.IsEnabled = abonents != null;
            UpdateBtn.IsEnabled = true;

            if (abonents == null) DataLoadingFailedTextBlock.Visibility = Visibility.Visible;
            AbonentsLoadingPb.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Data blocks
        private async void PrevPageBtn_Click(object sender, RoutedEventArgs e)
        {
            DisableDataBlocks();

            if (_pageNumber > 1) _pageNumber--;
            else _pageNumber = 1;

            await SetAbonentsFromAPI();
        }

        private async void NextPageBtn_Click(object sender, RoutedEventArgs e)
        {
            DisableDataBlocks();
            AbonentsLoadingPb.Visibility = Visibility.Visible;

            var abonentsCount = await GetAbonentsCountFromAPI();

            if (_pageNumber * _pageSize < abonentsCount) _pageNumber++;

            await SetAbonentsFromAPI();           
        }

        private void PageNumberAndSizeTextBoxes_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void PageNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DisableDataBlocks();
                AbonentsLoadingPb.Visibility = Visibility.Visible;

                _pageNumber = await GetPageNumberOrSizeFromTextBox(PageNumberTextBox.Text, 1, pageSize: _pageSize);
                await SetAbonentsFromAPI();
            }
        }

        private async void PageSizeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DisableDataBlocks();
                AbonentsLoadingPb.Visibility = Visibility.Visible;

                _pageNumber = 1;
                _pageSize = await GetPageNumberOrSizeFromTextBox(PageSizeTextBox.Text, 10, pageNumber: _pageNumber);
                await SetAbonentsFromAPI();
            }
        }

        private async Task<int> GetPageNumberOrSizeFromTextBox(string source, int defaultValue, int? pageNumber = null, int? pageSize = null)
        {
            int? parsedValue = null;
            if (int.TryParse(source, out var res))
                parsedValue = res;

            if (parsedValue != null && parsedValue > 0 && await GetAbonentsCountFromAPI() >= (pageNumber ?? parsedValue * pageSize ?? parsedValue))
                return parsedValue.Value;
            else
                return defaultValue;
        }

        private async void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            DisableDataBlocks();
            await SetAbonentsFromAPI();
        }

        private void DisableDataBlocks()
        {
            PrevPageBtn.IsEnabled = false;
            NextPageBtn.IsEnabled = false;

            PageNumberTextBox.IsEnabled = false;
            PageSizeTextBox.IsEnabled = false;       
            
            ApplyFiltersBtn.IsEnabled = false;
            ResetFiltersBtn.IsEnabled = false;

            GetCSVBtn.IsEnabled = false;
            UpdateBtn.IsEnabled = false;
        }
        #endregion       

        #region Filtering and sorting
        private async void TextBoxes_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) await ApplyFilters(); }
        private async void ApplyFiltersBtn_Click(object sender, RoutedEventArgs e) { await ApplyFilters(); }
        private async void ResetFiltersBtn_Click(object sender, RoutedEventArgs e) { await ApplyFilters(reset: true); }

        private async Task ApplyFilters(bool reset = false)
        {
            DisableDataBlocks();

            _filters.Id = reset ? null : IdFilterTextBox.Text;
            _filters.FullName = reset ? null : FullNameFilterTextBox.Text;
            _filters.Street = reset ? null : StreetFilterTextBox.Text;
            _filters.BuildingNumber = reset ? null : BuildingNumberFilterTextBox.Text;
            _filters.HomePhoneNumber = reset ? null : HomePhoneNumberFilterTextBox.Text;
            _filters.WorkPhoneNumber = reset ? null : WorkPhoneNumberFilterTextBox.Text;
            _filters.MobilePhoneNumber = reset ? null : MobilePhoneNumberFilterTextBox.Text;

            if (reset)
            {
                IdFilterTextBox.Text = null;
                FullNameFilterTextBox.Text = null;
                StreetFilterTextBox.Text = null;
                BuildingNumberFilterTextBox.Text = null;
                HomePhoneNumberFilterTextBox.Text = null;
                WorkPhoneNumberFilterTextBox.Text = null;
                MobilePhoneNumberFilterTextBox.Text = null;
            }

            _pageNumber = 1;
            PageNumberTextBox.Text = "1";

            await SetAbonentsFromAPI();
        }

        private async void AbonentsListDg_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            _sortByDesc = e.Column.SortDirection == ListSortDirection.Descending;

            e.Column.SortDirection = _sortByDesc ? ListSortDirection.Ascending : ListSortDirection.Descending;

            switch (e.Column.SortMemberPath)
            {
                case "Id":
                    _sortField = SortingFieldsEnum.Id;
                    break;
                case "FullName":
                    _sortField = SortingFieldsEnum.FullName;
                    break;
                case "Street":
                    _sortField = SortingFieldsEnum.Street;
                    break;
                case "BuildingNumber":
                    _sortField = SortingFieldsEnum.BuildingNumber;
                    break;
                case "HomePhoneNumber":
                    _sortField = SortingFieldsEnum.HomePhoneNumber;
                    break;
                case "WorkPhoneNumber":
                    _sortField = SortingFieldsEnum.WorkPhoneNumber;
                    break;
                case "MobilePhoneNumber":
                    _sortField = SortingFieldsEnum.MobilePhoneNumber;
                    break;
            }

            await SetAbonentsFromAPI();
        }
        #endregion

        #region Window buttons
        private void StreetsBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new StreetsInfoWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void SearchByNumberBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new SearchByNumberWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private async void GetCSVBtn_Click(object sender, RoutedEventArgs e)
        {
            GetCSVBtn.IsEnabled = false;
            var data = await GetAbonentsFromAPI();

            if (data == null)
                return;

            var window = new SaveCSVWindow();
            
            window.abonents = data;

            window.Owner = this;
            window.ShowDialog();
            GetCSVBtn.IsEnabled = true;
        }
        #endregion        

        private class AbonentFiltersModel
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Street { get; set; }
            public string BuildingNumber { get; set; }
            public string HomePhoneNumber { get; set; }
            public string WorkPhoneNumber { get; set; }
            public string MobilePhoneNumber { get; set; }
        }
    }
}
