using AleStock.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace AleStock.Controllers.Home
{
    public class HomeController : Controller
    {

        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        StockDbContext _dbContext;
        private readonly Supabase.Client _supabaseClient;
        // private readonly Supabase.Postgrest.Client _pgClient;
        private readonly IConfiguration _configuration;

        // class member instantiation
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _supabaseClient = new Supabase.Client(_configuration["SecretSection:url"], _configuration["SecretSection:key"]);
            _dbContext = new StockDbContext(configuration);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult HomePage()
        {
            Supabase.Gotrue.Session session = _dbContext.GetSession();
            if (session != null)
            {
                return View();
            }
            else
            {
                TempData["ValidationMsg"] = "Error with authenticating the current session. Please re-login.";
                return View("Index");
            }

        }

        [HttpPost]
        public async Task<ActionResult> Register(SignIn credentials)
        {
            if (credentials.Username == null || credentials.Password == null)
            {
                TempData["ValidationMsg"] = "Missing username or password.";
                return View("Register");
            }

            Supabase.Gotrue.Session session = await _dbContext.CreateUser(credentials.Username, credentials.Password);

            if (session != null)
            {
                return View("HomePage");
            }
            else
            {
                TempData["ValidationMsg"] = "Error with your login. Please retry or register.";
                return View("Index");
            }

        }

        [HttpPost]
        public async Task<ActionResult> SignIn(SignIn credentials)
        {
            if (credentials.Username == null || credentials.Password == null)
            {
                TempData["ValidationMsg"] = "Missing username or password.";
                return View("Index");
            }

            Supabase.Gotrue.Session session = await _dbContext.SignIn(credentials.Username, credentials.Password);

            if (session != null)
            {
                return View("HomePage");
            } else
            {
                TempData["ValidationMsg"] = "Error with your login. Please retry or register.";
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult SendEmail(SignIn credentials)
        {
            try
            {
                if (ModelState.IsValid)
                {


                    var senderEmail = new MailAddress("");
                    var receiverEmail = new MailAddress(credentials.Username);
                    var body = "";
                    var smtp = new SmtpClient
                    {
                        Host = "",
                        Port = 2,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = true,
                        // Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = "New user information",
                        Body = body
                    })
                    {
                        smtp.Send(mess);
                    }
                    TempData["ValidationMsg"] = "Email successfully sent.";
                    return View("Index");
                }
            }
            catch (Exception)
            {
                TempData["ValidationMsg"] = "Error occurred.";
                return View("Index");
            }
            return View("Index");
        }
    }
}
