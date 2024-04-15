using System;
using System.Collections.Generic;

namespace DBA_Frontend.Models.DatabaseModels
{
    public class Abonent
    {
        public int Id { get; set; }
        public int AddressId { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string FullName
        {
            get
            {
                var fullName = new List<string>();

                Action<string> addToFullName = x => { if (x != null) fullName.Add(x); };

                addToFullName(Surname);
                addToFullName(Name);
                addToFullName(Patronymic);

                return string.Join(" ", fullName);
            }
        }

        public Address Address { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
    }
}
