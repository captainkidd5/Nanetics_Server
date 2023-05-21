using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.DependencyInjections;
using Core;
using DatabaseServices;
using Api.Configurations;
using Api.DependencyInjections.Authentication;
using Api.DependencyInjections.Azure;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Models;
using Api.DependencyInjections.Email;
using ExceptionHandling.CustomMiddlewares;
using Microsoft.AspNetCore.Identity;
using Api.DependencyInjections.S3;
using Core.DependencyInjections.MQTT;
using Api.DependencyInjections.IoT;
using Microsoft.Extensions.Http;
using Api.CustomMiddlewares;
using Microsoft.AspNetCore.Authorization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);




try
{

    var mapperConfig = new MapperConfiguration(cfg =>
    cfg.AddProfile(new MapperInitializer())
    );


    var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

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
                                                  "http://localhost:19006",

                                                  "http://192.168.4.39",
                                                     "http://192.168.4.21",
                                                     "http://192.168.1.184"

                                                  );
                              // policy.AllowAnyOrigin();

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
    builder.Services.AddTransient<IDeviceRegistryService, DeviceRegistryService>();
    builder.Services.AddTransient<IIotService, IoTService>();


    string keyVaultName = builder.Configuration.GetSection("Azure").GetSection("KeyVaultName").Value;
    string kvUri = "https://" + keyVaultName + ".vault.azure.net";
    SecretClient secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

    //var secret = secretClient.GetSecret("GooglePlacesAPIKey");
    string dbSecret = secretClient.GetSecret("cs--naneticsdb").Value.Value;
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(dbSecret));

    //Serilog.Log.Logger = new LoggerConfiguration()
    //    .ReadFrom.Configuration(configuration)
    //    .CreateLogger();

    Serilog.Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
        .Enrich.WithProperty("Application", "Nanetics")
        .WriteTo.Seq("https://naneticsseq.azurewebsites.net:443")
    .CreateLogger();

    LoggerFactory loggerFactory = new LoggerFactory();
    Log.Information("Log session started");

    //To view logs - Search "seq" --> Browse Seq --> will open browser to localhost:5341. Settings are found for seq under serilog in appsettings.json
    //https://github.com/datalust/serilog-sinks-seq
    //https://docs.datalust.co/docs/using-serilog


    builder.Host.UseSerilog();
    builder.Services.AddSingleton<IMQTTService, MQTTService>();
    builder.Services.AddSingleton<IAuthorizationHandler, AuthorizationPolicies>();
    builder.Services.AddHostedService<PushWorker>();
    //builder.Services.AddHostedService<MQTTWorker>();


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
    builder.Services.AddAuthorization(options =>
      options.AddPolicy("LoggedIn",
      policy => policy.RequireRole("User")));
    builder.Services.AddControllers(config =>
    {

        config.Filters.Add(typeof(LogUnauthorizedRequestFilter));
    });
    WebApplication app = builder.Build();
    app.UseSerilogRequestLogging();

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



}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");

}
finally
{
    Log.CloseAndFlush();
}