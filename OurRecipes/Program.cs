using AspNetCoreHero.ToastNotification;
using AspNetCoreRateLimit;
using Humanizer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using OurRecipes.Data;
using Rotativa.AspNetCore;
using Sieve.Services;
using System.Configuration;
using System.Text.Json.Serialization;

namespace OurRecipes
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMvc().AddNToastNotifyNoty();

            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            //  builder.Services.AddInMemoryRateLimiting();

            // Add services to the container.

            //  builder.Services.AddAuthentication(options =>
            //  {
            //      options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //      options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            //  })
            //.AddCookie()
            //.AddGoogle(googleOptions =>
            // {
            // googleOptions.ClientId = "";
            // googleOptions.ClientSecret = "";
            // });

           
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<SieveProcessor>();
            builder.Services.AddMvc(options =>
            {
                options.MaxModelValidationErrors = 50;
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "The field is required.");
            })
  .AddViewOptions(options =>
  {
      options.HtmlHelperOptions.ClientValidationEnabled = true; // Ensure client validation is enabled
  });
            builder.Services.AddDbContext<AppDbContext>(x =>
            x.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")).LogTo(Console.WriteLine));
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
            builder.Services.AddRazorPages();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
            });
            builder.Services.AddAutoMapper(typeof(Program));
          
            var app = builder.Build();
            RotativaConfiguration.Setup(app.Environment.WebRootPath, @"C:\\Program Files\\wkhtmltopdf\\bin");
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // app.UseExceptionHandler("/Users/Error");
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

          
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseIpRateLimiting();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseNToastNotify();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
          
            app.Run();
        }
    }
}