using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AleStock.Models
{
    [Supabase.Postgrest.Attributes.Table("StockEconomicalReports")]
    public class StockEconomicalInfo : BaseModel
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("id")]
        public int Id { get; set; }

        // stock report metadata
        public string Ticker { get; set; }
        public string Quarter { get; set; }
        public int Year { get; set; }

        // company balances
        public string TotalAssets { get; set; }
        public string TotalLiabilities { get; set; }
        public string TotalEquity { get; set; }

        // metrics
        public string GrossProfitMargin { get; set; }
        public string OperatingMargin { get; set; }
        public string NetProfitMargin { get; set; }
        public float ReturnOnEquity { get; set; }
        public float ReturnOnAssets { get; set; }
        public float ReturnOnInvested { get; set; }
        public float LiquidityRatio { get; set; }
        public float LiabilitiesToEquityRatio { get; set; }
        public float DebtRatio { get; set; }
        public string TotalDebt {  get; set; }
        public float DividendPayoutRatio { get; set; }

        // company cash flow
        public float DividendsPaid {  get; set; }
        public string NetCashOperating { get; set; }
        public string NetCashInvesting { get; set; }
        public string NetCashFinancing { get; set; }
        public string NetCashDelta { get; set; }
    }
}
