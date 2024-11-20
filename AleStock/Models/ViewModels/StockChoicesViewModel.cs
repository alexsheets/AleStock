using System.ComponentModel.DataAnnotations;

namespace AleStock.Models.ViewModels
{
    public class StockChoicesViewModel
    {
        [Required]
        public string Ticker { get; set; }
        [Required]
        public string Quarter { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public string APIKey { get; set; }
    }
}
