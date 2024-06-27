using Amazon.DynamoDBv2;
using FamilyCalendar.Common;
using FamilyCalendar.Entries;
using Microsoft.SemanticKernel;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FamilyCalendar;

public class Startup(IConfiguration configuration)
{
  public IConfiguration Configuration { get; } = configuration;

  public void ConfigureServices(IServiceCollection services)
  {
    services.Configure<FamilyCalendarSettings>(Configuration.GetSection("FamilyCalendar"));

    AddOpenAIChatCompletion(services);
    services.AddRazorPages();
    services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
    services.AddSingleton<IEntryRepository, EntryRepository>();
    services.AddSingleton<IPartialViewRenderer, PartialViewRenderer>();
    services.AddSingleton<IEntryParser, OpenAiEntryParser>();
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

  private void AddOpenAIChatCompletion(IServiceCollection services)
  {
    var openAiModelId = Configuration.GetSection("FamilyCalendar:OpenAiModelId").Get<string>();
    var openAiApiKey = Configuration.GetSection("FamilyCalendar:OpenAiApiKey").Get<string>();

    if (openAiModelId == null)
    {
      throw new ArgumentException("invalid FamilyCalendar:OpenAiModelId setting");
    }

    if (openAiApiKey == null)
    {
      throw new ArgumentException("invalid FamilyCalendar:OpenAiApiKey setting");
    }

    services.AddOpenAIChatCompletion(openAiModelId, openAiApiKey);
  }
}
