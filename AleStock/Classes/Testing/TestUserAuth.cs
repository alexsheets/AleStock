using Xunit;
using FluentAssertions;
using AleStock.Models.TestModels;
using Telerik.SvgIcons;
using Microsoft.EntityFrameworkCore;
using dotenv;
using dotenv.net;
using Supabase;
using AleStock.Models;

namespace AleStock.Classes.Testing
{
    public class TestUserAuth : IDisposable
    {

        TestStockDbContext _context;
        private readonly Supabase.Client _supabaseClient;
        // private readonly Supabase.Postgrest.Client _pgClient;
        private readonly IConfiguration _configuration;

        public TestUserAuth(IConfiguration configuration)
        {

            _configuration = configuration;
            _supabaseClient = new Supabase.Client(_configuration["SecretSection:url"], _configuration["SecretSection:key"]);
            _context = new TestStockDbContext(configuration);
            _context.Database.EnsureDeleted();
            _context.Database.Migrate();

        }

        // for cleanup, we implement IDisposable interface and place cleanup code here
        // xunit does the rest
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task TestSignInAndRetrieveUser()
        {
            // retrieve username and password from env variables
            DotEnv.Load();
            string email = Environment.GetEnvironmentVariable("TEST_EMAIL");
            string password = Environment.GetEnvironmentVariable("TEST_PASSWORD");

            var session = await _context.TestSignIn(email, password);
            string _clientEmail = _supabaseClient.Auth.CurrentUser.Email;

            _clientEmail.Should().BeEquivalentTo(email);
        }

    }
}
