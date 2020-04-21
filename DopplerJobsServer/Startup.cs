using CrossCutting;
using CrossCutting.DopplerSapService;
using CrossCutting.DopplerSapService.Settings;
using Doppler.Billing.Job;
using Doppler.Billing.Job.Database;
using Doppler.Billing.Job.Settings;
using Doppler.Currency.Job;
using Doppler.Currency.Job.DopplerCurrencyService;
using Doppler.Currency.Job.Settings;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;

namespace Doppler.Jobs.Server
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
                .AddTransientHttpErrorPolicy(builder => GetRetryPolicy(httpClientPolicies.Policies.RetryAttemps));

            services.Configure<DopplerCurrencyServiceSettings>(Configuration.GetSection(nameof(DopplerCurrencyServiceSettings)));

            var jobsConfig = new TimeZoneJobConfigurations
            {
                TimeZoneJobs = TimeZoneHelper.GetTimeZoneByOperativeSystem(Configuration["TimeZoneJobs"])
            };
            services.AddSingleton(jobsConfig);
            services.AddTransient<IDopplerCurrencyService, DopplerCurrencyService>();

            services.Configure<DopplerSapConfiguration>(Configuration.GetSection(nameof(DopplerSapConfiguration)));
            services.AddTransient<IDopplerSapService, DopplerSapService>();

            services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
            services.Configure<DopplerBillingJobSettings>(Configuration.GetSection("Jobs:DopplerBillingJobSettings"));
            services.AddTransient<IDopplerRepository, DopplerRepository>();

            ConfigureJobsScheduler();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = 1
            });
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

        private void ConfigureJobsScheduler()
        {
            JobStorage.Current = new SQLiteStorage("Hangfire.db");
            
            var tz = TimeZoneHelper.GetTimeZoneByOperativeSystem(Configuration["TimeZoneJobs"]);

            RecurringJob.AddOrUpdate<DopplerBillingJob>(
                Configuration["Jobs:DopplerBillingJobSettings:Identifier"],
                job => job.Run(),
                Configuration["Jobs:DopplerBillingJobSettings:IntervalCronExpression"],
                TimeZoneInfo.FindSystemTimeZoneById(tz));

            RecurringJob.AddOrUpdate<DopplerCurrencyJob>(
                Configuration["Jobs:DopplerCurrencyJob:Identifier"],
                job => job.Run(),
                Configuration["Jobs:DopplerCurrencyJob:IntervalCronExpression"],
                TimeZoneInfo.FindSystemTimeZoneById(tz));
        }
    }
}
