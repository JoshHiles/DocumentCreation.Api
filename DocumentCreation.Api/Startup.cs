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

            Handlebars.RegisterHelper("#FormatDateMoveOutDateOnly", (writer, context, parameters) =>
            {
                string newDate = context.ToString().Substring(0, 10);
                writer.WriteSafeString(newDate);
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

            Handlebars.RegisterHelper("#FormatPropertyYoureLeaving", (output, context, parameters) =>
            {
                var propertyYoureLeaving = parameters[0].ToString();
                var formattedPropertyYoureLeaving = "";

                switch (propertyYoureLeaving.ToLower())
                {
                    case "owner":
                        formattedPropertyYoureLeaving = "Owner";
                        break;
                    case "tenant":
                        formattedPropertyYoureLeaving = "Tenant";
                        break;
                    case "resident-counciltaxpayer":
                        formattedPropertyYoureLeaving = "Resident - Council Tax payer";
                        break;
                    case "resident-noncounciltaxpayer":
                        formattedPropertyYoureLeaving = "Resident - Non Council Tax payer";
                        break;
                }

                output.WriteSafeString(formattedPropertyYoureLeaving);
            });

            Handlebars.RegisterHelper("#FormatWhatsHappeningNow", (output, context, parameters) =>
            {
                var propertyYoureLeaving = parameters[0].ToString();
                var formattedPropertyYoureLeaving = "";

                switch (propertyYoureLeaving.ToLower())
                {
                    case "unoccupied":
                        formattedPropertyYoureLeaving = "Unoccupied";
                        break;
                    case "renovations":
                        formattedPropertyYoureLeaving = "Unoccupied for renovations";
                        break;
                    case "tenants":
                        formattedPropertyYoureLeaving = "Tenants moving in";
                        break;
                    case "family":
                        formattedPropertyYoureLeaving = "Occupied by family members";
                        break;
                    case "repossessed":
                        formattedPropertyYoureLeaving = "The property was repossessed";
                        break;
                }

                output.WriteSafeString(formattedPropertyYoureLeaving);
            });

            Handlebars.RegisterHelper("HasValue", (writer, options, context, args) =>
            {
                if (!string.IsNullOrEmpty(args[0].ToString().Trim()) && args[0].ToString() != "[]" && args[0].ToString() != "01/01/0001 00:01:15")
                {
                    options.Template(writer, (object)context);
                }
            });

            Handlebars.RegisterHelper("#FormatRelationshipToApplicant", (output, context, parameters) =>
            {
                var relationshipToApplicant = parameters[0].ToString();
                var formattedRelationshipToApplicant = "";

                switch (relationshipToApplicant.ToLower())
                {
                    case "partner":
                        formattedRelationshipToApplicant = "Partner";
                        break;
                    case "familymember":
                        formattedRelationshipToApplicant = "Family Member";
                        break;
                    case "jointownertenant":
                        formattedRelationshipToApplicant = "Joint owner/tenant";
                        break;
                    case "lodger":
                        formattedRelationshipToApplicant = "Lodger";
                        break;
                    case "other":
                        formattedRelationshipToApplicant = "Other";
                        break;
                }

                output.WriteSafeString(formattedRelationshipToApplicant);
            });

            Handlebars.RegisterHelper("#FormatSMI", (output, context, parameters) =>
            {
                var relationshipToApplicant = parameters[0].ToString();
                var formattedRelationshipToApplicant = "";

                switch (relationshipToApplicant.ToLower())
                {
                    case "invalidity pension":
                        formattedRelationshipToApplicant = "Invalidity pension";
                        break;
                    case "unemployability supplement":
                        formattedRelationshipToApplicant = "Unemployability supplement";
                        break;
                    case "attendance allowance":
                        formattedRelationshipToApplicant = "Attendance Allowance";
                        break;
                    case "working tax credit with a disability element":
                        formattedRelationshipToApplicant = "Working Tax Credit with a disability element";
                        break;
                    case "constant attendance allowance":
                        formattedRelationshipToApplicant = "Constant Attendance Allowance";
                        break;
                    case "unemployability allowance":
                        formattedRelationshipToApplicant = "Unemployability allowance";
                        break;
                    case "an increase in the rate of disablement pension where constant attendance is needed":
                        formattedRelationshipToApplicant = "An increase in the rate of Disablement Pension where constant attendance is needed";
                        break;
                    case "the care component of a disability living allowance paid at the highest or middle rate":
                        formattedRelationshipToApplicant = "The care component of a Disability Living Allowance, paid at the highest or middle rate";
                        break;
                    case "income support where the applicable amount includes a disability premium":
                        formattedRelationshipToApplicant = "Income Support where the applicable amount includes a Disability Premium";
                        break;
                    case "independence payment":
                        formattedRelationshipToApplicant = "Other";
                        break;
                    case "severe disablement allowance":
                        formattedRelationshipToApplicant = "Severe Disablement Allowance";
                        break;
                    case "employment and support allowance (support component)":
                        formattedRelationshipToApplicant = "Employment and Support Allowance (Support Component)";
                        break;
                    case "the daily living component of a personal independence payment":
                        formattedRelationshipToApplicant =
                            "The daily living component of a Personal Independence Payment";
                        break;
                }

                output.WriteSafeString(formattedRelationshipToApplicant);
            });

        }
    }
}
