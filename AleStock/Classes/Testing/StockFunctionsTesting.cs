using Xunit;
using FluentAssertions;
using Ale.Models;
using Telerik.SvgIcons;
using Microsoft.EntityFrameworkCore;
using AleStock.Models;

namespace AleStock.Classes.Testing
{
    public class StockFunctionsTesting : IDisposable
    {
        private readonly TestStockDbContext _context;

        public StockFunctionsTesting()
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
        public async Task TestSubmitAndRetrieveStockReport()
        {
            // create stock economical info viewmodel
            StockEconomicalInfo info = new StockEconomicalInfo();


            // save the changes to database
            await _context.SaveChangesAsync();

            // test by assertion
            var reports = await _context.GetAllTestStockReports();
            reports.Should().HaveCount(1);

            // test retrieval
        }

    }
}