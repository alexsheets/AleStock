namespace AleStock.Models.ViewModels
{
    public class AIFinanceSummaryViewModel
    {
        public string GrossProfitMargin { get; set; }
        public string OperatingMargin { get; set; }
        public string NetProfitMargin { get; set; }
        public string ReturnOnEquity { get; set; }
        public string ReturnOnAssets { get; set; }
        public string ReturnOnInvested { get; set; }
        public string LiquidityRatio { get; set; }
        public string LiabilitiesToEquityRatio { get; set; }
        public string DebtRatio { get; set; }
        public string DividendPayoutRatio { get; set; }

        public string GrossProfitMarginResponse { get; set; }
        public string OperatingMarginResponse { get; set; }
        public string NetProfitMarginResponse { get; set; }
        public string ReturnOnEquityResponse { get; set; }
        public string ReturnOnAssetsResponse { get; set; }
        public string ReturnOnInvestedResponse { get; set; }
        public string LiquidityRatioResponse { get; set; }
        public string LiabilitiesToEquityRatioResponse { get; set; }
        public string DebtRatioResponse { get; set; }
        public string DividendPayoutRatioResponse { get; set; }
    }
}
