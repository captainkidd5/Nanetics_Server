using Microsoft.OpenApi.Models;
using Core;

namespace Api.Configurations
{
    internal class SwaggerConfiguration : ConfigurationBase
    {
        public SwaggerConfiguration(WebApplicationBuilder builder) : base(builder)
        {
        }

        public override void Build()
        {
            base.Build();
            Builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                    
                    
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"

                            },
                            Scheme="oauth2",
                            Name = "Bearer",
                           
                            In = ParameterLocation.Header
                        },
                        new string[] {}
                    }
                });
            });
        }

        public override void ConfigureMiddleware(WebApplication app)
        {
            base.ConfigureMiddleware(app);
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }

    }
}
