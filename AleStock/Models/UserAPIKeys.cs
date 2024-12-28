using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AleStock.Models
{
    [Supabase.Postgrest.Attributes.Table("APIKeyUserLookup")]
    public class UserAPIKeys : BaseModel
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("id")]
        public int Id { get; set; }

        public string Email { get; set; }
        public string Simfin_Key { get; set; }
        public string OpenAI_Key { get; set; }
        
    }
}
