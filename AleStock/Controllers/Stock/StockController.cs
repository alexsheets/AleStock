using AleStock.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Python.Runtime;
using Kendo.Mvc.UI;
using AleStock.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using Supabase.Gotrue;


namespace AleStock.Controllers.Stock
{
    public class StockController : Controller
    {

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        StockDbContext _dbContext;
        private readonly Supabase.Client _supabaseClient;
        // private readonly Supabase.Postgrest.Client _pgClient;
        private readonly IConfiguration _configuration;

        // class member instantiation
        public StockController(IConfiguration configuration)
        {
            _configuration = configuration;
            _supabaseClient = new Supabase.Client(_configuration["SecretSection:url"], _configuration["SecretSection:key"]);
            _dbContext = new StockDbContext(configuration);
        }

        public JsonResult getYears()
        {
            try
            {
                List<SelectListItem> Years = new List<SelectListItem>
                {
                    new SelectListItem() { Text = "2018", Value = "2018" },
                    new SelectListItem() { Text = "2019", Value = "2019" },
                    new SelectListItem() { Text = "2020", Value = "2020" },
                    new SelectListItem() { Text = "2021", Value = "2021" },
                    new SelectListItem() { Text = "2022", Value = "2022" },
                    new SelectListItem() { Text = "2023", Value = "2023" },
                    new SelectListItem() { Text = "2024", Value = "2024" }
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

        // function used to create new session based off passed tokens
        public async Task<Session> SetSessionForControllerAsync()
        {
            string currToken = _httpContextAccessor.HttpContext.Session.GetString("currToken");
            string refrToken = _httpContextAccessor.HttpContext.Session.GetString("refrToken");

            Session session = await _dbContext.SetSessionAsync(currToken, refrToken);
            return session;
        }
        /*
         * Functions simply for returning the associated views
         */

        [HttpGet]
        public async Task<ActionResult> FinanceAnalyzationAsync()
        {
            Session session = await SetSessionForControllerAsync();
            if (session != null)
            {
                return View("FinanceAnalyzation");
            }
            else
            {
                TempData["ValidationMsg"] = "Error with authenticating the current session. Please re-login.";
                return RedirectToAction("Index", "Home");
            }

        }

        public async Task<IActionResult> SpecificFinancials() 
        {
            Session session = await SetSessionForControllerAsync();
            if (session != null)
            {
                return View("SpecificFinancials");
            }
            else
            {
                TempData["ValidationMsg"] = "Error with authenticating the current session. Please re-login.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Chat() 
        {
            Session session = await SetSessionForControllerAsync();
            if (session != null)
            {
                return View();
            }
            else
            {
                TempData["ValidationMsg"] = "Error with authenticating the current session. Please re-login.";
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SupplyKey() 
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public async Task<ActionResult> SubmitAPIKeys([DataSourceRequest] DataSourceRequest request, APIKeysViewModel vm)
        {

            try {
                if (vm != null) {
                    // dont need to check if there is existing simfin key because users
                    // have to submit this to proceed with analyzation/Ai analyzation
                    UserAPIKeys user_keys = new UserAPIKeys();
                    user_keys.Email = vm.Email;
                    user_keys.Simfin_Key = vm.Simfin_Key;

                    // create user keys record
                    HttpResponseMessage msg = await _dbContext.SubmitAPIKeys(user_keys);

                    // validate result msg

                    // send to page to submit stock analyzation choices
                    return View("FinanceAnalyzation");
                } else {
                    TempData["ValidationMsg"] = "There was an error submitting your SimFin key. Please try again.";
                    return RedirectToAction("SupplyKey", "Stock");
                }
            } catch (Exception ex) {
                TempData["ValidationMsg"] = "You have yet to submit your associated SimFin key.";
                return RedirectToAction("SupplyKey", "Stock");
            }
        }

        // function to retrieve the financial information of a single stock record
        [HttpGet]
        public async Task<ActionResult<StockEconomicalInfo>> SpecificFinancials(string tick, string quarter, string year)
        {
            int year_int = Int32.Parse(year);

            // retrieve model associated
            StockEconomicalInfo stockRecord = await _dbContext.GetSpecificStockReport(tick, quarter, year_int);

            return View("SpecificFinancials", stockRecord);
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
            // check if user has attributed simfin key
            var user = _supabaseClient.Auth.CurrentUser;

            // retrieve user api keys based on email
            UserAPIKeys keys = await _dbContext.GetUserAPIKeys(user.Email);

            // if the user has submitted their simfin key, proceed with rest
            if (keys.Simfin_Key != null) {

                int year_int = Int32.Parse(model.Year.ToString());

                // set information in http session for retrieval if user chooses to do an AI summarization
                _httpContextAccessor.HttpContext.Session.SetString("Quarter", model.Quarter.ToString());
                _httpContextAccessor.HttpContext.Session.SetString("Ticker", model.Ticker.ToString());
                _httpContextAccessor.HttpContext.Session.SetString("Year", model.Year.ToString());

                // check if stock exists by checking
                StockEconomicalInfo init_record = await CheckExistence(model.Ticker, model.Quarter, year_int);

                if (init_record == null)
                {
                    // retrieves info and processes to db
                    string result = await RunScript(@"Scripts\simfin.py", keys.Simfin_Key, model.Ticker, model.Quarter, year_int);
                    if (result == "SUCCESS!")
                    {
                        return RedirectToAction("SpecificFinancials", "Stock", new { tick = model.Ticker.ToString(), quarter = model.Quarter.ToString(), year = model.Year.ToString() });
                    }
                    else
                    {
                        TempData["ValidationMsg"] = "Error running SimFin Script. Please retry submitting values.";
                        return RedirectToAction("FinanceAnalyzation");
                    }
                }
                // send to page to view results
                return RedirectToAction("SpecificFinancials", "Stock", new { tick=model.Ticker.ToString(), quarter=model.Quarter.ToString(), year=model.Year.ToString() });
            } else {
                // if they have no simfin api key, send to key submission page
                TempData["ValidationMsg"] = "You have yet to submit your associated SimFin API key.";
                return RedirectToAction("SupplyKey", "Stock");
            }
        }

        // function to run the pythonNet script and run simfin script with necessary user-submitted variables
        public async Task<string> RunScript(string script, string api_key, string ticker_submitted, string quarter_submitted, int year_submitted)
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

                var result = simfin_instance.InvokeMethod("convert_to_json", new PyObject[] { ticker, year, quarter });

                //dynamic api = Py.Import(script);
                //dynamic simfin_instance = api.SimFinAPI(key);

                //var result = simfin_instance.convert_to_json(ticker, year, quarter);

                if (result != null)
                {
                    // process result here

                    StockEconomicalInfo stockRecord = new StockEconomicalInfo();
                    
                    // ensure that it is in valid json by using newtonsoft JSON to create parsable json
                    var json_object = JObject.Parse(result);

                    // assign vars to object
                    stockRecord.TotalAssets = json_object["Assets"]["Total Assets"];
                    stockRecord.TotalLiabilities = json_object["Liabilities"]["Total Liabilities"];
                    stockRecord.TotalEquity = json_object["Equity"]["Total Equity"];
                    stockRecord.TotalDebt = json_object["Solvency Metrics"]["Total Debt"];

                    stockRecord.GrossProfitMargin = json_object["Profitability Metrics"]["Gross Profit Margin"];
                    stockRecord.OperatingMargin = json_object["Profitability Metrics"]["Operating Margin"];
                    stockRecord.NetProfitMargin = json_object["Profitability Metrics"]["Net Profit Margin"];
                    stockRecord.ReturnOnEquity = json_object["Profitability Metrics"]["Return on Equity"];
                    stockRecord.ReturnOnAssets = json_object["Profitability Metrics"]["Return on Assets"];
                    stockRecord.ReturnOnInvested = json_object["Profitability Metrics"]["Return on Invested Capital"];

                    stockRecord.LiquidityRatio = json_object["Liquidity Metrics"]["Current Ratio"];
                    stockRecord.LiabilitiesToEquityRatio = json_object["Solvency Metrics"]["Liabilities to Equity Ratio"];
                    stockRecord.DebtRatio = json_object["Solvency Metrics"]["Debt Ratio"];
                    stockRecord.DividendPayoutRatio = json_object["Other Important Metrics"]["Dividend Payout Ratio"];

                    stockRecord.DividendsPaid = json_object["Financing Activities"]["Dividends Paid"];
                    stockRecord.NetCashOperating = json_object["Operating Activities"]["Net Cash from Operating Activities"];
                    stockRecord.NetCashInvesting = json_object["Investing Activities"]["Net Cash from Investing Activities"];
                    stockRecord.NetCashFinancing = json_object["Financing Activities"]["Net Cash from Financing Activities"];
                    stockRecord.NetCashDelta = json_object["Net Change"]["Net Change in Cash"];

                    await _dbContext.SubmitStockReport(stockRecord);

                    return "SUCCESS!";
                } else
                {
                    return "Failed to convert to JSON";
                }
            }
        }
    }
}
