using Supabase.Postgrest.Models;

namespace AleStock.Models.TestModels
{
    [Supabase.Postgrest.Attributes.Table("TestStockEconomicalReports")]

    public class TestStockEconomicalReport : BaseModel
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
        public string TotalDebt { get; set; }

        // profit metrics
        public string GrossProfitMargin { get; set; }
        public string OperatingMargin { get; set; }
        public string NetProfitMargin { get; set; }
        public string ReturnOnEquity { get; set; }
        public string ReturnOnAssets { get; set; }
        public string ReturnOnInvested { get; set; }

        // other metrics
        public string LiquidityRatio { get; set; }
        public string LiabilitiesToEquityRatio { get; set; }
        public string DebtRatio { get; set; }
        public string DividendPayoutRatio { get; set; }

        // company cash flow
        public string DividendsPaid { get; set; }
        public string NetCashOperating { get; set; }
        public string NetCashInvesting { get; set; }
        public string NetCashFinancing { get; set; }
        public string NetCashDelta { get; set; }
        
    }
}
