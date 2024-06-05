using Amazon.DynamoDBv2;
using FamilyCalendar.Common;
using FamilyCalendar.Entries;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FamilyCalendar;

public class Startup(IConfiguration configuration)
{
  public IConfiguration Configuration { get; } = configuration;

  public void ConfigureServices(IServiceCollection services)
  {
    services.Configure<FamilyCalendarSettings>(Configuration.GetSection("FamilyCalendar"));

    services.AddRazorPages();
    services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
    services.AddSingleton<IEntryRepository, EntryRepository>();
    services.AddSingleton<IPartialViewRenderer, PartialViewRenderer>();
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
      .AddCookie(options =>
      {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
      });
    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseExceptionHandler("/Error");
      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapRazorPages();
    });
  }
}
