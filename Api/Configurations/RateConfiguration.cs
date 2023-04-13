using AspNetCoreRateLimit;
using Core;
using WaiikiCore;

namespace Api.Configurations
{
    internal class RateConfiguration : ConfigurationBase
    {
        public RateConfiguration(WebApplicationBuilder builder) : base(builder)
        {
        }

        public override void Build()
        {
            base.Build();
            List<RateLimitRule> rateLimitRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    //* means every endpoint
                    Endpoint = "*",
                    //1 call per 1 second
                    Limit = 15,
                    Period = "1s"
                }
            };

            Builder.Services.Configure<IpRateLimitOptions>(opt =>
            {
                opt.GeneralRules = rateLimitRules;
            });

            Builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            Builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            Builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            Builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();


            Builder.Services.AddHttpContextAccessor();
        }

        public override void ConfigureMiddleware(WebApplication app)
        {
            base.ConfigureMiddleware(app);
            app.UseIpRateLimiting();
        }
    }
}
