using System;
using System.Collections.Generic;
using System.Web.Razor.Parser.SyntaxTree;
using AleStock.Models;
using AleStock.Models.TestModels;
using Microsoft.EntityFrameworkCore;
using Telerik.SvgIcons;
using Supabase;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Supabase.Postgrest;
// using Supabase.Interfaces;


namespace Ale.Models;

public partial class TestStockDbContext : DbContext
{

    private string connString;

    private readonly Supabase.Client _supabaseClient;
    // private readonly Supabase.Postgrest.Client _pgClient;

    public virtual DbSet<TestStockEconomicalReport> TestStockEconomicalReports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder();
        builder.AddJsonFile("appsettings.json", optional: false);

        var _configuration = builder.Build();

        connString = _configuration.GetConnectionString("StockDb");
        optionsBuilder.UseNpgsql(connString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
        base.OnConfiguring(optionsBuilder);
    }

    // RETRIEVAL DB operations
    public async Task<List<TestStockEconomicalReport>> GetAllTestStockReports()
    {
        try
        {
            var result = await _supabaseClient.From<TestStockEconomicalReport>().Get();
            return result.Models;
        }
        catch (Exception ex)
        {
            return new List<TestStockEconomicalReport>();
        }
    }

    public async Task<TestStockEconomicalReport> GetSpecificTestStockReport(string ticker, string quarter, int year)
    {
        try
        {
            var result = await _supabaseClient.From<TestStockEconomicalReport>()
                .Where(e => (e.Ticker == ticker) && (e.Quarter == quarter) && (e.Year == year)).Get();
            return result.Model;
        }
        catch (Exception ex)
        {
            return new TestStockEconomicalReport();
        }
    }

    // INSERT DB operations
    public async Task<HttpResponseMessage?> SubmitTestStockReport(TestStockEconomicalReport model)
    {
        var result = await _supabaseClient.From<TestStockEconomicalReport>().Insert(model);
        return result.ResponseMessage;
    }

}

