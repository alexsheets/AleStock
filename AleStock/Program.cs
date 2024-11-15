using Ale.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(o => o.SerializerSettings.ContractResolver = new DefaultContractResolver());

builder.Services.AddRazorPages();
builder.Services.AddKendo();
builder.Services.AddControllers();
// builder.Services.AddServerSideBlazor();
builder.Services.AddMvc();
builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(500));
builder.Services.AddHttpContextAccessor();

// add database
builder.Services.AddDbContextPool<StockDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("StockDb")));



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




app.Run();