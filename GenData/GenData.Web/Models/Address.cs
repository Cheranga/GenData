using System.Collections.Generic;

namespace GenData.Web.Models
{
    public class Address
    {
        public string State { get; set; }
        public string Suburb { get; set; }
        public string PostCode { get; set; }

        public ContactNumber[] ArrayContactNumbers { get; set; }
        public IEnumerable<ContactNumber> EnumerableContactNumbers { get; set; }
        public List<ContactNumber> ListContactNumbers { get; set; }
        public ICollection<ContactNumber> CollectionContactNumbers { get; set; }
        
    }
}