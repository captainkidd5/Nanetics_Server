
using Core;
using Marvin.Cache.Headers;

namespace SilverMenu.Configurations
{
    internal class CacheConfiguration : ConfigurationBase
    {
        public CacheConfiguration(WebApplicationBuilder builder) : base(builder)
        {
        }
        public override void Build()
        {
            base.Build();
            Builder.Services.AddMemoryCache();
            Builder.Services.AddResponseCaching();
            Builder.Services.AddHttpCacheHeaders(
                (expirationOpt) =>
                {
                    expirationOpt.MaxAge = 65;
                    expirationOpt.CacheLocation = CacheLocation.Private;
                },
                (validationOpt)=>
                {
                    validationOpt.MustRevalidate = true;
                }
                );
        }

        public override void ConfigureMiddleware(WebApplication app)
        {
            base.ConfigureMiddleware(app);
            app.UseResponseCaching();
            app.UseHttpCacheHeaders();
        }
    }
}
