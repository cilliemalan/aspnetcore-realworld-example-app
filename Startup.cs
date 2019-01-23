using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConduitApi.Data;
using ConduitApi.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConduitApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(s => Authentication.CreateHmac(Configuration["SecretKey"]));

            services.AddDbContext<ConduitDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString(nameof(ConduitDbContext))));

            services.AddMvc(o => o.UseRoutePrefix("api"))
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    o.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    o.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = AuthenticationOptions.DefaultScheme;
            }).AddBasicTokenAuthentication();

            services.AddCors();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ConduitDbContext>();
                context.Database.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            

            app.UseHttpsRedirection();
            if (Configuration["EnableCORS"] == "True")
            {
                app.UseCors(o => o.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            }
            app.UseErrorHandling();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
