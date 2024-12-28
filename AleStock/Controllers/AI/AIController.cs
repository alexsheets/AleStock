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

        StockDbContext _dbContext = new StockDbContext();

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

        public async Task<ActionResult> GetAISummarization(string api_key)
        {
            // these would have been set when the user submits their choices for stock review
            string q = _httpContextAccessor.HttpContext.Session.GetString("Quarter").ToString();
            string t = _httpContextAccessor.HttpContext.Session.GetString("Ticker").ToString();
            string y = _httpContextAccessor.HttpContext.Session.GetString("Year").ToString();

            Runtime.PythonDLL = @"C:\Users\asheet3\.nuget\packages\pythonnet\3.0.4\lib\netstandard2.0\Python.Runtime.dll";
            PythonEngine.Initialize();

            if (q != null && t != null && y != null) 
            {
                using (Py.GIL())
                {
                    dynamic api_class = Py.Import(@"Scripts\openAI_api.py").GetAttr("OpenAI");
                    // API KEY GOES HERE
                    dynamic openAI_instance = api_class(api_key);
                }
            } else {
                // if any value empty, return to the stock choice screen with error msg
                TempData["ValidationMsg"] = "One of your values was missing for the AI summarization. Please re-submit your choices.";
                return View("FinanceAnalyzation");
            }

        }
    }
}