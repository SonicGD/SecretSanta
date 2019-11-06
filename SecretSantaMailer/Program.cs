using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SecretSantaMailer
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            using (var host = CreateHostBuilder(args).Build())
            {
                await host.StartAsync();

                var sender = host.Services.GetRequiredService<Sender>();
                await sender.SendAsync();

                await host.StopAsync();
                await host.WaitForShutdownAsync();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => { builder.AddJsonFile("./config.json"); })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

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
            var smtpConfig = new SMTPMailConfig
            {
                Host = Configuration["SANTA_SMTP_HOST"],
                Port = int.Parse(Configuration["SANTA_SMTP_PORT"]),
                FromEmail = Configuration["SANTA_SMTP_FROM_EMAIL"],
                UserName = Configuration["SANTA_SMTP_USER_NAME"],
                Password = Configuration["SANTA_SMTP_PASSWORD"],
                UseTLS = bool.Parse(Configuration["SANTA_SMTP_USE_TLS"])
            };
            services.AddSingleton(smtpConfig);
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHttpContextAccessor();
            services.AddSingleton<MailSender<Participant>>();
            services.AddControllersWithViews();
            services.AddSingleton<ViewRenderService>();
            services.AddSingleton<Sender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}
