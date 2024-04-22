using DBA_Frontend.Models.ViewModels;
using System.Windows;

namespace DBA_Frontend
{
    public partial class SearchByNumberWindow : Window
    {
        public SearchByNumberWindow()
        {
            InitializeComponent();
            DataContext = new SearchByNumberViewModel();
        }
    }
}
