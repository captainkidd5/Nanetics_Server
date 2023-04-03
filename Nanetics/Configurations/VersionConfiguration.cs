using Core;
using Microsoft.AspNetCore.Mvc;

namespace SilverMenu.Configurations
{
    internal class VersionConfiguration : ConfigurationBase
    {
        public VersionConfiguration(WebApplicationBuilder builder) : base(builder)
        {
        }
        public override void Build()
        {
            base.Build();
            //Builder.Services.AddApiVersioning(opt =>
            //{
            //    opt.ReportApiVersions = true;
            //    opt.AssumeDefaultVersionWhenUnspecified = true;
            //    opt.DefaultApiVersion = new ApiVersion(1, 0);
            //});
        }
    }
}
