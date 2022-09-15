using FRW.PR.Extra.Extensions;
using FRW.PR.Extra.Middlewares;
using FRW.PR.Extra.Services;
using FRW.PR.Extra.Utils;
using FRW.PR.Extranet.Utils.Swagger;
using FRW.PR.Services;
using FRW.PR.Utils.Culture;
using FRW.TR.Commun.Http;
using FRW.TR.Commun.Services;
using FRW.TR.Contrats;
using Docnet.Core;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using FRW.TR.Commun;

namespace FRW.PR.Extra
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
            var resolver = CompositeResolver.Create(NativeDateTimeResolver.Instance, ContractlessStandardResolver.Instance);
            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            MessagePackSerializer.DefaultOptions = options;

            services.ConfigureRequestLocalization();

            services.AddScoped<IContexteDevAccesseur, ContexteDevAccesseur>();
            services.AddSingleton<IVueParser>(new VueParser());
            services.AddSingleton<AuthService>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                           .AddCookie(options =>
                           {
                               options.Cookie.Name = "FRWSessions";
                               options.Cookie.SameSite = SameSiteMode.Lax;
                               options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                           });

            services.AddControllers();
            services.AddRazorPages()
                    .AddRazorPagesOptions(options =>
                        {
                            options.Conventions.Add(new CultureTemplateRouteModelConvention());
                            options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                        })
                    .AddRazorRuntimeCompilation();

            services.AddDataProtection(options => options.ApplicationDiscriminator = "FRW")
                    .AddKeyManagementOptions(options =>
                    {
                        //options.XmlRepository = new ApiXmlRepository(Configuration);
                        options.NewKeyLifetime = TimeSpan.FromDays(90);
                    });

            services.AddHttpContextAccessor();
            services.AddHttpClient<IDorsale, Dorsale>()
                        .ConfigurePrimaryHttpMessageHandler(() =>
                        {
                            return FrwHttpMessageHandler.CreerMessageHandler(Configuration.GetValue<string>("FRW:NomCertificatClientProxy"));
                        });
            services.AddSingleton<IFormulairesService, FormulairesService>();
            services.AddScoped<IConfigurationApplication, ConfigurationApplicationPR>();
            services.AddSingleton<ISystemeAutoriseService, SystemeAutoriseService>();

            services.AddSingleton<IJournalisationService, JournalisationService>();
            services.AddSingleton<ISessionService, SessionService>();
            services.AddSingleton<ISuiviEtatService, SuiviEtatService>();
            services.AddSingleton<ITexteEditeService, TexteEditeService>();
            services.AddSingleton<IRIRService, RIRService>();

            services.AddSingleton<IStringLocalizer, YamlStringLocalizer>();
            services.AddSingleton<IStringLocalizerFactory, YamlStringLocalizerFactory>();
            services.AddSingleton<IHtmlLocalizer, YamlHtmlLocalizer>();
            services.AddSingleton<IHtmlLocalizerFactory, YamlHtmlLocalizerFactory>();

            services.AddSingleton<TransmissionDocumentsService>();
            services.AddSingleton<IDocLib>(DocLib.Instance);

            services.AddHsts(options =>
            {
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            var hosts = Configuration["AllowedHosts"]?
                   .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (hosts?.Length > 0)
            {
                services.Configure<HostFilteringOptions>(options => options.AllowedHosts = hosts);
            }
            if (bool.TryParse(Configuration["estProduction"], out var estProd) && !estProd)
            {
                // Ajout d'un document 
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "FRW.PR.Extra",
                        Version = "v1",
                        Description = "Service PR FRW.",
                        Contact = new OpenApiContact() { Name = "�quipe DTN" }
                    });

                    // Set the comments path for the Swagger JSON and UI.
                    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    var xmlPath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".xml");
                    c.IncludeXmlComments(xmlPath);
                    c.OperationFilter<JsonContentFilter>();
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Actions sp�cifiques pour le developpement/prod
            if (!Configuration.GetValue<bool>("estProduction"))
                app.UseDevcfgMiddleware();
            else
                app.UseHsts();


            // Page d'exception
            if (!Configuration.GetValue<bool>("estProduction") && Configuration.GetValue<bool>("ForceUseDeveloperExceptionPage"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseWhen(x => x.Request.Path.Value.Contains("/api/"), builder =>
                {
                    builder.UseFrwApiExceptionHandler();
                });

                app.UseWhen(x => x.Request.Path.Value.StartsWith("/en/") && !x.Request.Path.Value.Contains("/api/"), builder =>
                {
                    builder.UseExceptionHandler("/en/Error");
                });

                app.UseWhen(x => !x.Request.Path.Value.StartsWith("/en/") && !x.Request.Path.Value.Contains("/api/"), builder =>
                {
                    builder.UseExceptionHandler("/fr/Error");
                });
            }

            app.UseHostFiltering();
            app.UseRequestLocalization();
            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    var headers = context.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(30)
                    };
                }
            });
            app.UseFileServer();
            app.UseStatusCodePagesWithReExecute("/PageIntrouvable");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            if (!Configuration.GetValue<bool>("estProduction"))
            {
                // G�n�rateur Swagger
                app.UseSwagger(e => e.SerializeAsV2 = true);

                // Swagger-ui (HTML, JS, CSS, etc.), 
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FRW.PR.Extra");
                });
            }


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/offline.htm", async context =>
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync($"Serveur: {Environment.MachineName}\r\nDorsale: {Configuration.GetValue<string>("FRW:UrlDorsale")}\r\nX-NLB: {context.Request.Headers["X-NLB"]}\r\nX-NLB2: {context.Request.Headers["X-NLB2"]}");
                });

                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
