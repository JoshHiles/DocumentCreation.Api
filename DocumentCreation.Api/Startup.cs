using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentCreation.Api.Helpers;
using DocumentCreation.Api.Models;
using DocumentCreation.Api.Service;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NCalc;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Swagger;

namespace DocumentCreation.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
                {
                    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddScoped<IHandlebarHelper, HandlebarHelper>();
            services.AddScoped<IPdfCreationService, PdfCreationService>();
            services.AddScoped<IFileHelper, FileHelper>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Document Creation Api",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    In = "header",
                    Name = "Authorization",
                    Type = "apiKey",
                    Description = "Bearer {token}"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                    {
                        {
                        "Bearer",
                        new string[]{}
                        }
                    });
                c.DescribeAllEnumsAsStrings();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            CreateHandlebarsHelpers();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Creation Api V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseMvc();
        }

        private void CreateHandlebarsHelpers()
        {
            Handlebars.RegisterHelper("#DisplayAddress", (writer, context, parameters) =>
            {
                var jaddress = parameters[0] as JObject;
                if (jaddress != null)
                {
                    var address = jaddress.ToObject<Address>();
                    if (address.IsAutomaticallyFound)
                    {
                        writer.WriteSafeString("<b>" + address.SelectedAddress + "</b>");
                    }
                    else
                    {
                        writer.WriteSafeString("<p>Address line 1: <b>" + address.AddressLine1 + "</b></p>");
                        writer.WriteSafeString("<p>Address line 2: <b>" + address.AddressLine2 + "</b></p>");
                        writer.WriteSafeString("<p>Town: <b>" + address.Town + "</b></p>");
                    }
                    writer.WriteSafeString("<p>Postcode: <b>" + address.Postcode + "</b></p>");
                }
            });

            Handlebars.RegisterHelper("ShouldDisplayAddress", (writer, options, context, args) =>
            {
                if (args[0] is JObject jaddress)
                {
                    var address = jaddress.ToObject<Address>();
                    if (address.IsAutomaticallyFound || !string.IsNullOrEmpty(address.AddressLine1))
                    {
                        options.Template(writer, (object)context);
                    }
                }
            });

            Handlebars.RegisterHelper("#FormatDate", (writer, context, parameters) =>
            {

                if (!string.IsNullOrEmpty(parameters[0].ToString()))
                {
                    string newDate = parameters[0].ToString().Substring(0, 10);
                    writer.WriteSafeString(newDate);
                }

            });

            Handlebars.RegisterHelper("#FormatBoolean", (writer, context, parameters) =>
            {
                if (parameters[0].ToString().ToLower().Equals("true"))
                {
                    writer.WriteSafeString("Yes");
                }
                else
                {
                    writer.WriteSafeString("No");
                }

            });

            Handlebars.RegisterHelper("math", (output, context, parameters) =>
            {
                Expression e = new Expression(string.Format("{0}{1}{2}", parameters[0], parameters[1], parameters[2]));
                output.WriteSafeString(e.Evaluate().ToString());

            });

            Handlebars.RegisterHelper("#Capital", (output, context, parameters) =>
            {
                char[] a = parameters[0].ToString().ToCharArray();
                a[0] = char.ToUpper(a[0]);
                string s = new string(a);
                output.WriteSafeString(s);
            });

            Handlebars.RegisterHelper("HasValue", (writer, options, context, args) =>
            {
                if (!string.IsNullOrEmpty(args[0].ToString().Trim()) && args[0].ToString() != "[]" && args[0].ToString() != "01/01/0001 00:01:15")
                {
                    options.Template(writer, (object)context);
                }
            });
        }
    }
}
