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
        public ActionResult SignIn(SignIn credentials)
        {
            if (credentials.Username == null || credentials.Password == null)
            {
                TempData["ValidationMsg"] = "Missing username or password.";
                return View("Index");
            }

            // set up authentication



        }
    }
}
