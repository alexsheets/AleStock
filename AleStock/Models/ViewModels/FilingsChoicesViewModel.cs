using System.ComponentModel.DataAnnotations;

namespace AleStock.Models.ViewModels
{
    public class FilingsChoicesViewModel
    {
        [Required]
        public string Ticker { get; set; }
        [Required]
        public string Year { get; set; }
    }
}
