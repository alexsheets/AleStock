using Xunit;
using FluentAssertions;
using AleStock.Models.TestModels;
using Telerik.SvgIcons;
using Microsoft.EntityFrameworkCore;
using AleStock.Models;

namespace AleStock.Classes.Testing
{
    public class StockFunctionsTesting : IDisposable
    {
        IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        TestStockDbContext _context;
        private readonly Supabase.Client _supabaseClient;
        // private readonly Supabase.Postgrest.Client _pgClient;
        private readonly IConfiguration _configuration;

        public StockFunctionsTesting(IConfiguration configuration)
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
        public async Task TestSubmitAndRetrieveStockReport()
        {
            // create stock economical info viewmodel; only information non-nullable is ticker, quarter and year
            TestStockEconomicalReport info = new TestStockEconomicalReport();
            info.Ticker = "AAPL";
            info.Quarter = "Q3";
            info.Year = 2023;

            // attempt stock report submission
            await _context.SubmitTestStockReport(info);

            // save the changes to database
            await _context.SaveChangesAsync();

            // test by assertion
            var reports = await _context.GetAllTestStockReports();
            reports.Should().HaveCount(1);

            // test retrieval
            TestStockEconomicalReport testReport = await _context.GetSpecificTestStockReport(info.Ticker, info.Quarter, info.Year);
            testReport.Should().NotBeNull();
        }

    }
}