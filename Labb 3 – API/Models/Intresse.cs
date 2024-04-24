using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Labb_3___API.Models
{
    public class Intresse
    {
        [Key]
        public int IntresseId { get; set; }
        public string Titel { get; set; }
        public string Beskrivning { get; set; }
        
        public ICollection<IntressePerson>? Intresset { get; set; }

    }
}
