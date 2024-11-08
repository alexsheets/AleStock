using AleStock.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Python.Runtime;
using Kendo.Mvc.UI;
using AleStock.Models.ViewModels;

namespace AleStock.Controllers
{
    public class StockController : Controller
    {

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

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

        [HttpPost]
        public ActionResult SubmitInitReportChoices([DataSourceRequest] DataSourceRequest request, StockChoicesViewModel model)
        {

            RunScript("simfin.py", model.Ticker, model.Quarter, model.Year);
            // receive results (or maybe set them in DB?)
            // send to page to view results

        }

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
                var result = pyScript.InvokeMethod("", new PyObject[] { ticker, quarter, year });
            }
        }

    }
}
