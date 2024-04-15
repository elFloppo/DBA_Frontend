namespace DBA_Frontend.Models.DatabaseModels
{
    public class Address
    {
        public int Id { get; set; }
        public int StreetId { get; set; }
        public string BuildingNumber { get; set; }
       
        public Street Street { get; set; }
    }
}
