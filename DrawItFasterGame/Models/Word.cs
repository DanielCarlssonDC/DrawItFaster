using System.ComponentModel.DataAnnotations;

namespace DrawItFaster.Models
{
    public class Word
    {
        [Key]
        public int WordID { get; set; }
        public string WordString { get; set; }
    }
}