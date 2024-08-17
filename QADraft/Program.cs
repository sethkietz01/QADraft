using DotNetEnv;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Services;
using QADraft.Utilities;
using System;
using System.IO;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();

// Load configuration files
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<GeekQAService>();
// Add TimedHostedService background timer
//builder.Services.AddSingleton<SnipeItApiClient>();
//builder.Services.AddHostedService<TimedHostedService>();

// Configure Entity Framework to use SQL Server with connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     .Replace("your_hashed_password_here", Environment.GetEnvironmentVariable("DefaultConnectionPasswordHash"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

var app = builder.Build();

// Password Migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var users = context.Users.ToList();
    foreach (var user in users)
    {
        if (!IsPasswordHashed(user.Password))
        {
            user.Password = PasswordHasher.HashPassword(user.Password);
        }
    }
    context.SaveChanges();
}

bool IsPasswordHashed(string password)
{
    // Implement logic to check if the password is already hashed
    // For simplicity, assume that a hashed password length is 64 characters
    return password.Length == 64;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
}



app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable serving static files like CSS, JS, etc.

app.UseRouting();

// Use session before authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
