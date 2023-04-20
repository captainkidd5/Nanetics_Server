using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DatabaseServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Models.Authentication;
using System.Text;

namespace Core
{
    public class IdentityConfiguration : ConfigurationBase
    {
        private readonly SecretClient _keyVaultRetriever;

        public IdentityConfiguration(WebApplicationBuilder configuration, SecretClient keyVaultRetriever) : base(configuration)
        {
            _keyVaultRetriever = keyVaultRetriever;
        }

        public override void Build()
        {
            base.Build();
            BuildApplicationUser();
        }

        private void BuildApplicationUser()
        {

            Builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;


            }).AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider)


                       .AddEntityFrameworkStores<AppDbContext>();
            Builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
        o.TokenLifespan = TimeSpan.FromHours(12));
            Builder.Services.Configure<IdentityOptions>(opts =>
            {
                opts.Password.RequiredLength = 1;
                opts.Password.RequireDigit = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;

                opts.User.RequireUniqueEmail = true;
            });
        }

        /// <summary>
        /// Only build this in the api project
        /// </summary>
        public void BuildJWT()
        {
            IConfigurationSection jwtSettings = Builder.Configuration.GetSection("Jwt");
            string keyVaultName = Builder.Configuration.GetSection("Azure").GetSection("KeyVaultName").Value;
            string kvUri = "https://" + keyVaultName + ".vault.azure.net";
            SecretClient secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            Builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.GetSection("Issuer").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_keyVaultRetriever.GetSecret("JwtKey").Value.Value))

                };
            }).AddGoogle(o =>
            {
                o.ClientId = secretClient.GetSecret("google-auth-client-id").Value.Value;

                o.ClientSecret = secretClient.GetSecret("google-auth-client-secret").Value.Value;

            });
        }
    }
}
