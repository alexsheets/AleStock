using System.ComponentModel.DataAnnotations;

namespace AleStock.Models
{
    public class StockEconomicalInfo
    {
        [Key]
        public int Id { get; set; }
        // relation back to stock table
        public int StockId { get; set; }

        // company balances
        public int TotalAssets { get; set; }
        public int TotalLiabilities { get; set; }
        public int TotalEquity { get; set; }

        // metrics
        public int GrossProfitMargin { get; set; }
        public int OperatingMargin { get; set; }
        public int NetProfitMargin { get; set; }
        public int ReturnOnEquity { get; set; }
        public int ReturnOnAssets { get; set; }
        public int ReturnOnInvested { get; set; }
        public float LiquidityRatio { get; set; }
        public float LiabilitiesToEquityRatio { get; set; }
        public float DebtRatio { get; set; }
        public float TotalDebt {  get; set; }
        public float DividendPayoutRatio { get; set; }

        // company cash flow
        public float DividendsPaid {  get; set; }
        public int NetCashOperating { get; set; }
        public int NetCashInvesting { get; set; }
        public int NetCashFinancing { get; set; }
        public int NetCashDelta { get; set; }
    }
}
