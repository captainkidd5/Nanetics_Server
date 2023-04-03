using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.DependencyInjections;
using Core;
using DatabaseServices;
using SilverMenu.Configurations;
using SilverMenu.DependencyInjections.Authentication;
using SilverMenu.DependencyInjections.Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.Authentication;
using Models.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Services;
using System.Configuration;
using System.Net;
using System.Text;
using SilverMenu.Configurations;
using SilverMenu.DependencyInjections.Authentication;
using SilverMenu.DependencyInjections.Azure;
using Core;
using Core.DependencyInjections;
using Models;
using SilverMenu.DependencyInjections.Email;
using ExceptionHandling.CustomMiddlewares;
using SendGrid.SmtpApi;
using Microsoft.AspNetCore.Identity;
using SilverMenu.DependencyInjections.S3;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);





builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowAll", builder =>
    builder.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});

var mapperConfig = new MapperConfiguration(cfg =>
cfg.AddProfile(new MapperInitializer())
);



builder.Services.AddAutoMapper(typeof(MapperInitializer));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000/",
                                              "http://www.localhost:3000/",
                              "http://localhost:3001/",
                              "http://localhost:3001",
                              "http://www.localhost:3001/",

                                              "http://localhost:3000",
                                              "http://localhost:19006/",
                                              "http://www.localhost:19006/",
                                              "http://localhost:19006");

                          policy.AllowAnyHeader();
                          policy.AllowAnyMethod();
                          policy.AllowCredentials();

                      });
});
SwaggerConfiguration swaggerConfig = new SwaggerConfiguration(builder);

swaggerConfig.Build();




builder.Services.AddScoped<IKeyVaultRetriever, KeyVaultRetriever>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IS3Helper, S3Helper>();

string keyVaultName = builder.Configuration.GetSection("Azure").GetSection("KeyVaultName").Value;
string kvUri = "https://" + keyVaultName + ".vault.azure.net";
SecretClient secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

var secret = secretClient.GetSecret("GooglePlacesAPIKey");
string dbSecret = secretClient.GetSecret("ConnectionStrings--switchcountdb").Value.Value;
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(dbSecret));

Serilog.Log.Logger = new LoggerConfiguration()
    .WriteTo.MSSqlServer(dbSecret,
        new MSSqlServerSinkOptions
        {
            TableName = "Logs",
            SchemaName = "dbo",
            AutoCreateSqlTable = true
        }

        ).MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
        .CreateLogger();









builder.Host.UseSerilog();


builder.Services.AddHostedService<LoggerWorker>();
builder.Services.AddHostedService<PushWorker>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IAuthManager, AuthManager>();

//CacheConfiguration cacheConfig = new CacheConfiguration(builder);
//cacheConfig.Build();
IdentityConfiguration identityConfig = new IdentityConfiguration(builder, secretClient);
identityConfig.Build();
identityConfig.BuildJWT();
//RateConfiguration rateLimitConfig = new RateConfiguration(builder);

//rateLimitConfig.Build();

VersionConfiguration versionConfig = new VersionConfiguration(builder);
versionConfig.Build();

builder.Services.AddControllers(config =>
{
    //config.CacheProfiles.Add("LongDuration", new CacheProfile
    //{
    //    Duration = 5
    //});
});
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

}
swaggerConfig.ConfigureMiddleware(app);
app.UseCors(MyAllowSpecificOrigins);
//cacheConfig.ConfigureMiddleware(app);
//rateLimitConfig.ConfigureMiddleware(app);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging();

app.MapControllers();
app.UseCors(MyAllowSpecificOrigins);

app.UseMiddleware<ExceptionHandlingMiddleware>();


using (var scope = app.Services.CreateScope())
    try
    {
        var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        ApplicationUser user = _context.Users.FirstOrDefault(x => x.UserName == "waiikipomm@gmail.com");
        if (user == null)
        {
            user = new ApplicationUser()
            {
                Id = Guid.NewGuid(),
                ConcurrencyStamp = "f3a6223f-ac2d-47d9-87d7-e297ea1b78e4",
                Email = "waiikipomm@gmail.com",
                NormalizedEmail = "WAIIKIPOMM@GMAIL.COM",
                EmailConfirmed = true,
                UserName = "waiikipomm@gmail.com",
                NormalizedUserName = "WAIIKIPOMM@GMAIL.COM",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConfirmationCode = "000000",
            };
            await _userManager.CreateAsync(user, "Runescape1!");
        }

       await _roleManager.CreateAsync(new ApplicationRole() { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER" });
        await _roleManager.CreateAsync(new ApplicationRole() { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN" });

        await _userManager.AddToRoleAsync(user, "User");
        await _userManager.AddToRoleAsync(user, "Admin");

        var roles = await _userManager.GetRolesAsync(user);

        Console.WriteLine(roles);
    }
    catch (Exception ex)
    {
        Console.WriteLine("test");
    }
app.Run();



