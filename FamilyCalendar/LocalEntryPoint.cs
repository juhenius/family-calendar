namespace FamilyCalendar;

public class Program
{
  public static void Main(string[] args)
  {
    DotNetEnv.Env.NoClobber().Load();
    CreateHostBuilder(args).Build().Run();
  }

  public static IHostBuilder CreateHostBuilder(string[] args)
  {
    return Host.CreateDefaultBuilder(args)
      .ConfigureWebHostDefaults(builder =>
      {
        builder.UseStartup<Startup>();
      });
  }
}