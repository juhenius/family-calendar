using Amazon.Lambda.AspNetCoreServer;

namespace FamilyCalendar;

public class LambdaEntryPoint : APIGatewayHttpApiV2ProxyFunction
{
  protected override void Init(IWebHostBuilder builder)
  {
    builder.UseStartup<Startup>();
  }

}