namespace DBA_Frontend.Models.DatabaseModels
{
    public class PhoneNumberType
    {
        public PhoneNumberTypesEnum Id { get; set; }
        public string TypeName { get; set; }
    }

    public enum PhoneNumberTypesEnum
    {
        Home,
        Work,
        Mobile
    }
}
