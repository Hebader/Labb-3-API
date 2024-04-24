using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Labb_3___API.Models
{
    public class IntressePerson
    {
        [Key]
        public int IntressePersonId { get; set; }

        [ForeignKey("Intresse")]
        public int FkIntresseId { get; set; }
        public Intresse? Intresse { get; set; }

        [ForeignKey("Person")]
        public int FkPersonId { get; set; }
        //Navigering
        public Person? Person { get; set; }

    }
}
