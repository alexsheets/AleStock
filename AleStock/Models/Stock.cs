using System.ComponentModel.DataAnnotations;

namespace AleStock.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }
        public string Ticker { get; set; }
        public string Quarter { get; set; }
        public int Year { get; set; }

    }
}
