using KurumTahsilatSistemi.Data;
using KurumTahsilatSistemi.MachineLearning.Services;
using KurumTahsilatSistemi.MachineLearning.Training;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpClient<
    IAiChatService,
    OllamaChatService>(
    client =>
    {
        client.BaseAddress =
            new Uri("http://localhost:11434/");

        client.Timeout =
            TimeSpan.FromMinutes(3);
    });

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;

    options.Cookie.Name = ".KurumTahsilat.AiSession";
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedAccount = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";

        options.Cookie.Name =
            "KurumTahsilatOturumu";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

builder.Services.AddScoped<SentetikOdemeVerisiUretici>();
builder.Services.AddScoped<
    OdemeRiskModelTrainer>();

builder.Services.AddScoped<
    IOdemeRiskTahminService,
    OdemeRiskTahminService>();

builder.Services.AddScoped<IBildirimService,
    BildirimService>();

builder.Services.AddScoped<
    IVadeBildirimService,
    VadeBildirimService>();

builder.Services.AddScoped<
    IOdemeKurallariService,
    OdemeKurallariService>();

builder.Services.AddScoped<
    ITahsilatService,
    TahsilatService>();


QuestPDF.Settings.License =
    LicenseType.Community;



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var vadeServisi =
            scope.ServiceProvider
                .GetRequiredService<IVadeBildirimService>();

        await vadeServisi.KontrolEtAsync();
    }
    catch (Exception hata)
    {
        var logger =
            scope.ServiceProvider
                .GetRequiredService<ILogger<Program>>();

        logger.LogError(
            hata,
            "Baþlangýç vade kontrolü gerçekleþtirilemedi.");
    }
}


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

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await IdentitySeed.SeedAsync(
        scope.ServiceProvider);
}

app.Run();
