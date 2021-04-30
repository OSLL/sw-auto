using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using WebPaperAnalyzer.Models;
using PaperAnalyzer;
using PaperAnalyzer.Service;
using WebPaperAnalyzer.DAL;
using WebPaperAnalyzer.Services;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;
using System;
using Hangfire.Dashboard;

namespace TestWebApp
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
            //services.AddTransient<ApplicationContext>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Account/Login");
                    options.AccessDeniedPath = new PathString("/Account/Login");
                });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<ResultScoreSettings>(Configuration.GetSection(nameof(ResultScoreSettings)));
            services.Configure<MongoSettings>(Configuration.GetSection(nameof(MongoSettings)));
            services.AddTransient<DbInitializer>();
            services.AddHttpClient();
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddSingleton<IResultRepository, ResultRepository>();
            services.AddTransient<IPaperAnalyzerService, PaperAnalyzerService>();
            services.AddTransient<IPaperAnalyzer, PapersAnalyzerRemake>();

            services.AddSingleton<IMLSAnalysisService, MLSAnalysisService>();

            services.AddSingleton<IPaperAnalyzerEnvironment, PaperAnalyzerEnvironment>();

            // TODO: get mongo connection string from configuration file
            var connectionString = Configuration.GetSection(nameof(MongoSettings)).Get<MongoSettings>().ConnectionString;
            MongoClient client = new MongoClient(connectionString);
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseMaxArgumentSizeToRender(99999)
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(client, "hangfire", new MongoStorageOptions
                {
                    InvisibilityTimeout = TimeSpan.FromMinutes(5),
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    },
                    Prefix = "hangfire.mongo",
                    CheckConnection = true
                })
            );

            services.AddHangfireServer(serverOptions =>
            {
                serverOptions.WorkerCount = 5;
                serverOptions.ServerName = "Hangfire.Mongo server";
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IHostingEnvironment env, IRecurringJobManager
                      recurringJobs)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            env.ConfigureNLog("NLog.config");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            DashboardOptions options = new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            };
            app.UseHangfireDashboard("/hangfire", options);
            recurringJobs.AddOrUpdate<DbInitializer>("remove-old-results",
                    (dbInitializer) => dbInitializer.RemoveOldResults(),
                    Cron.Hourly
                );
        }
    }
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole("admin");
        }
    }
}