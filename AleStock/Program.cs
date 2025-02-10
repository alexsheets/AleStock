using AleStock.Hubs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using dotenv;
using dotenv.net;

// added for frontend
using React.AspNet;
using JavaScriptEngineSwitcher.V8;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// add c# chat package
builder.Services.AddSignalR();

// add react/frontend services
builder.Services.AddReact();
builder.Services.AddJsEngineSwitcher(options => options.DefaultEngineName = V8JsEngine.EngineName).AddV8();

// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(o => o.SerializerSettings.ContractResolver = new DefaultContractResolver());

builder.Services.AddRazorPages();
builder.Services.AddKendo();
builder.Services.AddControllers();
builder.Services.AddMvc();
builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(500));
builder.Services.AddHttpContextAccessor();

// calling load with no parameters locates .env file in same directory as library
DotEnv.Load();

var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");

// instantiate supabase client to access throughout project
var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true,
    AutoRefreshToken = true,
};
var supabase = new Supabase.Client(url, key, options);
await supabase.InitializeAsync();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.MapRazorPages();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// setup for chat endpoint
app.MapHub<ChatHub>("/chatHub");

// setup for adding react files
app.UseReact(config =>
{
    // pulls from /scripts/ subdir in wwwroot
    config.AddScript("~/scripts/Example.jsx");
});
app.UseStaticFiles();


app.Run();