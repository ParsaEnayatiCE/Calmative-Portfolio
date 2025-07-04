using Calmative.Web.App.Models;
using Calmative.Web.App.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure data protection to use a persistent key storage location
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")));

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Configure custom model binding for better error handling
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "این فیلد الزامی است");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(value => $"مقدار '{value}' نامعتبر است");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, fieldName) => 
        $"مقدار '{value}' برای {fieldName} نامعتبر است");
        
    // Register custom model binder for AssetType
    options.ModelBinderProviders.Insert(0, new AssetTypeModelBinderProvider());
});

// Add authentication and authorization services
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });
builder.Services.AddAuthorization();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add memory cache
builder.Services.AddMemoryCache();

// Add HttpClient for API calls
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000/api/");
});

// Add AssetTypeService
builder.Services.AddScoped<IAssetTypeService, AssetTypeService>();

// Add session support for storing JWT tokens
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Add explicit route for Recommendation controller
app.MapControllerRoute(
    name: "recommendation",
    pattern: "Recommendation/{action=Index}/{id?}",
    defaults: new { controller = "Recommendation" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
