using IotPlatform.Api.Busi.Logic.Common;
using IotPlatform.Api.Busi.Logic.EdgeDriver;
using IotPlatform.Api.Busi.Logic.Enums;
using IotPlatform.Api.Repository.Driver;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api;

public class DriverRunBackgroundService(IServiceScopeFactory scopeFactory,ILogger<DriverRunBackgroundService> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = scopeFactory.CreateScope())
        {
            var iconfig = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbConnectionString = iconfig.GetConnectionString("contentplatform-db"); // 或者 _configuration["ConnectionStrings:Database"]
            logger.LogInformation("链接字符串："+dbConnectionString);
            // Resolve the Scoped service from the scope's service provider
            var driverRepository = scope.ServiceProvider.GetRequiredService<IDriverRepository>();
            var edgeDriverResolver = scope.ServiceProvider.GetRequiredService<Constants.EdgeDriverResolver>();
            var edgeDriverFactory = scope.ServiceProvider.GetRequiredService<IEdgeDriverFactory>();

            var drivers = await driverRepository.GetQuery().ToListAsync();
            foreach (var driver in drivers)
            {
                var edgeDriver = edgeDriverResolver((DriverTypeEnum)driver.DriverType);
                edgeDriver.Run(driver);
                edgeDriverFactory.GetDrivers().Add(driver.DriverCode, edgeDriver);
            }
        }
    }
}