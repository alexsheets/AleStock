using Ale.Models;
using AleStock.Models;
using Microsoft.AspNetCore.Mvc;

namespace AleStock.Controllers.Home
{
    public class HomeController : Controller
    {

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        // instantiate db context 
        StockDbContext _dbContext;

        // create supabase client and init connection string
        public HomeController(StockDbContext context) => _dbContext = context;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(SignIn credentials)
        {
            if (credentials.Username == null || credentials.Password == null)
            {
                TempData["ValidationMsg"] = "Missing username or password.";
                return View("Register");
            }

            await _dbContext.CreateUser(credentials.Username, credentials.Password);

            return View("Index", "Home");

        }

        [HttpPost]
        public async Task<ActionResult> SignIn(SignIn credentials)
        {
            if (credentials.Username == null || credentials.Password == null)
            {
                TempData["ValidationMsg"] = "Missing username or password.";
                return View("Index");
            }

            await _dbContext.SignIn(credentials.Username, credentials.Password);

            return View("FinanceAnalyzation", "Stock");

        }
    }
}
