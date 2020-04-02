using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using CrossCutting;
using Doppler.Sap.Job.Service;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.DopplerSapService;
using Doppler.Sap.Job.Service.Settings;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;

namespace Doppler.Service.Job.Server
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
            services.AddHangfire(x =>
            {
                x.UseSQLiteStorage();
            });

            var httpClientPolicies = new HttpClientPoliciesSettings();
            Configuration.GetSection("HttpClient:Client").Bind(httpClientPolicies);
            services.AddSingleton(httpClientPolicies);

            var handlerHttpClient = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Automatic,
                SslProtocols = SslProtocols.Tls12
            };

            services.AddHttpClient(httpClientPolicies.ClientName, c => { })
                .ConfigurePrimaryHttpMessageHandler(() => handlerHttpClient)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy(httpClientPolicies.Policies.RetryAttemps));

            var dopplerCurrencySettings = new DopplerCurrencyServiceSettings
            {
                CurrencyCodeList = Configuration.GetSection("DopplerCurrencyServiceSettings:CurrencyCode")
                    .Get<List<string>>()
            };
            Configuration.GetSection(nameof(DopplerCurrencyServiceSettings)).Bind(dopplerCurrencySettings);
            services.AddSingleton(dopplerCurrencySettings);

            var jobsConfig = new TimeZoneJobConfigurations
            {
                TimeZoneJobs = Configuration["TimeZoneJobs"]
            };
            services.AddSingleton(jobsConfig);

            services.AddTransient<DopplerCurrencyService>();

            services.Configure<DopplerSapServiceSettings>(Configuration.GetSection(nameof(DopplerSapServiceSettings)));

            services.AddTransient<DopplerSapService>();

            ConfigureJob(services);
            ConfigureJobsScheduler(services, jobsConfig);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retry)
        {
            HttpStatusCode[] httpStatusCodesWorthRetrying =
            {
                HttpStatusCode.RequestTimeout,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.BadGateway,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.GatewayTimeout
            };
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryAsync(retry, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            var jobScheduler = (JobScheduler)app.ApplicationServices.GetService(typeof(JobScheduler));
            jobScheduler.ScheduleJobs();
        }

        private void ConfigureJob(IServiceCollection services)
        {
            var jobConfig = new RecurringJobConfiguration
            {
                Identifier = Configuration["Jobs:SapServiceJob:Identifier"],
                IntervalCronExpression = Configuration["Jobs:SapServiceJob:IntervalCronExpression"]
            };

            services.AddTransient(sp => new DopplerSapJob(
                sp.GetService<ILogger<DopplerSapJob>>(),
                jobConfig.IntervalCronExpression, 
                jobConfig.Identifier,
                services.BuildServiceProvider().GetService<DopplerCurrencyService>(),
                services.BuildServiceProvider().GetService<DopplerSapService>()));
        }

        private static void ConfigureJobsScheduler(IServiceCollection services, TimeZoneJobConfigurations jobsConfig)
        {
            services.AddTransient(sp =>
                new JobScheduler(new List<IRecurringJob>
                    {
                        services.BuildServiceProvider().GetService<DopplerSapJob>()
                    },
                    sp.GetRequiredService<ILogger<JobScheduler>>(),
                    jobsConfig)
            );
        }
    }
}
