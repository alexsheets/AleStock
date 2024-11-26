using System;
using System.Collections.Generic;
using System.Web.Razor.Parser.SyntaxTree;
using AleStock.Models;
using Microsoft.EntityFrameworkCore;
using Telerik.SvgIcons;
using Supabase;
// using Supabase.Postgrest;
// using Supabase.Interfaces;


namespace Ale.Models;

public partial class StockDbContext : DbContext
{

    private string connString;

    private readonly Client _supabaseClient;

    public StockDbContext(Client supabaseClient) : base()
    {
        _supabaseClient = supabaseClient;

        var builder = new ConfigurationBuilder();
        builder.AddJsonFile("appsettings.json", optional: false);

        var _configuration = builder.Build();

        connString = _configuration.GetConnectionString("StockDb");
    }

    public virtual DbSet<StockEconomicalInfo> StockEconomicalReports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
        base.OnConfiguring(optionsBuilder);
    }

    // RETRIEVAL DB operations
    public async Task<List<StockEconomicalInfo>> GetAllStockReports()
    {
        try
        {
            var result = await _supabaseClient.From<StockEconomicalInfo>().Get();
            return result.Models;
        }
        catch (Exception ex)
        {
            return new List<StockEconomicalInfo>();
        }
    }

    public async Task<StockEconomicalInfo> GetSpecificStockReport(string ticker, string quarter, int year)
    {
        try
        {
            var result = await _supabaseClient.From<StockEconomicalInfo>()
                .Where(e => (e.Ticker == ticker) && (e.Quarter == quarter) && (e.Year == year)).Get();
            return result.Model;
        }
        catch (Exception ex)
        {
            return new StockEconomicalInfo();
        }
    }

    // INSERT DB operations
    public async 
    // INSERT DB operations
    Task
SubmitStockReport(StockEconomicalInfo model)
    {
        

        var result = await _supabaseClient.From<StockEconomicalInfo>().Insert(model);
        
    }

}
