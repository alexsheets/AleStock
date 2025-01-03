using Ale.Models;
using AleStock.Hubs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using dotenv;
using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

// add c# chat package
builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(o => o.SerializerSettings.ContractResolver = new DefaultContractResolver());

builder.Services.AddRazorPages();
builder.Services.AddKendo();
builder.Services.AddControllers();
// builder.Services.AddServerSideBlazor();
builder.Services.AddMvc();
builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(500));
builder.Services.AddHttpContextAccessor();

DotEnv.Load();

var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");

var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true
};

var supabase = new Supabase.Client(url, key, options);
await supabase.InitializeAsync();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.MapRazorPages();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// setup for chat endpoint
app.MapHub<ChatHub>("/Chat");


app.Run();