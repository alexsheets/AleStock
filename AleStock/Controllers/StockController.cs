using AleStock.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Python.Runtime;
using Kendo.Mvc.UI;
using AleStock.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Supabase.Postgrest;

namespace AleStock.Controllers
{
    public class StockController : Controller
    {

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        /*
         * Functions simply for returning the associated views
         */
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SpecificFinancials()
        {
            // process in vars set earlier
            String tick = _httpContextAccessor.HttpContext.Session.GetString("Ticker");
            String quarter = _httpContextAccessor.HttpContext.Session.GetString("Quarter");
            String year = _httpContextAccessor.HttpContext.Session.GetString("Year");

            // retrieve model associated

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

        // function which takes in the initial report choices, generates the report and returns view
        [HttpPost]
        public ActionResult SubmitInitReportChoices([DataSourceRequest] DataSourceRequest request, StockChoicesViewModel model)
        {

            // set vars in http context to access them later
            _httpContextAccessor.HttpContext.Session.SetString("Ticker", model.Ticker.ToString());
            _httpContextAccessor.HttpContext.Session.SetString("Quarter", model.Quarter.ToString());
            _httpContextAccessor.HttpContext.Session.SetString("Year", model.Year.ToString());

            RunScript(@"Scripts\simfin.py", model.Ticker, model.Quarter, model.Year);

            // receives results and processes them to db
            // send to page to view results

            return View("SpecificFinancials");

        }

        // function to run the pythonNet script and run simfin script with necessary user-submitted variables
        static void RunScript(string script, string ticker_submitted, string quarter_submitted, int year_submitted)
        {
            Runtime.PythonDLL = @"C:\Users\asheet3\.nuget\packages\pythonnet\3.0.4\lib\netstandard2.0\Python.Runtime.dll";
            PythonEngine.Initialize();

            // acquire GIL 
            using (Py.GIL())
            {
                var ticker = new PyString(ticker_submitted);
                var quarter = new PyString(quarter_submitted);
                var year = new PyInt(year_submitted);

                var pyScript = Py.Import(script);
                var result = pyScript.InvokeMethod("convert_to_json", new PyObject[] { ticker, quarter, year });

                if (result != null)
                {
                    // process result here
                }
                
            }
        }

    }
}
