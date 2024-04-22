using DBA_Frontend.Models.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace DBA_Frontend
{
    public partial class SaveCSVWindow : Window
    {
        public SaveCSVWindow(IEnumerable<AbonentVM> abonents)
        {
            InitializeComponent();
            DataContext = new SaveCSVViewModel(abonents, this);
        }       
    }
}
