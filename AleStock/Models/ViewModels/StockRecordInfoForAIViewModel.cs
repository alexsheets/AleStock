namespace AleStock.Models.ViewModels
{
    // viewmodel for all the information we want to pass for AI analyzation
    // we exclude the stock information (ticker, quarter, year) as well as the self-explainable total company balances,
    // dividends paid and net cash
    public class StockRecordInfoForAIViewModel
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

    }
}
