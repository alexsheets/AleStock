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
using System.Text.Json;
using Supabase;

namespace AleStock.Controllers.Stock
{
    public class AIController : Controller
    {

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        StockDbContext _dbContext = new StockDbContext();

        private readonly Supabase.Client _supabaseClient;

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

        public async Task<ActionResult> CheckForAPIKeys() {
            
            // retrieve current logged in user
            var user = _supabaseClient.Auth.CurrentUser;
            string email = user.Email;

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

        //public async Task<ActionResult> GetAISummarization(string api_key)
        //{

        //    // these would have been set when the user submits their choices for stock review
        //    // better way of transmitting the information?
        //    string q = _httpContextAccessor.HttpContext.Session.GetString("Quarter").ToString();
        //    string t = _httpContextAccessor.HttpContext.Session.GetString("Ticker").ToString();
        //    string y = _httpContextAccessor.HttpContext.Session.GetString("Year").ToString();

        //    int year_int = Int32.Parse(y);

        //    Runtime.PythonDLL = @"C:\Users\asheet3\.nuget\packages\pythonnet\3.0.4\lib\netstandard2.0\Python.Runtime.dll";
        //    PythonEngine.Initialize();

        //    if (q != null && t != null && y != null) 
        //    {
        //        using (Py.GIL())
        //        {
        //            dynamic api_class = Py.Import(@"Scripts\openAI_api.py").GetAttr("OpenAI");

        //            // retrieve stock information
        //            StockEconomicalInfo stockRecord = await _dbContext.GetSpecificStockReport(q, t, year_int);
        //            // convert to json
        //            var opt = new JsonSerializerOptions() { WriteIndented=true };
        //            string json_str = JsonSerializer.Serialize<StockEconomicalInfo>(stockRecord);

        //            // if the json str is not null/empty create openAI class with api key and stock json
        //            if (!string.IsNullOrEmpty(json_str)) {
        //                // instantiate class with api key and json
        //                dynamic openAI_instance = api_class(api_key, json_str);
        //            }
        //        }
        //    } else {
        //        // if any value empty, return to the stock choice screen with error msg
        //        TempData["ValidationMsg"] = "One of your values was missing for the AI summarization. Please re-submit your choices.";
        //        return View("FinanceAnalyzation");
        //    }

        //}
    }
}