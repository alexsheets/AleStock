using System.ComponentModel.DataAnnotations;

namespace AleStock.Models.ViewModels
{
    public class APIKeysViewModel
    {
        [Required]
        public string Email { get; set; }
        public string Simfin_Key { get; set; }
        public string OpenAI_Key { get; set; }

    }
}