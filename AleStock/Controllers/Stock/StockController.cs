using AleStock.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Python.Runtime;
using Kendo.Mvc.UI;
using AleStock.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Supabase.Postgrest;
using Ale.Models;
using Newtonsoft.Json.Linq;
using Telerik.SvgIcons;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Security.Policy;

namespace AleStock.Controllers.Stock
{
    public class StockController : Controller
    {

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        // instantiate db context 
        StockDbContext _dbContext;

        // create supabase client and init connection string
        public StockController(StockDbContext context) => _dbContext = context;


        /*
         * Functions simply for returning the associated views
         */

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        // helper functions to return all potential years and quarters for reviewing financials
        public JsonResult getYears()
        {
            try
            {
                List<SelectListItem> Years = new List<SelectListItem>
                {
                    new SelectListItem() { Text = "2017", Value = "2017" },
                    new SelectListItem() { Text = "2018", Value = "2018" },
                    new SelectListItem() { Text = "2019", Value = "2019" },
                    new SelectListItem() { Text = "2020", Value = "2020" },
                    new SelectListItem() { Text = "2021", Value = "2021" },
                    new SelectListItem() { Text = "2022", Value = "2022" },
                    new SelectListItem() { Text = "2023", Value = "2023" },
                    new SelectListItem() { Text = "2024", Value = "2024" },
                };

                return Json(Years);
            }
            catch (Exception ex)
            {
                return Json("");
            }
        }

        public JsonResult getQuarters()
        {
            try
            {
                List<SelectListItem> Years = new List<SelectListItem>
                {
                    new SelectListItem() { Text = "Q1", Value = "Q1" },
                    new SelectListItem() { Text = "Q2", Value = "Q2" },
                    new SelectListItem() { Text = "Q3", Value = "Q3" },
                    new SelectListItem() { Text = "Q4", Value = "Q4" },
                };

                return Json(Years);
            }
            catch (Exception ex)
            {
                return Json("");
            }
        }


        // function to retrieve the financial information of a single stock record
        [HttpGet]
        public async Task<ActionResult<StockEconomicalInfo>> SpecificFinancials()
        {
            // process in vars set earlier
            string tick = _httpContextAccessor.HttpContext.Session.GetString("Ticker");
            string quarter = _httpContextAccessor.HttpContext.Session.GetString("Quarter");
            string yr_str = _httpContextAccessor.HttpContext.Session.GetString("Year");
            int year = int.Parse(yr_str);

            // retrieve model associated
            StockEconomicalInfo stockRecord = await _dbContext.GetSpecificStockReport(tick, quarter, year);

            return View(stockRecord);
        }

        // helper func to recognize whether this stock economical record has been created in DB yet
        public async Task<StockEconomicalInfo> CheckExistence(string tick, string quarter, int year)
        {
            // retrieve model associated
            StockEconomicalInfo stockRecord = await _dbContext.GetSpecificStockReport(tick, quarter, year);

            if (stockRecord != null)
            {
                return stockRecord;
            }
            else
            {
                return new StockEconomicalInfo();
            }
        }

        // function which takes in the initial report choices, generates the report (if it hasnt already been processed to DB) and returns view
        [HttpPost]
        public async Task<ActionResult> SubmitStockChoices([DataSourceRequest] DataSourceRequest request, StockChoicesViewModel model)
        {

            // set vars in http context to access them later
            _httpContextAccessor.HttpContext.Session.SetString("APIKey", model.APIKey.ToString());
            _httpContextAccessor.HttpContext.Session.SetString("Ticker", model.Ticker.ToString());
            _httpContextAccessor.HttpContext.Session.SetString("Quarter", model.Quarter.ToString());
            _httpContextAccessor.HttpContext.Session.SetString("Year", model.Year.ToString());

            int year_int = Int32.Parse(model.Year.ToString());

            StockEconomicalInfo init_record = await CheckExistence(model.Ticker, model.Quarter, year_int);

            if (init_record == null)
            {
                // retrieves info and processes to db
                RunScript(@"Scripts\simfin.py", model.APIKey, model.Ticker, model.Quarter, year_int);
            }

            // send to page to view results
            return View("SpecificFinancials");

        }

