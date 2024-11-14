using System;
using System.Collections.Generic;
using System.Web.Razor.Parser.SyntaxTree;
using AleStock.Models;
using Microsoft.EntityFrameworkCore;
using Telerik.SvgIcons;

namespace Ale.Models;

public partial class StockDbContext : DbContext
{

    private string connString;

    public StockDbContext() : base()
    {
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<StockEconomicalInfo>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Ticker).HasColumnName("Ticker");
            entity.Property(e => e.Quarter).HasColumnName("Quarter");
            entity.Property(e => e.Year).HasColumnName("Year");
            entity.Property(e => e.TotalAssets).HasColumnName("TotalAssets");
            entity.Property(e => e.TotalLiabilities).HasColumnName("TotalLiabilities");
            entity.Property(e => e.TotalEquity).HasColumnName("TotalEquity");
            entity.Property(e => e.ReturnOnAssets).HasColumnName("ReturnOnAssets");
            entity.Property(e => e.ReturnOnEquity).HasColumnName("ReturnOnEquity");
            entity.Property(e => e.ReturnOnInvested).HasColumnName("ReturnOnInvested");
            entity.Property(e => e.LiquidityRatio).HasColumnName("LiquidityRatio");
            entity.Property(e => e.LiabilitiesToEquityRatio).HasColumnName("LiabilitiesToEquityRatio");
            entity.Property(e => e.DebtRatio).HasColumnName("DebtRatio");
            entity.Property(e => e.TotalDebt).HasColumnName("TotalDebt");
            entity.Property(e => e.DividendPayoutRatio).HasColumnName("DividendPayoutRatio");
            entity.Property(e => e.DividendsPaid).HasColumnName("DividendsPaid");
            entity.Property(e => e.NetCashOperating).HasColumnName("NetCashOperating");
            entity.Property(e => e.NetCashFinancing).HasColumnName("NetCashFinancing");
            entity.Property(e => e.NetCashInvesting).HasColumnName("NetCashInvesting");
            entity.Property(e => e.NetCashDelta).HasColumnName("NetCashDelta");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
