using DBA_Frontend.Models.ViewModels;
using System.Windows;

namespace DBA_Frontend
{
    public partial class StreetsInfoWindow : Window
    {
        public StreetsInfoWindow()
        {
            InitializeComponent();
            DataContext = new StreetsInfoViewModel();
        }
    }
}
