

using Microsoft.AspNetCore.Builder;

namespace Core
{
    public abstract class ConfigurationBase
    {

        protected readonly WebApplicationBuilder Builder;


        public ConfigurationBase(WebApplicationBuilder builder)
        {
            Builder = builder;

        }


        public virtual void Build()
        {

        }

        public virtual void ConfigureMiddleware(WebApplication app)
        {

        }
    }
}