        // function to run the pythonNet script and run simfin script with necessary user-submitted variables
        public async void RunScript(string script, string api_key, string ticker_submitted, string quarter_submitted, int year_submitted)
        {
            Runtime.PythonDLL = @"C:\Users\asheet3\.nuget\packages\pythonnet\3.0.4\lib\netstandard2.0\Python.Runtime.dll";
            PythonEngine.Initialize();

            // acquire GIL 
            using (Py.GIL())
            {
                var key = new PyString(api_key);
                var ticker = new PyString(ticker_submitted);
                var quarter = new PyString(quarter_submitted);
                var year = new PyInt(year_submitted);

                dynamic api_class = Py.Import(script).GetAttr("SimFinAPI");
                // API KEY GOES HERE
                dynamic simfin_instance = api_class(key);

                // var pyScript = Py.Import(script);
#pragma warning disable IDE0300 // Simplify collection initialization
                var result = simfin_instance.InvokeMethod("convert_to_json", new PyObject[] { ticker, year, quarter });
#pragma warning restore IDE0300 // Simplify collection initialization

                if (result != null)
                {
                    // process result here
                    
                    // ensure that it is in valid json by using newtonsoft JSON to create parsable json
                    var json_object = JObject.Parse(result);

                    // assign vars to object
                    string totalAssets = json_object["Assets"]["Total Assets"];
                    string totalLiabilities = json_object["Liabilities"]["Total Liabilities"];
                    string totalEquity = json_object["Equity"]["Total Equity"];
                    string totalDebt = json_object["Solvency Metrics"]["Total Debt"];

                    string grossProfitMargin = json_object["Profitability Metrics"]["Gross Profit Margin"];
                    string operatingMargin = json_object["Profitability Metrics"]["Operating Margin"];
                    string netProfitMargin = json_object["Profitability Metrics"]["Net Profit Margin"];
                    string returnOnEquity = json_object["Profitability Metrics"]["Return on Equity"];
                    string returnOnAssets = json_object["Profitability Metrics"]["Return on Assets"];
                    string returnOnInvested = json_object["Profitability Metrics"]["Return on Invested Capital"];

                    string liquidityRatio = json_object["Liquidity Metrics"]["Current Ratio"];
                    string liabilitiesToEquityRatio = json_object["Solvency Metrics"]["Liabilities to Equity Ratio"];
                    string debtRatio = json_object["Solvency Metrics"]["Debt Ratio"];
                    string dividendPayoutRatio = json_object["Other Important Metrics"]["Dividend Payout Ratio"];

                    string dividendsPaid = json_object["Financing Activities"]["Dividends Paid"];
                    string netCashOperating = json_object["Operating Activities"]["Net Cash from Operating Activities"];
                    string netCashInvesting = json_object["Investing Activities"]["Net Cash from Investing Activities"];
                    string netCashFinancing = json_object["Financing Activities"]["Net Cash from Financing Activities"];
                    string netCashDelta = json_object["Net Change"]["Net Change in Cash"];

                    await CreateStockReport(ticker_submitted, quarter_submitted, year_submitted, totalAssets, totalLiabilities, totalEquity, grossProfitMargin, operatingMargin, netProfitMargin, returnOnEquity,
                        returnOnAssets, returnOnInvested, liquidityRatio, liabilitiesToEquityRatio, debtRatio, totalDebt, dividendPayoutRatio, dividendsPaid, netCashOperating, netCashInvesting, netCashFinancing, netCashDelta);

                }
            }
        }

        public async Task<ActionResult> CreateStockReport(string ticker, string quarter, int year, string totalAssets, string totalLiabilities, string totalEquity, string grossProfitMargin, string operatingMargin,
                string netProfitMargin, string returnOnEquity, string returnOnAssets, string returnOnInvested, string liquidityRatio, string liabilitiesToEquityRatio, string debtRatio, string totalDebt, string dividendPayoutRatio,
                string dividendsPaid, string netCashOperating, string netCashInvesting, string netCashFinancing, string netCashDelta)
        {

            var model = new StockEconomicalInfo()
            {
                Ticker = ticker,
                Quarter = quarter,
                Year = year,
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,
                TotalEquity = totalEquity,
                GrossProfitMargin = grossProfitMargin,
                OperatingMargin = operatingMargin,
                NetProfitMargin = netProfitMargin,
                ReturnOnEquity = returnOnEquity,
                ReturnOnAssets = returnOnAssets,
                ReturnOnInvested = returnOnInvested,
                LiquidityRatio = liquidityRatio,
                LiabilitiesToEquityRatio = liabilitiesToEquityRatio,
                DebtRatio = debtRatio,
                TotalDebt = totalDebt,
                DividendPayoutRatio = dividendPayoutRatio,
                DividendsPaid = dividendsPaid,
                NetCashOperating = netCashOperating,
                NetCashInvesting = netCashInvesting,
                NetCashFinancing = netCashFinancing,
                NetCashDelta = netCashDelta
            };

            await _dbContext.SubmitStockReport(model);

            return View("SpecificFinancials");
        }

    }
}
