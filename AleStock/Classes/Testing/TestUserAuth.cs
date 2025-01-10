using Xunit;
using FluentAssertions;
using Ale.Models;
using AleStock.Models.TestModels;
using Telerik.SvgIcons;
using Microsoft.EntityFrameworkCore;
using dotenv;
using dotenv.net;
using Supabase;

namespace AleStock.Classes.Testing
{
    public class TestUserAuth : IDisposable
    {
        TestStockDbContext _context;

        Client _client = new Supabase.Client("", "");

        public TestUserAuth()
        {
            // instantiate a new connection to DB by creating new context
            // db connection is handled in the class
            _context = new TestStockDbContext();

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
            string _clientEmail = _supabase.Auth.CurrentUser.Email;

            _clientEmail.Should().BeEquivalentTo(email);
        }

    }
}
