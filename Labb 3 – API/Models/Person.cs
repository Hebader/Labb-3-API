using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Labb_3___API.Models
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }
        public string PersonNamn { get; set; }
        public string Telefonnummer { get; set; }
        public List<IntressePerson>? Intresset { get; set; }
      
    }
}
