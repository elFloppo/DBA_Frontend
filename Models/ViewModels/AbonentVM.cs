using System.ComponentModel;

namespace DBA_Frontend.Models.ViewModels
{
    public class AbonentVM
    {
        [DisplayName("ID")]
        public int Id { get; set; }

        [DisplayName("ФИО")]
        public string FullName { get; set; }

        [DisplayName("Домашний тел.")]
        public string HomePhoneNumber { get; set; }

        [DisplayName("Рабочий тел.")]
        public string WorkPhoneNumber { get; set; }

        [DisplayName("Мобильный тел.")]
        public string MobilePhoneNumber { get; set; }

        [DisplayName("Улица")]
        public string Street { get; set; }

        [DisplayName("Номер дома")]
        public string BuildingNumber { get; set; }
    }
}
