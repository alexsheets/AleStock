using AleStock.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Python.Runtime;
using Kendo.Mvc.UI;
using AleStock.Models.ViewModels;
using System.Text.Json;

namespace AleStock.Controllers.Stock
{
    public class AIController : Controller
    {

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        StockDbContext _dbContext;
        private readonly Supabase.Client _supabaseClient;
        // private readonly Supabase.Postgrest.Client _pgClient;
        private readonly IConfiguration _configuration;

        // class member instantiation
        public AIController(IConfiguration configuration)
        {
            _configuration = configuration;
            _supabaseClient = new Supabase.Client(_configuration["SecretSection:url"], _configuration["SecretSection:key"]);
            _dbContext = new StockDbContext(configuration);
        }

        /*
         * Functions simply for returning the associated views
         */

        public IActionResult AIFinanceSummarization()
        {
            return View();
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

        // simple function to convert a stock record to the necessary information for AI analyzation
        public StockRecordInfoForAIViewModel ConvertStockRecord(StockEconomicalInfo record)
        {
            StockRecordInfoForAIViewModel infoForAi = new StockRecordInfoForAIViewModel() 
            {
                GrossProfitMargin = record.GrossProfitMargin,
                OperatingMargin = record.OperatingMargin,
                NetProfitMargin = record.NetProfitMargin,
                ReturnOnEquity = record.ReturnOnEquity,
                ReturnOnAssets = record.ReturnOnAssets,
                ReturnOnInvested = record.ReturnOnInvested,
                LiquidityRatio = record.LiquidityRatio,
                LiabilitiesToEquityRatio = record.LiabilitiesToEquityRatio,
                DebtRatio = record.DebtRatio,
                DividendPayoutRatio = record.DividendPayoutRatio
            };
            return infoForAi;
        }

        public async Task<ActionResult> CheckForAPIKeys() {
            
            // retrieve current logged in user
            var user = _supabaseClient.Auth.CurrentUser;

            // retrieve user api keys based on email
            UserAPIKeys keys = await _dbContext.GetUserAPIKeys(user.Email);

            if (keys.OpenAI_Key != null) {
                return RedirectToAction("GetAISummarization", "AI", new { api_key = keys.OpenAI_Key.ToString() });
            } else {
                // if they have no api keys record, send to key submission page
                TempData["ValidationMsg"] = "You have yet to submit your associated OpenAI key.";
                return RedirectToAction("SupplyKey", "AI");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SubmitAPIKeys([DataSourceRequest] DataSourceRequest request, APIKeysViewModel vm)
        {

            try {
                if (vm != null) {
                    // may have to retrieve an already existing set of API keys here with a simfin key
                    UserAPIKeys keys = await _dbContext.GetUserAPIKeys(vm.Email);

                    if (keys == null) { 
                        UserAPIKeys user_keys = new UserAPIKeys();
                        user_keys.Email = vm.Email;
                        user_keys.OpenAI_Key = vm.OpenAI_Key;

                        // create record if necessary
                        HttpResponseMessage msg = await _dbContext.SubmitAPIKeys(user_keys);

                    } else {
                        // already existing record, just update
                        keys.OpenAI_Key = vm.OpenAI_Key;
                        HttpResponseMessage msg = await _dbContext.UpdateOpenAIKey(keys);
                    }

                    // add in checks before sending
                    // send to page to view results
                    return RedirectToAction("GetAISummarization", "AI", new { api_key = vm.OpenAI_Key.ToString() });

                } else
                {
                    TempData["ValidationMsg"] = "There was an error submitting your OpenAI key. Please try again.";
                    return RedirectToAction("SupplyKey", "AI");
                }
            } catch (Exception ex) {
                TempData["ValidationMsg"] = "You have yet to submit your associated OpenAI key.";
                return RedirectToAction("SupplyKey", "AI");
            }
        }

        public async Task<ActionResult> GetAISummarization(string api_key)
        {

            // would have been set upon submitting parameters for stock financial analyzation
            // might need to change this
            string q = _httpContextAccessor.HttpContext.Session.GetString("Quarter").ToString();
            string t = _httpContextAccessor.HttpContext.Session.GetString("Ticker").ToString();
            string y = _httpContextAccessor.HttpContext.Session.GetString("Year").ToString();
            int year_int = Int32.Parse(y);

            Runtime.PythonDLL = @"C:\Users\asheet3\.nuget\packages\pythonnet\3.0.4\lib\netstandard2.0\Python.Runtime.dll";
            PythonEngine.Initialize();

            if (q != null && t != null && y != null)
            {
                using (Py.GIL())
                {
                    // instantiate the api as a callable class
                    dynamic api_class = Py.Import(@"Scripts\openAI_api.py").GetAttr("OpenAI");

                    // retrieve stock information
                    StockEconomicalInfo stockRecord = await _dbContext.GetSpecificStockReport(q, t, year_int);

                    // call func to create specific viewmodel of stock information to pass to AI analyzation
                    StockRecordInfoForAIViewModel infoForAi = ConvertStockRecord(stockRecord);

                    // convert to json
                    var opt = new JsonSerializerOptions() { WriteIndented = true };
                    string json_str = JsonSerializer.Serialize(infoForAi, opt);

                    // if the json str is not null/empty create openAI class with api key and stock json
                    if (!string.IsNullOrEmpty(json_str))
                    {
                        // create class instantiation with api key and json of financial information
                        dynamic openAI_instance = api_class(api_key, json_str);
                    }
                }
            }
            else
            {
                // if any value empty, return to the stock choice screen with error msg
                TempData["ValidationMsg"] = "One of your values was missing for the AI summarization. Please re-submit your choices.";
                return View("FinanceAnalyzation");
            }

        }
    }
}