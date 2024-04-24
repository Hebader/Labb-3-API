using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Labb_3___API.Models
{
    public class Länk
    {
        [Key]
        public int LänkId { get; set; }
        public string URL { get; set; }

        [ForeignKey("Intresse")]
        public int FkIntresseId {get; set; }
        public Intresse? Intresse { get; set; }  

    }
}
