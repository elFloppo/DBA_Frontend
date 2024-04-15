namespace DBA_Frontend.Models.DatabaseModels
{
    public class PhoneNumber
    {
        public int Id { get; set; }
        public int AbonentId { get; set; }
        public PhoneNumberTypesEnum TypeId { get; set; }
        public string Number { get; set; }

        public Abonent Abonent { get; set; }
        public PhoneNumberType Type { get; set; } 
    }
}
