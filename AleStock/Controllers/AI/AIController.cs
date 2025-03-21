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
using Supabase.Gotrue;
using Microsoft.AspNet.SignalR;
using RestSharp;

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

        // function to set session using refresh tokens
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

        public async Task<IActionResult> AIFinanceSummarization()
        {
            Session session = await SetSessionForControllerAsync();
            if (session != null)
            {
                return View("AIFinanceSummarization");
            }
            else
            {
                TempData["ValidationMsg"] = "Error with authenticating the current session. Please re-login.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> AIFilingSummarization()
        {
            Session session = await SetSessionForControllerAsync();
            if (session != null)
            {
                return View("AIFilingSummarization");
            }
            else
            {
                TempData["ValidationMsg"] = "Error with authenticating the current session. Please re-login.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> AIFilingSummarizationChoices()
        {
            Session session = await SetSessionForControllerAsync();
            if (session != null)
            {
                return View("AIFilingSummarizationChoices");
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

        // helper func to return string prompt
        public string returnPrompt()
        {
            return """
                You will act as a financial analyst. 
                You will be given some financial information for one fiscal quarter relating to a particular company in the format of a JSON string.
                The financial concepts are passed in the JSON as keys. 
                The amounts of money the company reported for the quarter for said financial concept are passed in the JSON as values.
                For each financial concept, return a new message briefly explainining said concept and what the amount of money associated means for the company. 
                Analyze the information and advise as to whether the company finds itself in good financial standing.
                You should try to relay the information in such a way that it is easily understandable,
                as if it were being written for someone who is a beginner in understanding the stock market.
            """;
        }

        public string returnPromptFiling()
        {
            return """
                You will act as a financial analyst. 
                You will be given recent filing, which you should analyze and respond with the most important parts in a simple summarization.
                You should try to relay the information in such a way that it is easily understandable,
                as if it were being written for someone who is a beginner in understanding the stock market.
            """;
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

        public AIFinanceSummaryViewModel CreateAISummaryViewModel(StockEconomicalInfo info, StockRecordInfoForAIViewModel record, List<string> ai_responses)
        {
            AIFinanceSummaryViewModel vm = new AIFinanceSummaryViewModel()
            {
                Ticker = info.Ticker,
                Quarter = info.Quarter,
                Year = info.Year,
                GrossProfitMargin = record.GrossProfitMargin,
                OperatingMargin = record.OperatingMargin,
                NetProfitMargin = record.NetProfitMargin,
                ReturnOnEquity = record.ReturnOnEquity,
                ReturnOnAssets = record.ReturnOnAssets,
                ReturnOnInvested = record.ReturnOnInvested,
                LiquidityRatio = record.LiquidityRatio,
                LiabilitiesToEquityRatio = record.LiabilitiesToEquityRatio,
                DebtRatio = record.DebtRatio,
                DividendPayoutRatio = record.DividendPayoutRatio,
                GrossProfitMarginResponse = ai_responses[0],
                OperatingMarginResponse = ai_responses[1],
                NetProfitMarginResponse = ai_responses[2],
                ReturnOnEquityResponse = ai_responses[3],
                ReturnOnAssetsResponse = ai_responses[4],
                ReturnOnInvestedResponse = ai_responses[5],
                LiquidityRatioResponse = ai_responses[6],
                LiabilitiesToEquityRatioResponse = ai_responses[7],
                DebtRatioResponse = ai_responses[8],
                DividendPayoutRatioResponse = ai_responses[9]
            };

            return vm;
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
                // https://www.nuget.org/packages/OpenAI
                ChatClient client = new(
                    model: "gpt-4o",
                    apiKey: api_key
                );

                // retrieve stock information
                StockEconomicalInfo stockRecord = await _dbContext.GetSpecificStockReport(q, t, year_int);

                // call func to create specific viewmodel of stock information to pass to AI analyzation
                StockRecordInfoForAIViewModel infoForAi = ConvertStockRecord(stockRecord);

                List<ChatMessage> prompt_messages = [
                    returnPrompt(),
                    Newtonsoft.Json.JsonConvert.SerializeObject(infoForAi)
                ];

                List<ChatMessage> response_messages = [];

                // process chunks of results as they come in
                // AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = client.CompleteChatStreamingAsync(messages);

                // in this case we want to process entirety of responses at once so we can increment through them
                // as we already know the order they should return in
                ChatCompletion completion = await client.CompleteChatAsync(prompt_messages);
                List<string> message_stream = new List<string>();

                // should be 10 responses
                for (int i = 0; i < 10; i++)
                {
                    if (completion.Content[i].Text == null)
                    {
                        TempData["ValidationMsg"] = "The OpenAI response incurred an error. Please try again.";
                        return RedirectToAction("FinanceAnalyzation", "Stock");
                    } else
                    {
                        message_stream.Add(completion.Content[i].Text);
                    }
                }

                // call function with stockrecordinfoviewmodel and message_stream
                // to create viewmodel for ai summarization page
                AIFinanceSummaryViewModel vm = CreateAISummaryViewModel(stockRecord, infoForAi, message_stream);

                return View("AIFinanceSummarization", vm);
            }
            else
            {
                // if any value empty, return to the stock choice screen with error msg
                TempData["ValidationMsg"] = "One of your values was missing for the AI summarization. Please re-submit your choices.";
                return RedirectToAction("FinanceAnalyzation", "Stock");
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetFilingAISummarization([DataSourceRequest] DataSourceRequest request, FilingsChoicesViewModel vm)
        {
            try
            {
                if (vm.Ticker == null || vm.Year == null)
                {
                    TempData["ValidationMsg"] = "Missing a parameter. Please retry.";
                    return View("AIFilingSummarizationChoices");
                }
                else
                {

                    // use information to retrieve filings from simfin
                    string url = $"https://prod.simfin.com/api/v3/filings/by-company?ticker={vm.Ticker}";

                    RestRequest _request = new RestRequest();
                    RestClient _restClient = new RestClient();
                    RestClientOptions _restClientOptions = new RestClientOptions();
                    _restClientOptions = new RestClientOptions(url);

                    // instantiate client
                    _restClient = new RestClient(_restClientOptions);
                    _request = new RestRequest("");
                    _request.AddHeader("Accept", "application/json, text/plain, */*");
                    _request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");

                    // check if user has attributed simfin key
                    var user = _supabaseClient.Auth.CurrentUser;

                    // retrieve user api keys based on email
                    UserAPIKeys keys = await _dbContext.GetUserAPIKeys(user.Email);

                    // add api key/auth header
                    _request.AddHeader("Authorization", keys.Simfin_Key);

                    // get link using params and return as jsonified content
                    var response = await _restClient.GetAsync(_request);

                    // return response
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // retrieve most recent filing from this list
                        string data = response.Content;

                        // perform AI analyzation
                        StringBuilder contentBuilder = new();

                        // instantiate openAI chat model using api
                        ChatClient client = new(
                            model: "gpt-4o",
                            apiKey: keys.OpenAI_Key
                        );

                        List<ChatMessage> prompt_messages = [
                            returnPromptFiling(),
                            Newtonsoft.Json.JsonConvert.SerializeObject(recentFiling)
                        ];

                        List<ChatMessage> response_messages = [];
                        ChatCompletion completion = await client.CompleteChatAsync(prompt_messages);
                        List<string> message_stream = new List<string>();

                        // process in chunks?

                        int i = 0;
                        while (completion.Content[i] != null)
                        {
                            message_stream.Add(completion.Content[i].Text);
                            i++;
                        }

                        // create viewmodel of passed back information
                    }


                    // create viewmodel of information




                    return View("AIFilingSummarization");
                }
            }
            catch (Exception ex)
            {
                TempData["ValidationMsg"] = "Error with submission. Please retry.";
                return View("AIFilingSummarizationChoices");
            }
        }

    }
}