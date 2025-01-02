﻿using System;
using System.Collections.Generic;
using System.Web.Razor.Parser.SyntaxTree;
using AleStock.Models;
using Microsoft.EntityFrameworkCore;
using Telerik.SvgIcons;
using Supabase;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Supabase.Postgrest;
// using Supabase.Interfaces;


namespace Ale.Models;

public partial class StockDbContext : DbContext
{

    private string connString;

    private readonly Supabase.Client _supabaseClient;
    private readonly Supabase.Postgrest.Client _pgClient;


    public virtual DbSet<StockEconomicalInfo> StockEconomicalReports { get; set; }
    public virtual DbSet<UserAPIKeys> APIKeyLookup { get; set; }

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

    public async Task<UserAPIKeys> GetUserAPIKeys(string email) 
    {
        try 
        {
            var result = await _supabaseClient.From<UserAPIKeys>().Where(e => e.Email == email).Get();
            return result.Model;
        } catch (Exception ex) {
            return new UserAPIKeys();
        }
    }

    // INSERT DB operations
    public async Task<HttpResponseMessage?> SubmitStockReport(StockEconomicalInfo model)
    {
        var result = await _supabaseClient.From<StockEconomicalInfo>().Insert(model);
        return result.ResponseMessage;
    }

    public async Task<HttpResponseMessage?> SubmitAPIKeys(UserAPIKeys model) 
    {
        var result = await _supabaseClient.From<UserAPIKeys>().Insert(model);
        return result.ResponseMessage;
    }

    public async Task<HttpResponseMessage?> UpdateOpenAIKey(UserAPIKeys model) 
    {
        var result = await _supabaseClient.From<UserAPIKeys>().Where(x => x.Email == model.Email).Set(x => x.OpenAI_Key, model.OpenAI_Key).Update();
        return result.ResponseMessage;
    }

    public async Task<HttpResponseMessage?> UpdateSimfinAPIKey(UserAPIKeys model) 
    {
        var result = await _supabaseClient.From<UserAPIKeys>().Where(x => x.Email == model.Email).Set(x => x.Simfin_Key, model.Simfin_Key).Update();
        return result.ResponseMessage;
    }



    public async Task CreateUser(string email, string password)
    {
        var session = await _supabaseClient.Auth.SignUp(email, password);   
    }

    public async Task SignIn(string email, string password)
    {
        var session = await _supabaseClient.Auth.SignIn(email, password);
    }

}
