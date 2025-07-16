// using necessary namespaces
using FilmService.Models; // Assuming your models are here
using FilmService.Services; // Assuming your services are here
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity; // For Identity if you plan to use it later, or just for claims
using Microsoft.EntityFrameworkCore;
using WebFilmOnline.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<FilmServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication services
builder.Services.AddAuthentication(options =>
{
    // Set the default scheme to Cookie authentication
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Set the default challenge scheme to handle unauthenticated requests
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    // Configure cookie options, e.g., login path, access denied path
    options.LoginPath = "/Account/Login"; // Path to your login page
    options.AccessDeniedPath = "/Account/AccessDenied"; // Path for access denied
})
.AddGoogle(googleOptions =>
{
    // Retrieve Google ClientId and ClientSecret from appsettings.json
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    // Specify the scope of information to request from Google
    googleOptions.Scope.Add("profile"); // Request profile information (name, picture)
    googleOptions.Scope.Add("email");   // Request email address
})
.AddFacebook(facebookOptions =>
{
    // Retrieve Facebook AppId and AppSecret from appsettings.json
    facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    // Specify the scope of information to request from Facebook
    facebookOptions.Scope.Add("email"); // Request email address
    facebookOptions.Scope.Add("public_profile"); // Request public profile information
});

// Register custom services
builder.Services.AddScoped<IUserService, UserService>(); // Register the user service interface and implementation

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

app.UseRouting();

// Enable authentication middleware
app.UseAuthentication();
// Enable authorization middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
