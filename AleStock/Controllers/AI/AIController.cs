using AleStock.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Python.Runtime;
using Kendo.Mvc.UI;
using AleStock.Models.ViewModels;
using System.Text.Json;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;

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

        public string returnPrompt()
        {
            return """
                You will act as a financial analyst. 
                You will be given some financial information for one fiscal quarter relating to a particular company in a JSON string.
                The financial concepts are passed in the JSON as keys. 
                The values of the JSON are the associated amounts of money the company reported for the quarter.
                Briefly explain each related financial concept and what the amount of money associated means for the company. 
                Analyze the information and give some advice as to whether the company finds itself in good standing.
                You should try to relay the financial information in such a way that it is easily understandable,
                as if it were being written for someone who is a beginner in understanding the stock market.
            """;
        }

        [HttpPost]
        public async Task<ActionResult> GetAISummarization(string api_key)
        {
            // method to complete AI Summarization by using OpenAI Nuget Package
            string q = _httpContextAccessor.HttpContext.Session.GetString("Quarter").ToString();
            string t = _httpContextAccessor.HttpContext.Session.GetString("Ticker").ToString();
            string y = _httpContextAccessor.HttpContext.Session.GetString("Year").ToString();
            int year_int = Int32.Parse(y);

            if (q != null && t != null && y != null)
            {
                StringBuilder contentBuilder = new();

                // instantiate openAI chat model using api
                ChatClient client = new(
                    model: "gpt-4o",
                    apiKey: api_key
                );

                // retrieve stock information
                StockEconomicalInfo stockRecord = await _dbContext.GetSpecificStockReport(q, t, year_int);

                // call func to create specific viewmodel of stock information to pass to AI analyzation
                StockRecordInfoForAIViewModel infoForAi = ConvertStockRecord(stockRecord);

                List<ChatMessage> messages = [
                    returnPrompt(),
                    Newtonsoft.Json.JsonConvert.SerializeObject(infoForAi)
                ];

                // process chunks of results as they come in
                AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = client.CompleteChatStreamingAsync(messages);

                await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    if (completionUpdate.ContentUpdate.Count > 0)
                    {
                        Console.Write(completionUpdate.ContentUpdate[0].Text);
                    }
                }

                return View("");
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